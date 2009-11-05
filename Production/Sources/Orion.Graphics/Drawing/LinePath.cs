using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using OpenTK.Math;

using Orion.Geometry;

namespace Orion.Graphics
{
    /// <summary>
    /// Defines a shape to be drawn by drawing lines between points.
    /// </summary>
    public sealed class LinePath
    {
        #region Instance
        #region Fields
        private readonly ReadOnlyCollection<LineSegment> lineSegments;
        #endregion

        #region Constructors
        private LinePath(IEnumerable<LineSegment> lineSegments)
        {
            Argument.EnsureNotNull(lineSegments, "lineSegments");
            this.lineSegments = lineSegments.ToList().AsReadOnly();
            Argument.EnsureNotNullNorEmpty(this.lineSegments, "lineSegments");
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the read-only collection of the line segments of this <see cref="LinePath"/>.
        /// </summary>
        public ReadOnlyCollection<LineSegment> LineSegments
        {
            get { return lineSegments; }
        }
        #endregion
        #endregion

        #region Static
        #region Fields
        public static readonly LinePath Circle = CreateCircle(0.5f, 32);
        public static readonly LinePath Diamond = CreateCircle(0.5f, 4);
        public static readonly LinePath Pentagon = CreateCircle(0.5f, 5);
        public static readonly LinePath Square;
        public static readonly LinePath Triangle = CreateCircle(0.5f, 3);
        public static readonly LinePath Plus;
        public static readonly LinePath Cross;
        #endregion

        #region Constructor
        static LinePath()
        {
            Square = LinePath.CreateLoop(new Vector2(-0.5f, -0.5f),
                new Vector2(-0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, -0.5f));
            Plus = LinePath.FromLineSegments(
                new LineSegment(-0.5f, 0, 0.5f, 0),
                new LineSegment(0, -0.5f, 0, 0.5f));
            Cross = LinePath.FromLineSegments(
                new LineSegment(-0.5f, -0.5f, 0.5f, 0.5f),
                new LineSegment(-0.5f, 0.5f, 0.5f, -0.5f));
        }
        #endregion

        #region Methods
        public static LinePath FromLineSegments(IEnumerable<LineSegment> lineSegments)
        {
            return new LinePath(lineSegments);
        }

        public static LinePath FromLineSegments(params LineSegment[] lineSegments)
        {
            return FromLineSegments((IEnumerable<LineSegment>)lineSegments);
        }

        public static LinePath CreateStrip(IEnumerable<Vector2> points)
        {
            Argument.EnsureNotNull(points, "points");

            List<LineSegment> lineSegments = new List<LineSegment>();

            using (IEnumerator<Vector2> enumerator = points.GetEnumerator())
            {
                if (!enumerator.MoveNext()) throw new ArgumentException("Expected at least two points to create a line path.", "points");
                Vector2 firstPoint = enumerator.Current;
                
                if (!enumerator.MoveNext()) throw new ArgumentException("Expected at least two points to create a line path.", "points");
                Vector2 previousPoint = firstPoint;
                Vector2 currentPoint = firstPoint;

                do
                {
                    currentPoint = enumerator.Current;
                    lineSegments.Add(new LineSegment(previousPoint, currentPoint));
                    previousPoint = currentPoint;
                } while (enumerator.MoveNext());

                lineSegments.Add(new LineSegment(currentPoint, firstPoint));
            }

            return new LinePath(lineSegments);
        }


        public static LinePath CreateLoop(IEnumerable<Vector2> points)
        {
            Argument.EnsureNotNull(points, "points");

            List<Vector2> loopPoints = points.ToList();
            if (loopPoints.Count < 2) throw new ArgumentException("Expected at least two points to create a line path.", "points");

            loopPoints.Add(loopPoints[0]);

            return CreateStrip(loopPoints);
        }

        public static LinePath CreateLoop(params Vector2[] points)
        {
            return CreateLoop((IEnumerable<Vector2>)points);
        }

        public static LinePath CreateCircle(float radius, int pointCount)
        {
            Argument.EnsureGreaterOrEqual(pointCount, 2, "pointCount");

            Vector2[] points = new Vector2[pointCount];
            double angleIncrement = Math.PI * 2 / points.Length;
            for (int i = 0; i < points.Length; ++i)
            {
                double angle = angleIncrement * i;
                double x = Math.Cos(angle) * radius;
                double y = Math.Sin(angle) * radius;
                points[i] = new Vector2((float)x, (float)y);
            }

            return CreateLoop(points);
        }
        #endregion
        #endregion
    }
}
