using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using OpenTK;
using Orion.Engine.Geometry;
using System.Diagnostics;

namespace Orion.Game.Simulation.Components
{
    public class Position : Component
    {
        #region Static
        public static readonly EntityStat<float> SightRangeStat = new EntityStat<float>(typeof(Position), "SightRange", "Portée de vision");
        #endregion

        #region Fields
        private CollisionLayer collisionLayer;
        private Size size;
        private float sightRange;

        private Vector2 location;
        private float angle;
        #endregion

        #region Constructors
        public Position(Entity entity, float sightRange, CollisionLayer layer, Size size)
            : base(entity)
        {
            this.sightRange = sightRange;
            this.collisionLayer = layer;
            this.size = size;
        }
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

        public float SightRange
        {
            get { return sightRange; }
        }

        public Circle LineOfSight
        {
            get { return new Circle(Center, Entity.GetStat(SightRangeStat)); }
        }

        public CollisionLayer CollisionLayer
        {
            get { return collisionLayer; }
        }

        public Size Size
        {
            get { return size; }
        }

        public int Width
        {
            get { return size.Width; }
        }

        public int Height
        {
            get { return size.Height; }
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

        public Vector2 Center
        {
            get
            {
                return new Vector2(Location.X + Size.Width * 0.5f, Location.Y + Size.Height * 0.5f);
            }
        }

        public Rectangle BoundingRectangle
        {
            get { return new Rectangle(Location.X, Location.Y, Size.Width, Size.Height); }
        }

        public Rectangle CollisionRectangle
        {
            get
            {
                return Rectangle.FromCenterSize(
                    BoundingRectangle.CenterX, BoundingRectangle.CenterY,
                    BoundingRectangle.Width - 0.2f, BoundingRectangle.Height - 0.2f);
            }
        }

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
