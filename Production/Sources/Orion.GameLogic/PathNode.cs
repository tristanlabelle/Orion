using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Math;
using System.Drawing;

namespace Orion.GameLogic
{
    public class PathNode
    {
        #region Fields
        PathNode parentNode;

       
        Point position;

        
        float totalCost;

        #endregion
        #region constructor
        /// <summary>
        /// Constructor for a Node (path of terrain).
        /// </summary>
        /// <param name="parentNode">The parent represent the fastest way to move from an near by Node </param>
        /// <param name="position">The x y position of the node in the world</param>
        /// <param name="totalCost">The cost of the move by adding the deplacement done and the distance from de destination</param>
        public PathNode(PathNode parentNode, Point position, float totalCost)
        {
            this.parentNode = parentNode;
            this.position = position;
            this.totalCost = totalCost;
        }
        #endregion

        #region Proprieties

        public float TotalCost
        {
            get { return totalCost; }
            set { totalCost = value; }
        }

        public Point Position
        {
            get { return position; }
            set { position = value; }
        }

        public PathNode ParentNode
        {
            get { return parentNode; }
            set { parentNode = value; }
        }

        #endregion
    }
}
