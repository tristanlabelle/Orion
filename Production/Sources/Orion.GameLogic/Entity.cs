using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Geometry;
using OpenTK.Math;

namespace Orion.GameLogic
{
    /// <summary>
    /// Abstract base class for game objects present in the game world.
    /// </summary>
    [Serializable]
    public abstract class Entity
    {
        #region Instance
        #region Fields
        private readonly World world;
        private readonly Handle handle;
        private bool isDead;
        #endregion

        #region Constructors
        protected Entity(World world, Handle handle)
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
        public event GenericEventHandler<Entity> Died;

        private void OnDied()
        {
            var handler = Died;
            if (handler != null) handler(this);
        }

        /// <summary>
        /// Raised when the <see cref="Entity"/> moves.
        /// </summary>
        public event ValueChangedEventHandler<Entity, Vector2> Moved;

        protected virtual void OnMoved(Vector2 oldPosition, Vector2 newPosition)
        {
            var handler = Moved;
            if (handler != null) handler(this, oldPosition, newPosition);
        }
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
        /// This value is garantee to remain constant.
        /// </summary>
        public abstract Size Size { get; }

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
        public abstract Vector2 Position { get; }

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

        public Region GridRegion
        {
            get { return GetGridRegion(Position, Size); }
        }
        #endregion

        /// <summary>
        /// Gets a value indicating if this <see cref="Entity"/> is solid.
        /// Solid entities occupy grid space and cannot overlap with other
        /// solid entities or with the terrain.
        /// </summary>
        public abstract bool IsSolid { get; }

        /// <summary>
        /// Gets a value indicating if this <see cref="Entity"/> is alive.
        /// </summary>
        public bool IsAlive
        {
            get { return !isDead; }
        }
        #endregion

        #region Methods
        public sealed override int GetHashCode()
        {
            return handle.GetHashCode();
        }

        public override string ToString()
        {
            return "Entity {0}".FormatInvariant(handle);
        }

        protected void Die()
        {
            if (isDead) return;
            isDead = true;
            OnDied();
        }

        internal virtual void Update(float timeDeltaInSeconds) { }
        #endregion
        #endregion

        #region Static
        #region Methods
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
