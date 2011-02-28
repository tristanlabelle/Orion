using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using OpenTK;
using Orion.Engine.Geometry;
using System.Diagnostics;
using Orion.Game.Simulation.Components.Serialization;

namespace Orion.Game.Simulation.Components
{
    /// <summary>
    /// Allows an <see cref="Entity"/> have a position and rotation in the world.
    /// </summary>
    public sealed class Spatial : Component
    {
        #region Fields
        private CollisionLayer collisionLayer;
        private Size size;

        private Vector2 position;
        private float angle;
        #endregion

        #region Constructors
        public Spatial(Entity entity) : base(entity) { }
        #endregion

        #region Events
        public event Action<Spatial, Vector2, Vector2> Moved;
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the position of this <see cref="Entity"/>, in world units.
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
            set
            {
                World world = Entity.World;
                if (world != null && !world.Bounds.ContainsPoint(value))
                {
                    Debug.Fail("Position out of bounds.");
                    value = world.Bounds.Clamp(value);
                }
                var oldPosition = position;
                position = value;
                Moved.Raise(this, oldPosition, position);
            }
        }

        /// <summary>
        /// Accesses the angle this <see cref="Entity"/> is oriented at, in radians.
        /// </summary>
        public float Angle
        {
            get { return angle; }
            set { angle = value; }
        }

        [Mandatory]
        public CollisionLayer CollisionLayer
        {
            get { return collisionLayer; }
            set { collisionLayer = value; }
        }

        [Mandatory]
        public Size Size
        {
            get { return size; }
            set { size = value; }
        }

        [Transient]
        public int Width
        {
            get { return size.Width; }
            set { size = new Size(value, size.Height); }
        }

        [Transient]
        public int Height
        {
            get { return size.Height; }
            set { size = new Size(size.Width, value); }
        }

        [Transient]
        public Vector2 Center
        {
            get { return new Vector2(Position.X + size.Width * 0.5f, Position.Y + size.Height * 0.5f); }
        }

        [Transient]
        public Rectangle BoundingRectangle
        {
            get { return new Rectangle(Position.X, Position.Y, size.Width, size.Height); }
        }

        [Transient]
        public Rectangle CollisionRectangle
        {
            get
            {
                return Rectangle.FromCenterSize(
                    BoundingRectangle.CenterX, BoundingRectangle.CenterY,
                    BoundingRectangle.Width - 0.2f, BoundingRectangle.Height - 0.2f);
            }
        }

        [Transient]
        public Region GridRegion
        {
            get
            {
                Point min = new Point((int)Math.Round(position.X), (int)Math.Round(position.Y));
                return new Region(min, size);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Rotates this <see cref="Entity"/> so that it faces a target.
        /// </summary>
        /// <param name="target">The location of the target to be faced.</param>
        public void LookAt(Vector2 target)
        {
            Vector2 delta = target - Center;
            if (delta.LengthSquared < 0.01f) return;

            Angle = (float)Math.Atan2(delta.Y, delta.X);
        }

        /// <summary>
        /// Tests if a given <see cref="Entity"/> is within a given range of this <see cref="Entity"/>.
        /// </summary>
        /// <param name="target">The target <see cref="Entity"/> to be tested.</param>
        /// <param name="radius">The radius of the range circle.</param>
        /// <returns>A value indicating if <paramref name="target"/> is in range.</returns>
        public bool IsInRange(Entity target, float radius)
        {
            Argument.EnsureNotNull(target, "target");

            Spatial targetSpatial = target.Spatial;
            return targetSpatial != null
                && Region.SquaredDistance(GridRegion, targetSpatial.GridRegion) <= radius * radius + 0.001f;
        }
        #endregion
    }
}
