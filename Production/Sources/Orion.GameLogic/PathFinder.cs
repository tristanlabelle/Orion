using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Math;
using System.Drawing;

namespace Orion.GameLogic
{
    public class PathFinder
    {
        #region Fields
        readonly World world;
        #endregion

        #region Constructors
        /// <summary>
        /// Create a New Object PathFinder that return an object Path.
        /// </summary>
        /// <param name="source">The Position of the unit</param>
        /// <param name="destination">The destination point</param>
        public PathFinder(World world)
        {
            Argument.EnsureNotNull(world, "world");
            this.world = world;
        }
        #endregion

        #region Methods
        public Path FindPath(Vector2 source, Vector2 destination)
        {
            Path path = new Path(this, source, destination);
            return path.Succeeded ? path : null;
        }
        #endregion

        #region Proprieties

        public World World
        {
            get { return world; }
        }

        #endregion
    }
}
