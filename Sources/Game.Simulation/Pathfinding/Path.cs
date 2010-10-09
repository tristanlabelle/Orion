using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using OpenTK;
using Orion.Engine;

namespace Orion.Game.Simulation.Pathfinding
{
    public sealed class Path
    {
        #region Fields
        private readonly ReadOnlyCollection<Point> points;
        private readonly bool isComplete;
        #endregion

        #region Constructor
        public Path(IEnumerable<Point> points, bool complete)
        {
            Argument.EnsureNotNull(points, "points");
            this.points = points.ToList().AsReadOnly();
            this.isComplete = complete;
        }

        public Path(IEnumerable<Point> points)
            : this(points, true) { }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the point that is at the source of this path.
        /// </summary>
        public Point Source
        {
            get { return points[0]; }
        }

        /// <summary>
        /// Gets the end point of this path.
        /// </summary>
        public Point End
        {
            get { return points[points.Count - 1]; }
        }

        /// <summary>
        /// Gets the sequence of points that trace this path.
        /// </summary>
        public ReadOnlyCollection<Point> Points
        {
            get { return points; }
        }

        /// <summary>
        /// Gets the number of points there are in this path.
        /// </summary>
        public int PointCount
        {
            get { return points.Count; }
        }

        /// <summary>
        /// Gets a value indicating if this path is complete,
        /// if the pathfinder has reached a destination.
        /// </summary>
        public bool IsComplete
        {
            get { return isComplete; }
        }
        #endregion
    }
}
