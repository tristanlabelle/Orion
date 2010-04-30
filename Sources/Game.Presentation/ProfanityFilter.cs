using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Orion.Engine;

namespace Orion.Game.Presentation
{
    /// <summary>
    /// Provides methods to remove profanity from user messages.
    /// </summary>
    public static class ProfanityFilter
    {
        private static readonly Regex[] regexes;

        static ProfanityFilter()
        {
            regexes = @"f+u+c*k+
m+[ae]+r+d+e*
r+o+u+x+
c+a+v+e+
l+a+i+d+
s+h+i+t+
f+u+c*k+
s+a+l+(e+|o+p+e*)
c+h+i+(a+s+e+|[ée]+r*)
c+o+n+(e+|a+r+d*)?
g+u*a+(y+|i+)".Split('\n')
              .Select(s => new Regex(@"\b" + s.Trim() + @"\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled))
              .ToArray();
         }

        public static string Filter(string message)
        {
            Argument.EnsureNotNull(message, "message");

            foreach (Regex regex in regexes)
                message = regex.Replace(message, "schtroumpf");

            return message;
        }
    }
}
