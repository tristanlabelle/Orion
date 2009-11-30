using System;

namespace Orion.GameLogic.Pathfinding
{
    public sealed class PathNode
    {
        #region Fields
        private PathNode source;
        private Point16 point;
        private float costFromSource;
        private float distanceToDestination;
        #endregion

        #region Properties
        public PathNode Source
        {
            get { return source; }
        }

        public Point16 Point
        {
            get { return point; }
        }

        public float CostFromSource
        {
            get { return costFromSource; }
        }

        public float DistanceToDestination
        {
            get { return distanceToDestination; }
        }

        public float TotalCost
        {
            get { return costFromSource + distanceToDestination; }
        }
        #endregion

        #region Methods
        internal void Reset(PathNode source, Point16 point,
            float costFromSource, float distanceToDestination)
        {
            this.source = source;
            this.point = point;
            this.costFromSource = costFromSource;
            this.distanceToDestination = distanceToDestination;
        }

        internal void ChangeSource(PathNode source, float costFromSource, float distanceToDestination)
        {
            this.source = source;
            this.costFromSource = costFromSource;
            this.distanceToDestination = distanceToDestination;
        }
        #endregion
    }
}
