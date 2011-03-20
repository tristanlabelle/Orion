﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Engine.Geometry;
using Orion.Game.Simulation.Components;
using FactionComponent = Orion.Game.Simulation.Components.FactionMembership;
using System.Collections.Generic;

namespace Orion.Game.Simulation
{
    /// <summary>
    /// Abstract base class for every entity in the game world,
    /// including units, resource nodes, fauna, etc.
    /// </summary>
    [Serializable]
    public partial class Entity
    {
        #region Instance
        #region Fields
        private readonly World world;
        private readonly Handle handle;
        private bool isDead;

        private readonly ComponentCollection components;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructs a prototype entity.
        /// </summary>
        /// <remarks>
        /// Prototype entities don't belong to the world and all have the handle '0'. They exist only to be cloned into existence.
        /// </remarks>
        public Entity(Handle handle)
        {
            this.handle = handle;
            this.components = new ComponentCollection(this);
        }

        public Entity(World world, Handle handle)
            : this(handle)
        {
            Argument.EnsureNotNull(world, "world");
            this.world = world;
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

        #region Components
        /// <summary>
        /// Gets the collection of this <see cref="Entity">entity's</see> <see cref="Component">components</see>.
        /// <see cref="Component">Components</see> are the building blocks of an <see cref="Entity">entity's</see> behaviour.
        /// </summary>
        public ComponentCollection Components
        {
            get { return components; }
        }

        /// <summary>
        /// Gets the <see cref="T:Identity"/> component of this <see cref="Entity"/>.
        /// If there is none, returns <c>null</c>.
        /// </summary>
        /// <remarks>
        /// This property is provided as a convenence because the identity component is often needed.
        /// </remarks>
        public Identity Identity
        {
            get { return Components.TryGet<Identity>(); }
        }

        /// <summary>
        /// Gets the <see cref="T:Spatial"/> component of this <see cref="Entity"/>.
        /// If there is none, returns <c>null</c>.
        /// </summary>
        /// <remarks>
        /// This property is provided as a convenence because the spatial component is often needed.
        /// </remarks>
        public Spatial Spatial
        {
            get { return components.TryGet<Spatial>(); }
        }
        #endregion

        #region Location/Size
        /// <summary>
        /// Gets the size of this <see cref="Entity"/>, in tiles.
        /// This value is garanteed to remain constant.
        /// </summary>
        public Size Size
        {
            get { return Spatial.Size; }
        }

        /// <summary>
        /// Gets the position of the origin of this <see cref="Entity"/>.
        /// </summary>
        public Vector2 Position
        {
            get { return Spatial.Position; }
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
        /// Gets the region of the world grid occupied by this <see cref="Entity"/>.
        /// </summary>
        public Region GridRegion
        {
            get { return GetGridRegion(Position, Size); }
        }
        #endregion

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
        public bool IsAliveInWorld
        {
            get { return IsAlive && Components.Has<Spatial>(); }
        }

        /// <summary>
        /// Gets a value indicating if heavier operations, such as detecting if there is an enemy
        /// to attack within a range, can be performed during this frame.
        /// This property is <c>true</c> every few frames, allowing to distribute computational work in time.
        /// </summary>
        public bool CanPerformHeavyOperation
        {
            get
            {
                const int frameDelta = 8;
                // The value of the entity's handle is taken into account so that
                // not all entities perform heavier operations on the same frame.
                return (world.LastSimulationStep.Number + (int)handle.Value) % frameDelta == 0;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets the value of a given <see cref="Stat"/> for this <see cref="Entity"/>.
        /// </summary>
        /// <param name="stat">The <see cref="Stat"/> for which the value is to be found.</param>
        /// <returns>The value associated with that <see cref="Stat"/>.</returns>
        public StatValue GetStatValue(Stat stat)
        {
            Argument.EnsureNotNull(stat, "stat");

            StatValue sum = StatValue.CreateZero(stat.Type);

            // In the absence of its declaring component,
            // a stat can only have a value of zero.
            if (!Components.Has(stat.ComponentType)) return sum;

            foreach (Component component in components)
                sum += component.GetStatBonus(stat);

            return sum;
        }

        internal void RaiseWarning(string warning)
        {
            FactionMembership factionComponent = Components.TryGet<FactionComponent>();
            if (factionComponent == null)
                Debug.WriteLine(warning);
            else
                factionComponent.Faction.RaiseWarning(warning);
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
            TaskQueue taskQueue = Components.TryGet<TaskQueue>();
            if (taskQueue != null) taskQueue.Clear();

            Died.Raise(this);

            Faction faction = FactionMembership.GetFaction(this);
            if (faction != null) faction.OnEntityDied(this);

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

            // Components are copied to a temporary buffer before being updated
            // so any modifications to the component collection while iterating
            // does not raise collection modification during iteration exceptions.
            if (tempComponents == null) tempComponents = new List<Component>();
            else tempComponents.Clear();

            foreach (Component component in components)
                tempComponents.Add(component);

            try
            {
                foreach (Component component in tempComponents)
                    component.Update(step);
            }
            finally
            {
                tempComponents.Clear();
            }
        }

        #region Object Model
        public override int GetHashCode()
        {
            return handle.GetHashCode();
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            // Write the faction name, if any
            Faction faction = FactionMembership.GetFaction(this);
            if (faction != null)
            {
                stringBuilder.Append(faction.Name);
                stringBuilder.Append(' ');
            }

            // Write the stereotype name, or "Entity" if there's none
            Identity identity = Identity;
            stringBuilder.Append(identity == null ? "Entity" : identity.Name);
            stringBuilder.Append(' ');

            // Write the handle value
            stringBuilder.Append(handle);
            stringBuilder.Append(": ");
            
            // Write the component names
            foreach (string str in Components.Select(component => component.GetType().Name).Interleave(", "))
                stringBuilder.Append(str);

            return stringBuilder.ToString();
        }
        #endregion
        #endregion
        #endregion

        #region Static
        #region Methods
        [ThreadStatic]
        private static List<Component> tempComponents;

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
        #endregion
        #endregion
    }
}
