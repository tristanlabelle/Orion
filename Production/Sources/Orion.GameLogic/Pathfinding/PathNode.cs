

namespace Orion.GameLogic.Pathfinding
{
    public sealed class PathNode
    {
        #region Fields
        private PathNode source;
        private Point16 point;
        private float costFromSource;
        private float totalCost; // Stored instead of costToDestination as it is most often accessed.
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

        public float EstimatedCostToDestination
        {
            get { return totalCost - costFromSource; }
        }

        public float TotalCost
        {
            get { return totalCost; }
        }
        #endregion

        #region Methods
        internal void Reset(PathNode source, Point16 point,
            float costFromSource, float estimatedCostToDestination)
        {
            this.source = source;
            this.point = point;
            this.costFromSource = costFromSource;
            this.totalCost = costFromSource + estimatedCostToDestination;
        }

        internal void ChangeSource(PathNode source, float costFromSource, float estimatedCostToDestination)
        {
            this.source = source;
            this.costFromSource = costFromSource;
            this.totalCost = costFromSource + estimatedCostToDestination;
        }
        #endregion
    }
}
