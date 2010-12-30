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
        public static readonly EntityStat SightRangeStat = new EntityStat(typeof(Position), "SightRange", "Portée de vision");
        #endregion

        #region Fields
        private CollisionLayer collisionLayer;
        private Size size;
        private Vector2 position;
        private float angle;
        private float sightRange;
        #endregion

        #region Constructors
        public Position(Entity entity, float sightRange, CollisionLayer layer, Size size, Vector2 position)
            : base(entity)
        {
            this.sightRange = sightRange;
            this.collisionLayer = layer;
            this.size = size;
            this.position = position;
        }
        #endregion

        #region Events
        public event Action<Position, Vector2, Vector2> Moved;
        #endregion

        #region Properties
        public float Angle
        {
            get { return angle; }
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

        public Vector2 Center
        {
            get
            {
                return new Vector2(Position.X + Size.Width * 0.5f, Position.Y + Size.Height * 0.5f);
            }
        }

        public Rectangle BoundingRectangle
        {
            get { return new Rectangle(Position.X, Position.Y, Size.Width, Size.Height); }
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
                Point min = new Point((int)Math.Round(position.X), (int)Math.Round(position.Y));
                return new Region(min, size);
            }
        }
        #endregion
    }
}
