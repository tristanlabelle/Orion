using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK.Math;

namespace Orion.Geometry
{
    public static class Intersection
    {
        #region Fields

        #endregion

        #region Constructors

        #endregion

        #region Properties

        #endregion

        #region Methods

        public static bool RectangleIntersectsCircle(Rectangle rectangle, Circle circle)
        {
            Vector2 closestPointInRect = ClosestPointInRect(rectangle, circle.Center);
            bool distanceLessThanRadius = DistanceLessThanRadius(closestPointInRect, circle.Center, circle.Radius);
            if (distanceLessThanRadius)
                return true;
            return false;
        }

        public static Vector2 ClosestPointInRect(Rectangle rectangle, Vector2 center)
        {
            float X = center.X;
            float Y = center.Y;
            
            if (center.X < rectangle.X)
                X = rectangle.X;
            else if (center.X > rectangle.MaxX)
                X = rectangle.MaxX;

            if (center.Y < rectangle.Y)
                Y = rectangle.Y;
            else if (center.Y > rectangle.MaxY)
                Y = rectangle.MaxY;

            return new Vector2(X,Y);
        }

        public static bool DistanceLessThanRadius(Vector2 closestPointInRect, Vector2 center, float radius)
        {
            float distance = (center - closestPointInRect).Length;
            if (distance < radius)
                return true;
            return false;
        }

        #endregion
        
    }
}
