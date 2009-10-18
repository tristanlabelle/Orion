using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK.Math;

namespace Orion.GameLogic.Pathfinding
{
    public sealed class Pathfinder
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
        public Pathfinder(World world)
        {
            Argument.EnsureNotNull(world, "world");
            this.world = world;
        }
        #endregion

        #region Methods
        public Path FindPath(Vector2 source, Vector2 destination)
        {
            Point16 destinationPoint = new Point16((short)destination.X, (short)destination.Y);
            if (destinationPoint.X < 0 || destinationPoint.Y < 0
                || destinationPoint.X >= world.Width || destinationPoint.Y >= world.Height
                || !world.Terrain.IsWalkable(destinationPoint.X, destinationPoint.Y))
                return null;

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
