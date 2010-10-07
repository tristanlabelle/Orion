using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Orion.Engine.Networking.Http
{
    public static class HttpEnumMethods
    {
        #region Methods
        #region Private
        private static IEnumerable<int> HeaderCaseChanges(string that)
        {
            for (int i = 1; i < that.Length; i++)
                if (Char.IsUpper(that[i])) yield return i;
            yield return that.Length;
        }

        private static string ToHeaderCase(string headerName)
        {
            StringBuilder result = new StringBuilder();
            int lastMatch = 0;
            foreach (int location in HeaderCaseChanges(headerName))
            {
                if (!Char.IsUpper(headerName[location - 1]))
                {
                    if (result.Length > 0) result.Append('-');
                    result.Append(headerName.Substring(lastMatch, location - lastMatch));
                    lastMatch = location;
                }
            }

            return result.ToString();
        }
        #endregion

        public static string ToString(HttpRequestHeader header)
        {
            return ToHeaderCase(header.ToString("g"));
        }

        public static string ToString(HttpResponseHeader header)
        {
            return ToHeaderCase(header.ToString("g"));
        }

        public static string ToString(HttpRequestMethod method)
        {
            return method.ToString("g").ToUpperInvariant();
        }
        #endregion
    }
}
