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
    public class Spatial : Component
    {
        #region Static
        public static readonly EntityStat SightRangeStat = new EntityStat(typeof(Spatial), StatType.Real, "SightRange", "Portée de vision");
        #endregion

        #region Fields
        private CollisionLayer collisionLayer;
        private float sightRange;
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
        public float Angle
        {
            get { return angle; }
            set { angle = value; }
        }

        [Mandatory]
        public float SightRange
        {
            get { return sightRange; }
            set { sightRange = value; }
        }

        [Transient]
        public Circle LineOfSight
        {
            get { return new Circle(Center, Entity.GetStat(SightRangeStat).RealValue); }
        }

        [Mandatory]
        public CollisionLayer CollisionLayer
        {
            get { return collisionLayer; }
            set { collisionLayer = value; }
        }

        public Vector2 Position
        {
            get { return position; }
            set
            {
                World world = Entity.World;
                if (!world.Bounds.ContainsPoint(value))
                {
                    Debug.Fail("Position out of bounds.");
                    value = world.Bounds.Clamp(value);
                }
                var oldPosition = position;
                position = value;
                Moved.Raise(this, oldPosition, position);
            }
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
            get
            {
                return new Vector2(Position.X + size.Width * 0.5f, Position.Y + size.Height * 0.5f);
            }
        }

        [Transient]
        public Rectangle BoundingRectangle
        {
            get
            {
                return new Rectangle(Position.X, Position.Y, size.Width, size.Height);
            }
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
    }
}
