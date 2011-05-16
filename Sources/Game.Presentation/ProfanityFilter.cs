using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Orion.Engine;
using Orion.Engine.Localization;

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
bio?tche?s?
c[aâ]li(ss|c)es?
caves?
chi(e|é|er|enne|ante?|age)s?
ch?ris(se|t)
con(ne|nard?)?s?
crottes?
culs?
[eao]stie?
enfoir[ée]s?
fag(g[oeu]t)?s?
fiff?(e|on)?s?
foutre(rie)?s?
gu?a(y|i)
laide?s?
m[ae]rdes?
(mother|mutha)?fuc?k(é|er)?s?
p[eé]dales?
p[eé]tasses?
pédés?
pets?
pds?
put(e|aine?)s?
roux
sacrament
sal(e|ope?)
shits?
tab[ae]rna(cle|k|que)
tapettes?
viarges?".Split('\n')
              .Select(s => new Regex(@"\b" + s.Trim() + @"\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled))
              .ToArray();
         }

        public static string Filter(string message, string replacement)
        {
            Argument.EnsureNotNull(message, "message");
            Argument.EnsureNotNull(replacement, "replacement");

            foreach (Regex regex in regexes)
                message = regex.Replace(message, replacement);

            return message;
        }
    }
}
