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
            if (Moved != null)
            {
                var eventArgs = new ValueChangedEventArgs<Vector2>(oldPosition, newPosition);
                Moved(this, eventArgs);
            }
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
        /// Gets the position of the center of this <see cref="Entity"/>.
        /// </summary>
        public abstract Vector2 Position { get; }

        /// <summary>
        /// Gets a <see cref="Rectangle"/> that bounds the physical representation of this <see cref="Entity"/>.
        /// </summary>
        public Rectangle BoundingRectangle
        {
            get { return Rectangle.FromCenterSize(Position.X, Position.Y, Size.Width, Size.Height); }
        }
        #endregion

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
    }
}
