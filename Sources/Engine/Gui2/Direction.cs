using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui2
{
    /// <summary>
    /// Specifies one of the four directions.
    /// </summary>
    public enum Direction
    {
        /// <summary>
        /// Specifies the negative X axis side.
        /// </summary>
        NegativeX,

        /// <summary>
        /// Specifies the negative Y axis side.
        /// </summary>
        NegativeY,

        /// <summary>
        /// Specifies the positive X axis side.
        /// </summary>
        PositiveX,

        /// <summary>
        /// Specifies the positive Y axis side.
        /// </summary>
        PositiveY
    }

    /// <summary>
    /// Provides extension utility methods to <see cref="Direction"/> instances.
    /// </summary>
    public static class DirectionExtensions
    {
        private static readonly Orientation[] orientations = new[]
        {
            Orientation.Horizontal,
            Orientation.Vertical,
            Orientation.Horizontal,
            Orientation.Vertical
        };

        /// <summary>
        /// Gets the <see cref="Orientation"/> of a given <see cref="Direction"/>.
        /// </summary>
        /// <param name="direction">The <see cref="Direction"/> for which the <see cref="Orientation"/> is to be found.</param>
        /// <returns>The <see cref="Orientation"/> of <paramref name="direction"/>.</returns>
        public static Orientation GetOrientation(this Direction direction)
        {
            return orientations[(int)direction];
        }

        /// <summary>
        /// Gets a value indicating if a given <see cref="Direction"/> is either <see cref="Direction.NegativeX"/> or <see cref="Direction.PositiveX"/>.
        /// </summary>
        /// <param name="direction">The <see cref="Direction"/>.</param>
        /// <returns><c>True</c> if it is <see cref="Direction.NegativeX"/> or <see cref="Direction.PositiveX"/>, <c>false</c> if not.</returns>
        public static bool IsHorizontal(this Direction direction)
        {
            return GetOrientation(direction) == Orientation.Horizontal;
        }

        /// <summary>
        /// Gets a value indicating if a given <see cref="Direction"/> is either <see cref="Direction.NegativeY"/> or <see cref="Direction.PositiveY"/>.
        /// </summary>
        /// <param name="direction">The <see cref="Direction"/>.</param>
        /// <returns><c>True</c> if it is <see cref="Direction.NegativeY"/> or <see cref="Direction.PositiveY"/>, <c>false</c> if not.</returns>
        public static bool IsVertical(this Direction direction)
        {
            return GetOrientation(direction) == Orientation.Vertical;
        }

        /// <summary>
        /// Gets a value indicating if a given <see cref="Direction"/> is either <see cref="Direction.NegativeX"/> or <see cref="Direction.NegativeY"/>.
        /// </summary>
        /// <param name="direction">The <see cref="Direction"/>.</param>
        /// <returns><c>True</c> if it is <see cref="Direction.NegativeX"/> or <see cref="Direction.NegativeY"/>, <c>false</c> if not.</returns>
        public static bool IsMin(this Direction direction)
        {
            return (int)direction < 2;
        }

        /// <summary>
        /// Gets a value indicating if a given <see cref="Direction"/> is either <see cref="Direction.PositiveX"/> or <see cref="Direction.PositiveY"/>.
        /// </summary>
        /// <param name="direction">The <see cref="Direction"/>.</param>
        /// <returns><c>True</c> if it is <see cref="Direction.PositiveX"/> or <see cref="Direction.PositiveY"/>, <c>false</c> if not.</returns>
        public static bool IsMax(this Direction direction)
        {
            return (int)direction > 1;
        }
    }
}
