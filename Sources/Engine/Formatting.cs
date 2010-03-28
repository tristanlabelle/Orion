using System;
using System.Globalization;
using System.Text;

namespace Orion.Engine
{
    /// <summary>
    /// Provides extension methods to simplify the use of format strings.
    /// </summary>
    public static class Formatting
    {
        #region FormatInvariant
        /// <summary>
        /// Formats a string using invariant culture information.
        /// </summary>
        /// <param name="format">The format string used for the formatting.</param>
        /// <param name="args">The arguments referenced by <paramref name="format"/>.</param>
        /// <returns>The format string formatted with the given args.</returns>
        public static string FormatInvariant(this string format, params object[] args)
        {
            Argument.EnsureNotNull(format, "format");
            Argument.EnsureNotNull(args, "args");
            return string.Format(CultureInfo.InvariantCulture, format, args);
        }
        #endregion

        #region ToStringInvariant
        /// <summary>
        /// Gets the string representation of an object using invariant culture information.
        /// </summary>
        /// <typeparam name="T">The type of object for which the string representation is to be retrieved.</typeparam>
        /// <param name="object">The object for which the string representation is to be retrieved.</param>
        /// <returns>The culture invariant string representation of <paramref name="object"/>.</returns>
        public static string ToStringInvariant<T>(this T @object) where T : IFormattable
        {
            if (!typeof(T).IsValueType) Argument.EnsureNotNull((object)@object, "object");
            return @object.ToString(null, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Gets the string representation of an object with invariant culture information using a format string.
        /// </summary>
        /// <typeparam name="T">The type of object for which the string representation is to be retrieved.</typeparam>
        /// <param name="object">The object for which the string representation is to be retrieved.</param>
        /// <param name="format">
        /// A format string describing the desired string representation format,
        /// or <c>null</c> to use the default string representation format.
        /// </param>
        /// <returns>The culture invariant string representation of <paramref name="object"/>.</returns>
        public static string ToStringInvariant<T>(this T @object, string format) where T : IFormattable
        {
            if (!typeof(T).IsValueType) Argument.EnsureNotNull((object)@object, "object");
            return @object.ToString(format, CultureInfo.InvariantCulture);
        }
        #endregion

        #region StringBuilder
        /// <summary>
        /// Clears the content of a <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="stringBuilder">The <see cref="StringBuilder"/> to be cleared.</param>
        public static void Clear(this StringBuilder stringBuilder)
        {
            Argument.EnsureNotNull(stringBuilder, "stringBuilder");
            stringBuilder.Remove(0, stringBuilder.Length);
        }

        public static StringBuilder AppendFormatInvariant(this StringBuilder stringBuilder,
            string format, params object[] args)
        {
            return stringBuilder.AppendFormat(CultureInfo.InvariantCulture, format, args);
        }
        #endregion
    }
}
