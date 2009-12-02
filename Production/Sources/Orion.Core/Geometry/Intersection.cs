
using OpenTK.Math;

namespace Orion.Geometry
{
    /// <summary>
    /// Provide helper methods to determine if various shapes intersect.
    /// </summary>
    public static class Intersection
    {
        #region Methods
        public static bool Test(Rectangle rectangle, Circle circle)
        {
            Vector2 closestPointInRect = rectangle.Clamp(circle.Center);
            float squaredDistance = (circle.Center - closestPointInRect).LengthSquared;
            return squaredDistance < circle.SquaredRadius;
        }

        public static bool Test(Circle circle, Rectangle rectangle)
        {
            return Intersection.Test(rectangle, circle);
        }
        #endregion
    }
}
