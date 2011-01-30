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
    public class Position : Component
    {
        #region Static
        public static readonly EntityStat SightRangeStat = new EntityStat(typeof(Position), StatType.Real, "SightRange", "Portée de vision");
        #endregion

        #region Fields
        private CollisionLayer collisionLayer;
        private float sightRange;
        private Size size;

        private Vector2 location;
        private float angle;
        #endregion

        #region Constructors
        public Position(Entity entity) : base(entity) { }
        #endregion

        #region Events
        public event Action<Position, Vector2, Vector2> Moved;
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

        public Vector2 Location
        {
            get { return location; }
            set
            {
                World world = Entity.World;
                if (!world.Bounds.ContainsPoint(value))
                {
                    Debug.Fail("Position out of bounds.");
                    value = world.Bounds.Clamp(value);
                }
                var oldPosition = location;
                location = value;
                Moved.Raise(this, oldPosition, location);
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
                return new Vector2(Location.X + size.Width * 0.5f, Location.Y + size.Height * 0.5f);
            }
        }

        [Transient]
        public Rectangle BoundingRectangle
        {
            get
            {
                return new Rectangle(Location.X, Location.Y, size.Width, size.Height);
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
                Point min = new Point((int)Math.Round(location.X), (int)Math.Round(location.Y));
                return new Region(min, size);
            }
        }
        #endregion
    }
}
