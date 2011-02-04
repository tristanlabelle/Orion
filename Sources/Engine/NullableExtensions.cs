using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine
{
    /// <summary>
    /// Provides extension utility methods for the <see cref="Nullable{T}"/> structure.
    /// </summary>
    public static class NullableExtensions
    {
        /// <summary>
        /// Attempts to retreive a value from a nullable.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="nullableValue">The nullable value.</param>
        /// <param name="value">The value of the nullable value.</param>
        /// <returns><c>True</c> if the nullable had a value, <c>false</c> if not.</returns>
        public static bool TryGetValue<T>(this T? nullableValue, out T value) where T : struct
        {
            if (nullableValue.HasValue)
            {
                value = nullableValue.Value;
                return true;
            }
            else
            {
                value = default(T);
                return false;
            }
        }
    }
}
