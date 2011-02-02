using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;

namespace Orion.Engine.Localization
{
    /// <summary>
    /// Represents a definition of a noun (Unit in Orion) or a sentence.
    /// </summary>
    internal class Definition
    {
        #region Fields
        private string key;
        private List<Translation> translations;
        #endregion

        #region Propreties
        public string Key { get { return key; } }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor of the definition.
        /// </summary>
        /// <param name="key">The unique key used to represent the definition.</param>
        public Definition(string key)
        {
            Debug.Assert(key != null);
            this.key = key;
            translations = new List<Translation>();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds a translation to the definition.
        /// </summary>
        /// <param name="translation">The translation to be added.</param>
        public void AddTranslation(Translation translation)
        {
            Debug.Assert(translation != null);
            translations.Add(translation);
        }

        /// <summary>
        /// Returns the translation of the definition for a given language.
        /// </summary>
        /// <param name="cultureInfo">The culture info used to retrieve the translation.</param>
        /// <returns>The translation matching the provided culture info.</returns>
        public Translation GetTranslation(CultureInfo cultureInfo)
        {
            //Sequential search should be change if the application would be to support more than two languages
            foreach(Translation t in translations)
            {
                if (cultureInfo.TwoLetterISOLanguageName == t.Language) return t;
            }
            return null;
        }
        #endregion
    }
}
