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
bitch(es)?
c[aâ]li(ss|c)e
caves?
chi(e|é|er|enne)s?
ch?ris(se|t)
con(ne|nard?)?s?
crottes?
culs?
[eao]stie?
enfoir(é|e)
fiff?(e|on)?
foutre
gu?a(y|i)
laide?s?
m[ae]rdes?
(mother|mutha)?fuc?k(é|er)?s?
p[eé]tasses?
pédés?
pd
put(e|aine?)s?
roux
sacrament
sal(e|ope?)
shit
tab[ae]rna(cle|k|que)
tapettes?
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
