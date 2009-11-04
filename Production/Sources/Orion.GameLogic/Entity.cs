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
        private readonly int id;
        #endregion

        #region Constructors
        protected Entity(World world, int id)
        {
            Argument.EnsureNotNull(world, "world");
            this.world = world;
            this.id = id;
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when this <see cref="Entity"/> dies.
        /// </summary>
        public event GenericEventHandler<Entity> Died;

        protected virtual void OnDied()
        {
            if (Died != null) Died(this);
        }

        /// <summary>
        /// Raised when the bounding rectangle of this <see cref="Entity"/> changes.
        /// </summary>
        public event ValueChangedEventHandler<Entity, Rectangle> BoundingRectangleChanged;

        protected virtual void OnBoundingRectangleChanged(
            Rectangle oldBoundingRectangle, Rectangle newBoundingRectangle)
        {
            if (BoundingRectangleChanged != null)
                BoundingRectangleChanged(this, new ValueChangedEventArgs<Rectangle>(oldBoundingRectangle, newBoundingRectangle));
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
        /// Gets the unique identifier of this <see cref="Entity"/>.
        /// This should be the same for all representations
        /// of this <see cref="Entity"/> in network games.
        /// </summary>
        public int ID
        {
            get { return id; }
        }

        /// <summary>
        /// Gets a <see cref="Rectangle"/> that bounds the physical representation of this <see cref="Entity"/>.
        /// </summary>
        public abstract Rectangle BoundingRectangle { get; }
        #endregion

        #region Methods
        public sealed override int GetHashCode()
        {
            return id;
        }

        public override string ToString()
        {
            return "Entity #{0}".FormatInvariant(id);
        }

        internal virtual void Update(float timeDeltaInSeconds) { }
        #endregion
    }
}
