using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Game.Simulation.Skills;
using Orion.Game.Simulation.Components;
using FactionComponent = Orion.Game.Simulation.Components.FactionMembership;

namespace Orion.Game.Simulation
{
    /// <summary>
    /// Abstract base class for game objects present in the game world.
    /// </summary>
    [Serializable]
    public class Entity
    {
        #region Instance
        #region Fields
        private readonly World world;
        private readonly Handle handle;
        private bool isDead;

        private readonly List<Component> components = new List<Component>();
        #endregion

        #region Constructors
        public Entity(World world, Handle handle)
        {
            Argument.EnsureNotNull(world, "world");
            this.world = world;
            this.handle = handle;
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when this <see cref="Entity"/> dies.
        /// </summary>
        public event Action<Entity> Died;

        /// <summary>
        /// Raised when the <see cref="Entity"/> moves.
        /// </summary>
        public event ValueChangedEventHandler<Entity, Vector2> Moved;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the <see cref="World"/> which contains this <see cref="Entity"/>.
        /// </summary>
        public World World
        {
            get { return world; }
        }

        /// <summary>
        /// Gets the handle of this <see cref="Entity"/>.
        /// This should be the same for all representations
        /// of this <see cref="Entity"/> across network games.
        /// </summary>
        public Handle Handle
        {
            get { return handle; }
        }

        #region Location/Size
        /// <summary>
        /// Gets the size of this <see cref="Entity"/>, in tiles.
        /// This value is garanteed to remain constant.
        /// </summary>
#warning Temporary hack until components take over
        public virtual Size Size
        {
            get { return GetComponent<Position>().Size; }
        }

        /// <summary>
        /// Gets the width of this <see cref="Entity"/>, in tiles.
        /// This value is garantee to remain constant.
        /// </summary>
        public int Width
        {
            get { return Size.Width; }
        }

        /// <summary>
        /// Gets the heigh tof this <see cref="Entity"/>, in tiles.
        /// This value is garantee to remain constant.
        /// </summary>
        public int Height
        {
            get { return Size.Height; }
        }

        /// <summary>
        /// Gets the position of the origin of this <see cref="Entity"/>.
        /// </summary>
        public Vector2 Position
        {
            get { return GetPosition(); }
        }

        /// <summary>
        /// Gets the position of the center of this <see cref="Entity"/>.
        /// </summary>
        public Vector2 Center
        {
            get { return new Vector2(Position.X + Size.Width * 0.5f, Position.Y + Size.Height * 0.5f); }
        }

        /// <summary>
        /// Gets a <see cref="Rectangle"/> that bounds the physical representation of this <see cref="Entity"/>.
        /// </summary>
        public Rectangle BoundingRectangle
        {
            get { return new Rectangle(Position.X, Position.Y, Size.Width, Size.Height); }
        }

        /// <summary>
        /// Gets the rectangle representing the part of this entity that can be collided with.
        /// </summary>
        public Rectangle CollisionRectangle
        {
            get { return GetCollisionRectangle(BoundingRectangle); }
        }

        /// <summary>
        /// Gets the region of the world grid occupied by this <see cref="Entity"/>.
        /// </summary>
        public Region GridRegion
        {
            get { return GetGridRegion(Position, Size); }
        }
        #endregion

        /// <summary>
        /// Gets the <see cref="CollisionLayer"/> on which this
        /// <see cref="Entity"/> lies. This should never change
        /// in the lifetime of the <see cref="Entity"/>.
        /// </summary>
#warning Temporary hack until components take over
        public virtual CollisionLayer CollisionLayer
        {
            get { return GetComponent<Position>().CollisionLayer; }
        }

        /// <summary>
        /// Gets a value indicating if this <see cref="Entity"/> is alive.
        /// </summary>
        public bool IsAlive
        {
            get { return !isDead; }
        }

        /// <summary>
        /// Gets a value indicating if this entity is alive and in the world.
        /// An entity is out of the world when it has died or when it is temporarily
        /// not interactible with (such as a unit being transported).
        /// </summary>
        public virtual bool IsAliveInWorld
        {
            get { return IsAlive; }
        }
        #endregion

        #region Methods
        #region Components
        public bool HasComponent<T>() where T : Component
        {
            return components.OfType<T>().Count() > 0;
        }

        public bool HasComponent(Type componentType)
        {
            return components.Count(c => c.GetType() == componentType) > 0;
        }

        public T GetComponent<T>() where T : Component
        {
            return components.OfType<T>().First();
        }

        public T GetComponentOrNull<T>() where T : Component
        {
            return components.OfType<T>().FirstOrDefault();
        }

        public IEnumerable<Component> GetComponents()
        {
            return components;
        }

        public void AddComponent(Component component)
        {
            Type componentType = component.GetType();
            if (components.Count(c => c.GetType() == componentType) > 0)
                throw new ArgumentException("component");
            components.Add(component);
        }

        public void RemoveComponent<T>() where T : Component
        {
            Component instance = GetComponent<T>();
            components.Remove(instance);
        }

        public Stat GetStat(EntityStat stat)
        {
            if (stat.NumericType == StatType.Integer)
                return new Stat(components.Sum(c => c.GetStatBonus(stat).IntegerValue));
            else
                return new Stat(components.Sum(c => c.GetStatBonus(stat).RealValue));
        }
        #endregion

        internal void RaiseWarning(string warning)
        {
            FactionMembership factionComponent = GetComponentOrNull<FactionComponent>();
            if (factionComponent == null)
                Debug.WriteLine(warning);
            else
                factionComponent.Faction.RaiseWarning(warning);
        }

        public sealed override int GetHashCode()
        {
            return handle.GetHashCode();
        }

        public override string ToString()
        {
            return "Entity {0}".FormatInvariant(handle);
        }

        public void Die()
        {
            if (isDead)
            {
                Debug.Fail("{0} attempted to die twice.".FormatInvariant(this));
                return;
            }

            isDead = true;
            OnDied();
        }

        protected virtual void OnDied()
        {
            Died.Raise(this);
            World.OnEntityDied(this);
        }

        protected virtual void OnMoved(Vector2 oldPosition, Vector2 newPosition)
        {
#if DEBUG
            if (isDead)
            {
                // #if'd so the FormatInvariant is not executed in release.
                Debug.Fail("{0} is dead and yet moves.".FormatInvariant(this));
            }
#endif
            var handler = Moved;
            if (handler != null) handler(this, oldPosition, newPosition);
            world.OnEntityMoved(this, oldPosition, newPosition);
        }

        /// <summary>
        /// Updates this <see cref="Entity"/> for a frame.
        /// </summary>
        /// <param name="step">Information on this simulation step.</param>
        /// <remarks>
        /// Invoked by <see cref="EntityManager"/>.
        /// </remarks>
        internal void Update(SimulationStep step)
        {
            if (!IsAliveInWorld)
            {
                Debug.Fail("{0} was updated when it wasn't alive and in the world.".FormatInvariant(this));
                return;
            }
                
            DoUpdate(step);
        }

#warning Temporary hack until components take over
        protected virtual Vector2 GetPosition()
        {
            return GetComponent<Position>().Location;
        }

        protected virtual void DoUpdate(SimulationStep step) { }
        #endregion
        #endregion

        #region Static
        #region Methods
        /// <summary>
        /// The maximum size (in width or height) of entities.
        /// This limitation exists to optimize the EntityZoneManager.
        /// </summary>
        public static readonly int MaxSize = 4;

        public static Region GetGridRegion(Vector2 position, Size size)
        {
            Point min = new Point((int)Math.Round(position.X), (int)Math.Round(position.Y));
            return new Region(min, size);
        }

        public static Rectangle GetCollisionRectangle(Rectangle boundingRectangle)
        {
            return Rectangle.FromCenterSize(
                boundingRectangle.CenterX, boundingRectangle.CenterY,
                boundingRectangle.Width - 0.2f, boundingRectangle.Height - 0.2f);
        }
        #endregion
        #endregion
    }
}
