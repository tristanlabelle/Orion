using System;

using OpenTK.Math;

namespace Orion.Geometry
{
    /// <summary>
    /// Represents a line segment.
    /// </summary>
    [Serializable]
    public struct LineSegment
    {
        #region Instance
        #region Fields
        private readonly Vector2 endPoint1;
        private readonly Vector2 endPoint2;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new line segment from its end points.
        /// </summary>
        /// <param name="endPoint1">The first end point of the segment.</param>
        /// <param name="endPoint2">The second end point of the segment.</param>
        /// <returns>A corresponding line segment.</returns>
        public LineSegment(Vector2 endPoint1, Vector2 endPoint2)
        {
            this.endPoint1 = endPoint1;
            this.endPoint2 = endPoint2;
        }

        /// <summary>
        /// Initializes a new line segment from its end points.
        /// </summary>
        /// <param name="endPoint1X">The X coordinate of the first end point of the segment.</param>
        /// <param name="endPoint1Y">The Y coordinate of the first end point of the segment.</param>
        /// <param name="endPoint2X">The X coordinate of the second end point of the segment.</param>
        /// <param name="endPoint2Y">The Y coordinate of the second end point of the segment.</param>
        /// <returns>A corresponding line segment.</returns>
        public LineSegment(float endPoint1X, float endPoint1Y, float endPoint2X, float endPoint2Y)
        {
            this.endPoint1 = new Vector2(endPoint1X, endPoint1Y);
            this.endPoint2 = new Vector2(endPoint2X, endPoint2Y);
        }
        #endregion

        #region Properties
        #region Points
        /// <summary>
        /// Gets the first end point of this line segment.
        /// </summary>
        public Vector2 EndPoint1
        {
            get { return endPoint1; }
        }

        /// <summary>
        /// Gets the second end point of this line segment.
        /// </summary>
        public Vector2 EndPoint2
        {
            get { return endPoint2; }
        }

        /// <summary>
        /// Gets the center of this line segment.
        /// </summary>
        public Vector2 Center
        {
            get { return (endPoint1 + endPoint2) * 0.5f; }
        }
        #endregion

        #region Delta/Length
        /// <summary>
        /// Gets a vector from the first end point ot the second.
        /// </summary>
        public Vector2 Delta
        {
            get { return endPoint2 - endPoint1; }
        }

        /// <summary>
        /// Gets the squared length of this line segment.
        /// </summary>
        public float SquaredLength
        {
            get { return Delta.LengthSquared; }
        }

        /// <summary>
        /// Gets the length of this line segment.
        /// </summary>
        public float Length
        {
            get { return Delta.Length; }
        }
        #endregion

        #region Bounding Shapes
        /// <summary>
        /// Gets a rectangle bounding this line segment.
        /// </summary>
        public Rectangle BoundingRectangle
        {
            get { return Rectangle.FromPoints(endPoint1, endPoint2); }
        }

        /// <summary>
        /// Gets a circle that bounds this line segment.
        /// </summary>
        public Circle BoundingCircle
        {
            get { return new Circle(Center, Length * 0.5f); }
        }
        #endregion
        #endregion

        #region Methods
        public override string ToString()
        {
            return "{0}----{1}".FormatInvariant(endPoint1, endPoint2);
        }
        #endregion
        #endregion

        #region Static
        #region Fields
        /// <summary>
        /// A zero-length line segment at the origin.
        /// </summary>
        public static readonly LineSegment Zero = FromCenterDelta(Vector2.Zero, Vector2.Zero);
        #endregion

        #region Methods
        #region Factory
        /// <summary>
        /// Creates a degenerate line segment from a single point.
        /// </summary>
        /// <param name="point">The point forming the degenerate line segment.</param>
        /// <returns>The corresponding line segment.</returns>
        public static LineSegment FromPoint(Vector2 point)
        {
            return new LineSegment(point, point);
        }

        /// <summary>
        /// Creates a new line segment from its center and delta vector.
        /// </summary>
        /// <param name="center">The center of the line segment.</param>
        /// <param name="delta">The delta from the segment's center to one of its end points.</param>
        /// <returns>A new line segment with those values.</returns>
        public static LineSegment FromCenterDelta(Vector2 center, Vector2 delta)
        {
            return new LineSegment(center + delta, center - delta);
        }

        /// <summary>
        /// Creates a new line segment from its center and delta vector.
        /// </summary>
        /// <param name="centerX">The X coordinate of the center of the line segment.</param>
        /// <param name="centerY">The Y coordinate of the center of the line segment.</param>
        /// <param name="deltaX">The X coordinate of the delta from the segment's center to one of its end points.</param>
        /// <param name="deltaY">The Y coordinate of the delta from the segment's center to one of its end points.</param>
        /// <returns>A new line segment with those values.</returns>
        public static LineSegment FromCenterDelta(float centerX, float centerY, float deltaX, float deltaY)
        {
            return new LineSegment(centerX, centerY, deltaX, deltaY);
        }
        #endregion
        #endregion
        #endregion
    }
}
