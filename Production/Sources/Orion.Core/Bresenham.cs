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
        public static bool All(LineSegment lineSegment, int width, Func<int, int, bool> predicate)
        {
            Argument.EnsureStrictlyPositive(width, "width");

            // Bresenham's line algorithm
            // Source: http://en.wikipedia.org/wiki/Bresenham's_line_algorithm

            Vector2 normal = lineSegment.Delta.PerpendicularLeft;
            normal.Normalize();

            for (int i = 0; i < width; ++i)
            {
                Vector2 displacement = normal * (i - width * 0.5f + 0.5f);

                Vector2 p0 = lineSegment.EndPoint1 + displacement;
                Vector2 p1 = lineSegment.EndPoint2 + displacement;

                if (!All((int)p0.X, (int)p0.Y, (int)p1.X, (int)p1.Y, predicate))
                    return false;
            }

            return true;
        }

        private static bool All(int x0, int y0, int x1, int y1, Func<int, int, bool> predicate)
        {
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
                bool isWalkable = steep ? predicate(y, x) : predicate(x, y);
                if (!isWalkable) return false;

                error = error - deltaY;
                if (error < 0)
                {
                    y += yStep;
                    error += deltaX;
                }
            }

            return true;
        }

        private static void Swap(ref int a, ref int b)
        {
            int temp = a;
            a = b;
            b = temp;
        }
    }
}
