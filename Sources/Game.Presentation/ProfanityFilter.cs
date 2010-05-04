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
            regexes = @"adf
ass(hole)?
bitch
c[aâ]li(ss|c)e
cave
chi(e|é|er|enne)
ch?ris(se|t)
con(ne|nard?)?
crotte
cul
[eao]stie?
fiff?(e|on)?
foutre
gu?a(y|i)
laid
m[ae]rde
(mother|mutha)?fuc?k(é|er)?
p[eé]tasse
putain
roux
sacrament
sal(e|ope?)
shit
tab[ae]rna(cle|k|que)
tapette
viarge".Split('\n')
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
