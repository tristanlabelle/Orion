using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Math;

namespace Orion.Graphics
{
    /// <summary>
    /// Defines a shape to be drawn by drawing lines between points.
    /// </summary>
    public sealed class ShapePath
    {
        #region Fields

        private readonly List<Vector2> points;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="ShapePath"/> object. 
        /// </summary>
        public ShapePath()
        {
            points = new List<Vector2>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the size of the list of points.
        /// </summary>
        public int Count
        {
            get { return points.Count; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds a new point to the list.
        /// </summary>
        /// <param name="point">The point to add to the list.</param>
        public void AddPoint(Vector2 point)
        {
            points.Add(point);
        }

        /// <summary>
        /// Returns a point from the list at a given index.
        /// </summary>
        /// <param name="index">The index of the requested point.</param>
        public Vector2 GetPointAt(int index)
        {
            return points[index]; 
        }

        #endregion
    }
}
