using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Math;
using Orion.Geometry;

namespace Orion
{
    /// <summary>
    /// Implement's Xiaolin Wu's line drawing algorithm (with antialiasing).
    /// </summary>
    public static class XiaolinWu
    {
        private static KeyValuePair<Point, float> plot(int x, int y, float c)
        {
            Point point = new Point(x, y);
            return new KeyValuePair<Point, float>(point, c);
        }

        private static int ipart(float x)
        {
            return (int)x;
        }

        private static int round(float x)
        {
            return ipart(x + 0.5f);
        }

        private static float fpart(float x)
        {
            return x - ipart(x);
        }

        private static float rfpart(float x)
        {
            return 1 - fpart(x);
        }

        public static IEnumerable<KeyValuePair<Point, float>> GetPoints(Vector2 p1, Vector2 p2)
        {
            Vector2 delta = p2 - p1;
           
            if (Math.Abs(delta.X) > Math.Abs(delta.Y))
            {
                //handle "horizontal" lines
                if (p2.X < p1.X)
                {
                    float swap = p1.X;
                    p1.X = p2.X;
                    p2.X = swap;

                    swap = p1.Y;
                    p1.Y = p2.Y;
                    p2.Y = swap;
                }

                float gradient = delta.Y / delta.X;
               
                // handle first endpoint
                int xend = round(p1.X);
                float yend = p1.Y + gradient * (xend - p1.X);
                float xgap = rfpart(p1.X + 0.5f);
                int xpxl1 = xend;  // this will be used in the main loop
                int ypxl1 = ipart(yend);

                yield return plot(xpxl1, ypxl1, rfpart(yend) * xgap);
                yield return plot(xpxl1, ypxl1 + 1, fpart(yend) * xgap);

                float intery = yend + gradient; // first y-intersection for the main loop
             
                // handle second endpoint
                xend = round(p2.X);
                yend = p2.Y + gradient * (xend - p2.X);
                xgap = fpart(p2.X + 0.5f);
                int xpxl2 = xend; // this will be used in the main loop
                int ypxl2 = ipart(yend);

                yield return plot(xpxl2, ypxl2, rfpart(yend) * xgap);
                yield return plot(xpxl2, ypxl2 + 1, fpart(yend) * xgap);

                // main loop
                for (int x = xpxl1 + 1; x < xpxl2; ++x)
                {
                    yield return plot(x, ipart(intery), rfpart(intery));
                    yield return plot(x, ipart(intery) + 1, fpart(intery));
                    intery = intery + gradient;
                }
            }
            else
            {
              //handle "vertical" lines  same code as above but X takes the role of Y
            }
        }

        public static IEnumerable<KeyValuePair<Point, float>> GetPoints(LineSegment segment)
        {
            return GetPoints(segment.EndPoint1, segment.EndPoint2);
        }
    }
}
