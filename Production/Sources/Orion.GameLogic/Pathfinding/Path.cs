using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using OpenTK.Math;

namespace Orion.GameLogic.Pathfinding
{
    public sealed class Path
    {
        #region Fields
        private readonly World world;
        private readonly Vector2 source;
        private readonly Vector2 destination;
        private readonly ReadOnlyCollection<Vector2> points;
        #endregion

        #region Constructor
        internal Path(World world, Vector2 source, Vector2 destination, IEnumerable<Vector2> points)
        {
            Argument.EnsureNotNull(world, "world");
            Argument.EnsureNotNull(points, "points");

            this.world = world;
            this.source = source;
            this.destination = destination;
            this.points = new ReadOnlyCollection<Vector2>(points.ToList());
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the point that is at the source of this path.
        /// </summary>
        public Vector2 Source
        {
            get { return source; }
        }

        /// <summary>
        /// Gets the destination point of this path.
        /// </summary>
        public Vector2 Destination
        {
            get { return destination; }
        }

        /// <summary>
        /// Gets the sequence of points that trace this path.
        /// </summary>
        public ReadOnlyCollection<Vector2> Points
        {
            get { return points; }
        }
        #endregion

        #region Methods
        
        #endregion
    }
}
