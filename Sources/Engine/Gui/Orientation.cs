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
        private static readonly Direction[] negativeDirections = new[] { Direction.NegativeX, Direction.NegativeY };
        private static readonly Direction[] positiveDirections = new[] { Direction.PositiveX, Direction.PositiveY };

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
        /// <returns>The corresponding negative <see cref="Direction"/>.</returns>
        public static Direction GetNegativeDirection(this Orientation orientation)
        {
            return negativeDirections[(int)orientation];
        }

        /// <summary>
        /// Gets the <see cref="Direction"/> which goes towards the positive side of an <see cref="Orientation"/>'s axis.
        /// </summary>
        /// <param name="orientation">The <see cref="Orientation"/>.</param>
        /// <returns>The corresponding positive <see cref="Direction"/>.</returns>
        public static Direction GetPositiveDirection(this Orientation orientation)
        {
            return positiveDirections[(int)orientation];
        }
    }
}
