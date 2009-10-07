using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Math;
using System.Drawing;

namespace Orion.GameLogic
{
    class PathFinder
    {
        #region Fields
        Path theFinalPath;
        Point source;
        Point destination;
        List<PathNode> openNode = new List<PathNode>();
        List<PathNode> closeNode = new List<PathNode>();
        #endregion

        #region constructor
        /// <summary>
        /// Create a New Object PathFinder that return an object Path.
        /// </summary>
        /// <param name="source">The Position of the unit</param>
        /// <param name="destination">The destination point</param>
        public PathFinder(Point source, Point destination)
        {
            this.source = source;
            this.destination = destination;
            theFinalPath = new Path();
        }
        private float CalculateMoveCost(PathNode aNode)
        {
            return Math.Abs(aNode.Position.X - destination.X) + Math.Abs(aNode.Position.Y - destination.Y);
        }
        private void getNearPointToAdd(PathNode currentNode)
        {
            for (int j = -1; j <= 1; j++) // colonne 
                for (int i = -1; i <= 1; i++) // ligne
                {
                    if (closeNode.Any(node =>
                        node.Position.X == (currentNode.Position.X - j) &&
                        node.Position.Y == (currentNode.Position.X - i))) ;
                    //TO BE CONTIUNE - MATHIEU

                }
        }
        public Path FindPath()
        {
            
            return null;
        }
        
        #endregion
    }
}
