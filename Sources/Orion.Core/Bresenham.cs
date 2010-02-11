using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Geometry;
using OpenTK.Math;

namespace Orion
{
    public static class Bresenham
    {
        public static IEnumerable<Point> GetPoints(LineSegment lineSegment, int width)
        {
            Argument.EnsureStrictlyPositive(width, "width");

            // Bresenham's line algorithm
            // Source: http://en.wikipedia.org/wiki/Bresenham's_line_algorithm

            Vector2 normalizedDelta = Vector2.Normalize(lineSegment.Delta);
            lineSegment = new LineSegment(lineSegment.EndPoint1,
                lineSegment.EndPoint1 + normalizedDelta * (lineSegment.Length + 1));
            Vector2 normalizedNormal = normalizedDelta.PerpendicularLeft;

            for (int i = 0; i < width; ++i)
            {
                Vector2 displacement = normalizedNormal * (i - width * 0.5f + 0.5f);

                Vector2 point0 = lineSegment.EndPoint1 + displacement;
                Vector2 point1 = lineSegment.EndPoint2 + displacement;

                int x0 = (int)(point0.X + 0.5f);
                int y0 = (int)(point0.Y + 0.5f);
                int x1 = (int)(point1.X + 0.5f);
                int y1 = (int)(point1.Y + 0.5f);

                bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
                if (steep)
                {
                    Swap(ref x0, ref y0);
                    Swap(ref x1, ref y1);
                }

                if (x0 > x1)
                {
                    Swap(ref x0, ref x1);
                    Swap(ref y0, ref y1);
                }

                int deltaX = x1 - x0;
                int deltaY = Math.Abs(y1 - y0);
                int error = deltaX / 2;
                int yStep = (y0 < y1) ? 1 : -1;
                int y = y0;

                for (int x = x0; x < x1; ++x)
                {
                    Point point = steep ? new Point(y, x) : new Point(x, y);
                    yield return point;

                    error = error - deltaY;
                    if (error < 0)
                    {
                        y += yStep;
                        error += deltaX;
                    }
                }
            }
        }

        private static void Swap(ref int a, ref int b)
        {
            int temp = a;
            a = b;
            b = temp;
        }
    }
}
