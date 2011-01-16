using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui2
{
    /// <summary>
    /// Specifies the orientation as either horizontal or vertical.
    /// </summary>
    public enum Orientation
    {
        /// <summary>
        /// Specifies a horizontal orientation.
        /// </summary>
        Horizontal,

        /// <summary>
        /// Specifies a vertical orientation.
        /// </summary>
        Vertical
    }

    /// <summary>
    /// Provides utility extension methods to the <see cref="Orientation"/> enumeration.
    /// </summary>
    public static class OrientationExtensions
    {
        private static readonly Direction[] minDirections = new[] { Direction.MinX, Direction.MinY };
        private static readonly Direction[] maxDirections = new[] { Direction.MaxX, Direction.MaxY };

        /// <summary>
        /// Gets the other <see cref="Orientation"/>.
        /// </summary>
        /// <param name="orientation">The initial <see cref="Orientation"/>.</param>
        /// <returns>The <see cref="Orientation"/> other than <paramref name="orientation"/>.</returns>
        public static Orientation GetOther(this Orientation orientation)
        {
            return (Orientation)(1 - (int)orientation);
        }

        /// <summary>
        /// Gets the <see cref="Direction"/> which goes towards the negative side of an <see cref="Orientation"/>'s axis.
        /// </summary>
        /// <param name="orientation">The <see cref="Orientation"/>.</param>
        /// <returns>The corresponding minimum <see cref="Direction"/>.</returns>
        public static Direction GetMinDirection(this Orientation orientation)
        {
            return minDirections[(int)orientation];
        }

        /// <summary>
        /// Gets the <see cref="Direction"/> which goes towards the positive side of an <see cref="Orientation"/>'s axis.
        /// </summary>
        /// <param name="orientation">The <see cref="Orientation"/>.</param>
        /// <returns>The corresponding maximum <see cref="Direction"/>.</returns>
        public static Direction GetMaxDirection(this Orientation orientation)
        {
            return maxDirections[(int)orientation];
        }
    }
}
