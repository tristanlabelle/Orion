using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Orion
{
    /// <summary>
    /// Provides helper methods to deal with case changes.
    /// </summary>
    public static class Casing
    {
        #region Fields
        private static readonly Regex camelSubwordStartRegex = new Regex(@"(?<=[A-Za-z])[A-Z]",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);

        private static readonly Regex secondWordRegex = new Regex(@"(?<=[a-zA-Z])\s+(?<word>[a-zA-Z]+)",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);
        #endregion

        #region Methods
        #region CamelToWords
        /// <summary>
        /// Splits a camel cased words to words by inserting spaces.
        /// </summary>
        /// <param name="text">A string containing camel-cased words.</param>
        /// <returns>The resulting string.</returns>
        public static string CamelToWords(string text)
        {
            Argument.EnsureNotNull(text, "text");
            return camelSubwordStartRegex.Replace(text, CamelToWordsMatchEvaluator);
        }

        /// <summary>
        /// Splits a camel cased words to lower-case words by inserting spaces.
        /// </summary>
        /// <param name="text">A string containing camel-cased words.</param>
        /// <returns>The resulting lower-case string.</returns>
        public static string CamelToLowerWords(string text)
        {
            return CamelToWords(text).ToLowerInvariant();
        }

        private static string CamelToWordsMatchEvaluator(Match match)
        {
            return ' ' + match.Value.ToLowerInvariant();
        }
        #endregion

        #region WordsToCamel
        /// <summary>
        /// Converts a series of words into camel case by squashing them together.
        /// </summary>
        /// <param name="text">The text to be converted.</param>
        /// <returns>The resulting string.</returns>
        public static string WordsToCamel(string text)
        {
            if (text == null) throw new ArgumentNullException("instance");
            return secondWordRegex.Replace(text, WordsToCamelMatchEvaluator);
        }

        private static string WordsToCamelMatchEvaluator(Match match)
        {
            string formattedWord = match.Groups["word"].Value;
            if (formattedWord.Length == 1) return formattedWord.ToUpperInvariant();
            else return char.ToUpperInvariant(formattedWord[0]) + formattedWord.Substring(1).ToLowerInvariant();
        }
        #endregion

        #region CapsWithUnderscoresToWords
        /// <summary>
        /// Converts a string made of word in CAPS_WITH_UNDERSCORES to a series of lowercase words.
        /// </summary>
        /// <param name="str">The source string.</param>
        /// <returns>The resulting string.</returns>
        public static string CapsWithUnderscoresToWords(string str)
        {
            Argument.EnsureNotNull(str, "str");
            return str.ToLowerInvariant().Replace('_', ' ');
        }
        #endregion
        #endregion
    }
}
