using System;

using OpenTK.Math;

namespace Orion.Geometry
{
    /// <summary>
    /// Defines a 2d circle as a central point and a radius
    /// </summary>
    [Serializable]
    public struct Circle : IEquatable<Circle>
    {
        #region Instance
        #region Fields
        private readonly Vector2 center;
        private readonly float radius;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance from the center point and radius of the circle.
        /// </summary>
        /// <param name="center">The point at the center of the circle.</param>
        /// <param name="radius">The radius of the circle.</param>
        public Circle(Vector2 center, float radius)
        {
            this.center = center;
            this.radius = Math.Abs(radius);
        }

        /// <summary>
        /// Initializes a new instance from the center point and radius of the circle.
        /// </summary>
        /// <param name="centerX">The X coordinate of the point at the center of the circle.</param>
        /// <param name="centerY">The Y coordinate of the point at the center of the circle.</param>
        /// <param name="radius">The radius of the circle.</param>
        public Circle(float centerX, float centerY, float radius)
            : this(new Vector2(centerX, centerY), radius) { }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the center point of this circle.
        /// </summary>
        public Vector2 Center
        {
            get { return center; }
        }

        /// <summary>
        /// Gets the radius of this circle.
        /// </summary>
        public float Radius
        {
            get { return radius; }
        }

        /// <summary>
        /// Gets the squared radius of this circle.
        /// </summary>
        public float SquaredRadius
        {
            get { return radius * radius; }
        }

        /// <summary>
        /// Gets the diameter of this circle.
        /// </summary>
        public float Diameter
        {
            get { return radius * 2; }
        }

        /// <summary>
        /// Gets the circumference of this circle.
        /// </summary>
        public float Circumference
        {
            get { return 2 * (float)Math.PI * radius; }
        }

        /// <summary>
        /// Gets the area of this circle.
        /// </summary>
        public float Area
        {
            get { return (float)Math.PI * SquaredRadius; }
        }

        /// <summary>
        /// Gets a rectangle that bounds this circle.
        /// </summary>
        public Rectangle BoundingRectangle
        {
            get { return new Rectangle(center.X - radius, center.Y - radius, Diameter, Diameter); }
        }
        #endregion

        #region Methods
        #region Point-related
        /// <summary>
        /// Gets the point of intersection of an angle with the circumference.
        /// </summary>
        /// <param name="angle">The angle.</param>
        /// <returns>The point of intersection with the circumference.</returns>
        public Vector2 PointFromAngle(float angle)
        {
            return new Vector2(center.X + (float)Math.Cos(angle) * radius,
                               center.Y + (float)Math.Sin(angle) * radius);
        }

        /// <summary>
        /// Gets a point within the circle that is the closest to another point.
        /// </summary>
        /// <param name="point">The point which's image is to be found.</param>
        /// <returns>The closets point to that point within the circle.</returns>
        public Vector2 ClosestPointInside(Vector2 point)
        {
            if (ContainsPoint(point)) return point;
            return Vector2.Normalize(point - center) * radius + center;
        }

        /// <summary>
        /// Tests if a point is within this circle.
        /// </summary>
        /// <param name="point">The point to be tested.</param>
        /// <returns>True if it is within the circle, false otherwise.</returns>
        public bool ContainsPoint(Vector2 point)
        {
            return (point - center).LengthSquared <= SquaredRadius;
        }

        /// <summary>
        /// Gets the point within this circle that is the farthest in a given direction.
        /// </summary>
        /// <param name="direction">The direction.</param>
        /// <returns>The point within the circle that is the farthest in a given direction.</returns>
        public Vector2 GetSupportPoint(Vector2 direction)
        {
            return center + Vector2.Normalize(direction) * radius;
        }
        #endregion

        #region Object Model
        public bool Equals(Circle other)
        {
            return center == other.center && radius == other.radius;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Circle)) return false;
            return Equals((Circle)obj);
        }

        public override int GetHashCode()
        {
            return center.GetHashCode() ^ radius.GetHashCode();
        }

        public override string ToString()
        {
            return "center: " + center.ToString() + ", radius: " + radius.ToStringInvariant();
        }
        #endregion
        #endregion
        #endregion

        #region Static
        #region Fields
        /// <summary>
        /// A circle centered at the origin with a radius of <c>0</c>.
        /// </summary>
        public static readonly Circle Zero = new Circle(0, 0, 0);

        /// <summary>
        /// A circle centered at the origin with a radius of <c>1</c>.
        /// </summary>
        public static readonly Circle Unit = new Circle(0, 0, 1);
        #endregion

        #region Methods
        #region Factory
        /// <summary>
        /// Gets a circle that contains two circles.
        /// </summary>
        /// <param name="circle1">The first circle.</param>
        /// <param name="circle2">The second circle.</param>
        /// <returns>The union of those circles, as a new circle containing both.</returns>
        public static Circle Union(Circle circle1, Circle circle2)
        {
            // TODO: Test Circle.Union
            // The center is the weighted average of the circle centers.
            float radiusSum = circle1.radius + circle2.radius;
            Vector2 center = (circle1.center * circle1.radius + circle2.center * circle2.radius) / radiusSum;
            return new Circle(center, radiusSum * 0.5f);
        }

        /// <summary>
        /// Gets the signed distance between two <see cref="Circle"/>.
        /// </summary>
        /// <param name="circle1">The first <see cref="Circle"/> to be tested..</param>
        /// <param name="circle2">The second <see cref="Circle"/> to be tested.</param>
        /// <returns>
        /// The signed distance between the two <see cref="Circle"/>s,
        /// a positive value if they do not overlap and a negative value if they do.
        /// </returns>
        public static float SignedDistance(Circle circle1, Circle circle2)
        {
            return (circle2.center - circle1.center).Length
                - circle1.radius - circle2.radius;
        }
        #endregion

        #region Object Model
        public static bool Equals(Circle first, Circle second)
        {
            return first.Equals(second);
        }
        #endregion
        #endregion

        #region Operators
        public static bool operator ==(Circle lhs, Circle rhs)
        {
            return Equals(lhs, rhs);
        }

        public static bool operator !=(Circle lhs, Circle rhs)
        {
            return !Equals(lhs, rhs);
        }
        #endregion
        #endregion
    }
}
