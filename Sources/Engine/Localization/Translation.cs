using System;
using System.Globalization;
using System.Text;

namespace Orion.Egine.Localization
{
    /// <summary>
    /// Represents a translation of a definition.
    /// </summary>
    internal class Translation
    {
        #region Fields
        private string translatedString;
        private string language;
        private Genders gender;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor of the translation (default gender is "Na" for nouns).
        /// </summary>
        /// <param name="translatedString">The translated string.</param>
        /// <param name="language">The language of the string.</param>
        public Translation(string translatedString, string language)
            : this(translatedString, language, Genders.Na)
        {
        }

        /// <summary>
        /// Constructor of the translation.
        /// </summary>
        /// <param name="translatedString">The translated string.</param>
        /// <param name="language">The language of the string.</param>
        /// <param name="gender">The gender of the string (for noun definitions)</param>
        public Translation(string translatedString, string language, Genders gender)
        {
            this.translatedString = translatedString;
            this.language = language;
            this.gender = gender;
        }
        #endregion

        #region Propreties
        /// <summary>
        /// Gets the translated string of the translation without formatting.
        /// </summary>
        public string TranslatedString { get { return translatedString; } }

        /// <summary>
        /// Gets the language of the translation. (Two letter ISO : English = "en", French = "fr")
        /// </summary>
        public string Language { get { return language; } }

        /// <summary>
        /// Gets the gender of the translation (generally applicable for noun translations).
        /// </summary>
        public Genders Gender { get { return gender; } }
        #endregion

        #region Methods
        #endregion
    }
}