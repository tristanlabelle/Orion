using System;
using OpenTK.Math;
using Orion.Engine;

namespace Orion.Engine.Geometry
{
    /// <summary>
    /// Represents a triangle, defined by its three 2D vertices.
    /// </summary>
    [Serializable]
    public struct Triangle
    {
        #region Instance
        #region Fields
        private readonly Vector2 vertex1;
        private readonly Vector2 vertex2;
        private readonly Vector2 vertex3;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance from the three vertices of the triangle.
        /// </summary>
        /// <param name="vertex1">The first vertex of the triangle.</param>
        /// <param name="vertex2">The second vertex of the triangle.</param>
        /// <param name="vertex3">The third vertex of the triangle.</param>
        public Triangle(Vector2 vertex1, Vector2 vertex2, Vector2 vertex3)
        {
            this.vertex1 = vertex1;
            this.vertex2 = vertex2;
            this.vertex3 = vertex3;
        }

        /// <summary>
        /// Initializes a new instance from the three vertices of the triangle.
        /// </summary>
        /// <param name="vertex1X">The X coordinate of the first vertex of the triangle.</param>
        /// <param name="vertex1Y">The Y coordinate of the first vertex of the triangle.</param>
        /// <param name="vertex2X">The X coordinate of the second vertex of the triangle.</param>
        /// <param name="vertex2Y">The Y coordinate of the second vertex of the triangle.</param>
        /// <param name="vertex3X">The X coordinate of the third vertex of the triangle.</param>
        /// <param name="vertex3Y">The Y coordinate of the third vertex of the triangle.</param>
        public Triangle(float vertex1X, float vertex1Y, float vertex2X, float vertex2Y, float vertex3X, float vertex3Y)
            : this(new Vector2(vertex1X, vertex1Y), new Vector2(vertex2X, vertex2Y), new Vector2(vertex3X, vertex3Y)) { }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the first vertex of this triangle.
        /// </summary>
        public Vector2 Vertex1
        {
            get { return vertex1; }
        }

        /// <summary>
        /// Gets the second vertex of this triangle.
        /// </summary>
        public Vector2 Vertex2
        {
            get { return vertex2; }
        }

        /// <summary>
        /// Gets the third vertex of this triangle.
        /// </summary>
        public Vector2 Vertex3
        {
            get { return vertex3; }
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return "{{{0}, {1}, {2}}}".FormatInvariant(vertex1, vertex2, vertex3);
        }
        #endregion
        #endregion

        #region Static
        #region Fields
        /// <summary>
        /// A degenerate zero-sized triangle at the origin.
        /// </summary>
        public static readonly Triangle Zero = new Triangle(Vector2.Zero, Vector2.Zero, Vector2.Zero);
        #endregion

        #region Methods
        #region Factory
        /// <summary>
        /// Creates a degenerate point-triangle from a single point.
        /// </summary>
        /// <param name="point">The degenerate triangle's point.</param>
        /// <returns>The resulting triangle.</returns>
        public static Triangle CreateDegenerate(Vector2 point)
        {
            return new Triangle(point, point, point);
        }

        /// <summary>
        /// Creates a degenerate line-triangle two vertices.
        /// </summary>
        /// <param name="vertex1">The first triangle vertex.</param>
        /// <param name="vertex2">The second triangle vertex.</param>
        /// <returns>The resulting triangle.</returns>
        public static Triangle CreateDegenerate(Vector2 vertex1, Vector2 vertex2)
        {
            return new Triangle(vertex1, vertex2, vertex2);
        }
        #endregion
        #endregion
        #endregion
    }
}
