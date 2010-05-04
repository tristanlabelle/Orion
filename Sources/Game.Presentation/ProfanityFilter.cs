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
            regexes = @"as(hole)?
bitch
cave
chi(e|é|er)
con(e|ard?)?
(mother|mutha)?fuc?k(é|er)?
gu?a(y|i)
laid
m[ae]rde
roux
sal(e|ope?)
shit
tapette
fiff?(e|on)?".Split('\n')
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
