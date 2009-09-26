using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK.Math;

namespace Orion.Geometry
{
    /// <summary>
    /// Represents a 2D ellipse shape.
    /// </summary>
    [Serializable]
    public struct Ellipse
    {
        #region Instance
        #region Fields
        private readonly Vector2 center;
        private readonly Vector2 radii;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new ellipse from a circle.
        /// </summary>
        /// <param name="circle">The original circle.</param>
        public Ellipse(Circle circle)
        {
            this.center = circle.Center;
            this.radii = new Vector2(circle.Radius, circle.Radius);
        }

        /// <summary>
        /// Initializes a new ellipse from its center and its radii.
        /// </summary>
        /// <param name="center">The center point of the ellipse.</param>
        /// <param name="radiiX">The x component of the ellipse radii.</param>
        /// <param name="radiiY">The y component of the ellipse radii.</param>
        public Ellipse(Vector2 center, float radiiX, float radiiY)
        {
            this.center = center;
            this.radii = new Vector2(Math.Abs(radiiX), Math.Abs(radiiY));
        }

        /// <summary>
        /// Initializes a new ellipse from its center and its radii.
        /// </summary>
        /// <param name="center">The ellipse center.</param>
        /// <param name="radii">The ellipse radii.</param>
        public Ellipse(Vector2 center, Vector2 radii)
            : this(center, radii.X, radii.Y) { }

        /// <summary>
        /// Initializes a new ellipse from its center and its radii.
        /// </summary>
        /// <param name="centerX">The x component of the ellipse center.</param>
        /// <param name="centerY">The y component of the ellipse center.</param>
        /// <param name="radiiX">The x component of the ellipse radii.</param>
        /// <param name="radiiY">The y component of the ellipse radii.</param>
        public Ellipse(float centerX, float centerY, float radiiX, float radiiY)
            : this(new Vector2(centerX, centerY), new Vector2(radiiX, radiiY)) { }
        #endregion

        #region Properties
        #region Center
        /// <summary>
        /// Gets the center point of the ellipse.
        /// </summary>
        public Vector2 Center
        {
            get { return center; }
        }
        #endregion

        #region Radius
        /// <summary>
        /// Gets the radii of the ellipse (radius along axes).
        /// </summary>
        public Vector2 Radii
        {
            get { return radii; }
        }

        /// <summary>
        /// Gets the X and Y components of the diameter of this ellipse.
        /// </summary>
        public Vector2 Diameters
        {
            get { return radii * 2; }
        }
        #endregion

        #region Bounding Shapes
        /// <summary>
        /// Gets a <see cref="Rectangle"/> that bounds this <see cref="Ellipse"/>.
        /// </summary>
        public Rectangle BoundingRectangle
        {
            get { return Rectangle.FromCenterExtent(center, radii); }
        }

        /// <summary>
        /// Gets a <see cref="Circle"/> that bounds this <see cref="Ellipse"/>.
        /// </summary>
        public Circle BoundingCircle
        {
            get { return new Circle(center, Math.Max(radii.X, radii.Y)); }
        }
        #endregion
        #endregion

        #region Methods
        #region Point-related
        /// <summary>
        /// Gets a point inside the ellipse that is the closest to another point.
        /// </summary>
        /// <param name="point">A point which's contained image should be retrieved.</param>
        /// <returns>The point within the ellipse that is the closest to that point.</returns>
        public Vector2 ClosestPointInside(Vector2 point)
        {
            Vector2 normalizedPoint = new Vector2((point.X - center.X) / radii.X, (point.Y - center.Y) / radii.Y);
            normalizedPoint = Circle.Unit.ClosestPointInside(normalizedPoint);
            return new Vector2(normalizedPoint.X * radii.X + center.X, normalizedPoint.Y * radii.Y + center.Y);
        }

        /// <summary>
        /// Gets a value indicating if this ellipse contains a point.
        /// </summary>
        /// <param name="point">The point to be tested for containment.</param>
        /// <returns>True if the point is within the ellipse, false otherwise.</returns>
        public bool Contains(Vector2 point)
        {
            //x^2   y^2
            //--- + --- <= 1
            //a^2   b^2
            Vector2 offset = center - point;
            float x = (offset.X * offset.X) / (radii.X * radii.X);
            float y = (offset.Y * offset.Y) / (radii.Y * radii.Y);
            return x + y <= 1;
        }
        #endregion

        #region Object Model
        /// <summary>
        /// Gets a string representation of this object.
        /// </summary>
        /// <returns>The string representing this object.</returns>
        public override string ToString()
        {
            return "{{{0}, {1}x{2}}}".FormatInvariant(center, radii.X, radii.Y);
        }
        #endregion
        #endregion
        #endregion

        #region Static
        #region Fields
        /// <summary>
        /// An ellipse with zero radii centered at the origin.
        /// </summary>
        public static readonly Ellipse Zero = new Ellipse(0, 0, 0, 0);

        /// <summary>
        /// A circular ellipse with radii of one by one centered at the origin.
        /// </summary>
        public static readonly Ellipse Unit = new Ellipse(0, 0, 1, 1);
        #endregion

        #region Operators
        #region Casting
        /// <summary>
        /// Casts a circle to an ellipse.
        /// </summary>
        /// <param name="circle">The original circle.</param>
        /// <returns>The circle casted as an ellipse.</returns>
        public static implicit operator Ellipse(Circle circle)
        {
            return new Ellipse(circle);
        }
        #endregion
        #endregion
        #endregion
    }
}
