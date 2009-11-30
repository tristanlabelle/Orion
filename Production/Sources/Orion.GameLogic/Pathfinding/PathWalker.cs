using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Math;

namespace Orion.GameLogic.Pathfinding
{
    public sealed class PathWalker
    {
        #region Fields
        private readonly Path path;
        private Vector2 position;
        private int targetPointIndex;
        #endregion

        #region Constructors
        public PathWalker(Path path)
        {
            Argument.EnsureNotNull(path, "path");

            this.path = path;
            this.position = path.Points[0];
            this.targetPointIndex = 1;
        }
        #endregion

        #region Properties
        public Path Path
        {
            get { return path; }
        }

        public Vector2 Position
        {
            get { return position; }
        }

        public float Angle
        {
            get
            {
                int secondPointIndex = targetPointIndex;
                if (secondPointIndex == path.PointCount) --secondPointIndex;
                int firstPointIndex = secondPointIndex - 1;
                Vector2 firstPoint = path.Points[firstPointIndex];
                Vector2 secondPoint = path.Points[secondPointIndex];

                return (float)Math.Atan2(secondPoint.Y - firstPoint.Y, secondPoint.X - firstPoint.X);
            }
        }

        public bool HasReachedDestination
        {
            get { return targetPointIndex == path.PointCount; }
        }
        #endregion

        #region Methods
        public void Walk(float distance)
        {
            if (HasReachedDestination) return;

            Vector2 targetPoint = path.Points[targetPointIndex];
            Vector2 delta = targetPoint - position;
            Vector2 direction = Vector2.Normalize(delta);

            if (distance < delta.Length)
            {
                position += (direction * distance);
            }
            else
            {
                position = targetPoint;
                ++targetPointIndex;
            }
        }
        #endregion
    }
}
