using System;
using System.Collections;
using System.Text;
using System.Globalization;
using System.Diagnostics;
using System.Xml;

namespace Orion.Egine.Localization
{
    /// <summary>
    /// This class allows to load and access definitions 
    /// for nouns and sentences saved in a XML file
    /// using a culture setting to retrieve the matching
    /// translation of the definition.
    /// </summary>
    public class Localizer
    {
        #region Fields
        private CultureInfo cultureInfo;
        private bool isLoaded = false;
        private Hashtable nouns;
        private Hashtable sentences;

        #region XmlConst
        private const string NOUN_DEFINITIONS = "NounDefinitions";
        private const string SENTENCTE_DEFINITIONS = "SentenceDefinitions";
        private const string NOUN = "Noun";
        private const string SENTENCE = "Sentence";
        private const string KEY = "key";
        private const string TRANSLATION = "Translation";
        private const string LANG = "lang";
        private const string GENDER = "gender";
        #endregion
        #endregion

        #region Constructors
        /// <summary>
        /// Main constructor of the Localizer.
        /// Loads definitions from the XML.
        /// Also sets the CultureInfo to English-UnitedStates by default (LCID=1033)
        /// </summary>
        /// <param name="path">The path to the XML file where the definitions are stored.</param>
        public Localizer(string path)
        {
            nouns = new Hashtable();
            sentences = new Hashtable();

            cultureInfo = new CultureInfo(1033); //English-United States

            LoadDictionary(path);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the culture info to be used when retrieving the string value of a definitions.
        /// English LCID = 1033
        /// French LCID = 12
        /// </summary>
        public CultureInfo CultureInfo
        {
            get { return cultureInfo; }
            set { cultureInfo = value; }
        }

        #endregion

        #region Methods
        /// <summary>
        /// Returns the localized string of a noun definition.
        /// The definitions must be loaded before they can be accessed.
        /// </summary>
        /// <param name="key">The unique key used to find the noun.</param>
        /// <returns>The localized string for the noun.</returns>
        public string GetString(string key)
        {
            Debug.Assert(isLoaded);

            Definition matchingDefinition = (Definition)nouns[key];

            return matchingDefinition.GetTranslation(cultureInfo).TranslatedString;
        }

        /// <summary>
        /// Returns the localized and formatted string of a sentence definition.
        /// The definitions must be loaded before they can be accessed.
        /// </summary>
        /// <param name="key">The unique key used to find the sentence.</param>
        /// <param name="values">Parameters to be passed to the sentence. (%0 = value[0])</param>
        /// <returns>The localized and formatted string for the sentence.</returns>
        public string GetString(string key, params string[] values)
        {
            Debug.Assert(isLoaded);
            
            //TODO:
            Definition matchingDefinition = (Definition)nouns[key];

            //- Replace all %nX by their matching values[X]
            //- Parse {} - 2 choices ~ OR > for the moment

            throw new NotImplementedException("COMING SOON!");
        }

        /// <summary>
        /// Loads the definitions.
        /// </summary>
        /// <param name="path">The path to the XML file.</param>
        public void LoadDictionary(string path)
        {
            Debug.Assert(path!=null && path!=string.Empty);

            XmlTextReader xr = new XmlTextReader(path);

            //Main reading loop
            while (xr.Read())
            {
                if (xr.NodeType == XmlNodeType.Element)
                {
                    //If it's a noun definition or a sentence definition
                    if (xr.Name == NOUN || xr.Name == SENTENCE)
                    {
                        bool isNoun = xr.Name == NOUN;

                        //xr.MoveToAttribute(KEY);

                        Definition definition = new Definition(xr.GetAttribute(KEY));

                        //While the noun or sentence is not read completely
                        while(
                            (isNoun && xr.Name != "/"+NOUN && xr.NodeType != XmlNodeType.EndElement) ||
                            (!isNoun && xr.Name != "/"+SENTENCE && xr.NodeType != XmlNodeType.EndElement)
                            )
                        {
                            //If a translation is found
                            if(xr.Name == TRANSLATION && xr.NodeType == XmlNodeType.Element)
                            {
                                string language = string.Empty;
                                Genders gender = Genders.Na;
                                string text = string.Empty;

                                //Read the attributes and the content
                                if(xr.MoveToAttribute(LANG)) 
                                { language = xr.GetAttribute(LANG); } else { throw new Exception("Language is mandatory"); }
                                if (xr.MoveToAttribute(GENDER))
                                {
                                    switch (xr.GetAttribute(GENDER))
                                    {
                                        case "M": gender = Genders.M;  break;
                                        case "F": gender = Genders.F; break;
                                    }
                                }

                                xr.MoveToElement();
                                text = xr.ReadElementContentAsString();

                                //Add the translation to the definition
                                definition.AddTranslation(new Translation(text, language, gender)); 
                            }
                            xr.Read();
                        }//End Translation Reading

                        //If it's a noun, add it to the nouns hashtable, else, to the sentences hashtable
                        if (isNoun)
                        { nouns.Add(definition.Key, definition); }
                        else
                        { sentences.Add(definition.Key, definition); }
                    }//End Noun/Sentence Reading
                }
            }//End Document Reading

            isLoaded = true;
            xr.Close();
        }

        /// <summary>
        /// Clears the dictionary.
        /// </summary>
        public void UnloadDictionary()
        {
            nouns.Clear();
            sentences.Clear();
            isLoaded = false;
        }
        #endregion
    }
}