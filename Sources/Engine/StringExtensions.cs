using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine
{
    /// <summary>
    /// Provides utility extension methods to the <see cref="System.String"/> class.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Creates a new string with one character removed at a given index.
        /// </summary>
        /// <param name="str">The original string.</param>
        /// <param name="index">The index of the character to be removed.</param>
        /// <returns>The new string with the character removed.</returns>
        public static string RemoveAt(this string str, int index)
        {
            Argument.EnsureNotNull(str, "str");
            if (index < 0 || index >= str.Length)
                throw new ArgumentOutOfRangeException("index");

            if (index == 0) return str.Substring(1);
            if (index == str.Length - 1) return str.Substring(0, str.Length - 1);
            return str.Substring(0, index) + str.Substring(index + 1);
        }

        /// <summary>
        /// Creates a new string with a character inserted at a given index.
        /// </summary>
        /// <param name="str">The original string.</param>
        /// <param name="index">The index where the character should be inserted.</param>
        /// <param name="character">The character to insert at that index.</param>
        /// <returns>A new string with the character insered.</returns>
        public static string Insert(this string str, int index, char character)
        {
            Argument.EnsureNotNull(str, "str");

            if (index == 0) return character + str;
            if (index == str.Length) return str + character;

            if (index > 0 && index < str.Length)
                return str.Substring(0, index) + character + str.Substring(index);

            throw new ArgumentOutOfRangeException("index");
        }
    }
}
