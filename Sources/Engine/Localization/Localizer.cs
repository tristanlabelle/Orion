using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Diagnostics;
using System.Xml;

namespace Orion.Engine.Localization
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

        private static readonly Regex objArgPattern
            = new Regex(@"%n[0-9]", RegexOptions.Compiled);

        private static readonly Regex objGenderPattern 
            = new Regex(@"\{n[0-9]~(M|F|N)\?.*?:?.*?\}", RegexOptions.Compiled);

        private static readonly Regex subReplaceValuesPattern 
            = new Regex(@".*\?(.*?:?.*?)\}.*", RegexOptions.Compiled);

        private Dictionary<string, Definition> nouns;
        private Dictionary<string, Definition> sentences;

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
        public Localizer()
        {
            nouns = new Dictionary<string, Definition>();
            sentences = new Dictionary<string, Definition>();

            cultureInfo = new CultureInfo(1033); //English-United States
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

        /// <summary>
        /// Gets the dictionary that contains the noun definitions.
        /// </summary>
        public Dictionary<string, Definition> Nouns
        {
            get { return nouns; }
        }

        /// <summary>
        /// Gets the dictionary that contains the sentence definitions.
        /// </summary>
        public Dictionary<string, Definition> Sentences
        {
            get { return sentences; }
        }

        #endregion

        #region Methods
        /// <summary>
        /// Returns the localized string of a noun definition.
        /// The definitions must be loaded before they can be accessed.
        /// </summary>
        /// <param name="key">The unique key used to find the noun.</param>
        /// <returns>The localized string for the noun.</returns>
        public string GetNoun(string key)
        {
            Definition matchingDefinition = nouns[key];

            return matchingDefinition.GetTranslation(cultureInfo).TranslatedString;
        }

        /// <summary>
        /// Returns the localized and formatted string of a sentence definition.
        /// The definitions must be loaded before they can be accessed.
        /// </summary>
        /// <param name="key">The unique key used to find the sentence.</param>
        /// <param name="values">Parameters to be passed to the sentence. (%n0 = value[0])</param>
        /// <returns>The localized and formatted string for the sentence.</returns>
        public string GetSentence(string key, params string[] values)
        {   
            Definition matchingDefinition = (Definition)nouns[key];

            string input = matchingDefinition.GetTranslation(this.cultureInfo).TranslatedString;
            StringBuilder output = new StringBuilder(input);

            //Noun Parsing
            //- Replace all %nX by their matching values[X]

            foreach (Match match in objArgPattern.Matches(input))
            {
                int argIndex = int.Parse(match.Value.Substring(2, 1)); //Position of the arg number
                output = output.Replace(match.Value, values[argIndex].ToString());
            }

            //Gender Parsing
            //- Replace all {n0~F?xxx:yyy} by the value before or after the ":" using the gender after the "~"

            foreach (Match match in objGenderPattern.Matches(input))
            {
                int argIndex = int.Parse(match.Value.Substring(2, 1)); //Position of the arg number
                Translation argTranslation = nouns[values[argIndex]].GetTranslation(cultureInfo);
                Debug.Assert(argTranslation != null);

                Genders gender;
                switch (match.Value.Substring(4, 1))
                {
                    case "M": gender = Genders.M; break;
                    case "F": gender = Genders.F; break;
                    default: gender = Genders.Na; break;
                }

                bool genderMatches = argTranslation.Gender == gender;
                bool expContainsColon = match.Value.Contains(":");

                string[] replaceValues = subReplaceValuesPattern.Replace(match.Value, "$1").Split(':');

                if (genderMatches)
                {
                    output = output.Replace(match.Value, replaceValues[0]);
                }
                else if (!genderMatches && expContainsColon)
                {
                    output = output.Replace(match.Value, replaceValues[1]);
                }
                else if (!genderMatches && !expContainsColon) //NAND
                {
                    output = output.Replace(match.Value, string.Empty);
                }
            }

            return output.ToString();
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

            xr.Close();
        }

        /// <summary>
        /// Clears the dictionary.
        /// </summary>
        public void UnloadDictionary()
        {
            nouns.Clear();
            sentences.Clear();
        }

        /// <summary>
        /// Adds a noun to the dictionary.
        /// </summary>
        /// <param name="nounDefinition">The noun definition to be added.</param>
        public void AddNoun(Definition nounDefinition)
        {
            nouns.Add(nounDefinition.Key, nounDefinition);
        }

        /// <summary>
        /// Adds a sentence to the dictionary.
        /// </summary>
        /// <param name="sentenceDefinition">The sentence to be added.</param>
        public void AddSentence(Definition sentenceDefinition)
        {
            sentences.Add(sentenceDefinition.Key,sentenceDefinition);
        }
        #endregion
    }
}