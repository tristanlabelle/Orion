using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Orion.Engine.Localization;
using System.Globalization;
using Xunit;

namespace Orion.Tests
{
    /// <summary>
    /// Tests the localization classes.
    /// </summary>
    public sealed class LocalizationTests
    {
        private Localizer localizer;

        private void Init()
        {
            localizer = new Localizer();
        }

        private void LoadValidXML()
        {
           localizer.LoadDictionary(System.IO.Directory.GetCurrentDirectory() + "valid_definitions.xml");
        }

        private void LoadValidDictionaryDynamically()
        {
            Definition newDefinition;
            
            //Nouns

            newDefinition = new Definition("Smurf");
            newDefinition.AddTranslation(new Translation("Smurf","en"));
            newDefinition.AddTranslation(new Translation("Schtroumpf","fr",Genders.M));
            localizer.AddNoun(newDefinition);

            newDefinition = new Definition("Smurfette");
            newDefinition.AddTranslation(new Translation("Smurfette", "en"));
            newDefinition.AddTranslation(new Translation("Schtroumpfette", "fr", Genders.F));
            localizer.AddNoun(newDefinition);

            newDefinition = new Definition("Jedihad");
            newDefinition.AddTranslation(new Translation("Jedihad", "en"));
            newDefinition.AddTranslation(new Translation("Jedihad", "fr", Genders.M));
            localizer.AddNoun(newDefinition);

            //Sentences

            newDefinition = new Definition("UnitCreation");
            newDefinition.AddTranslation(new Translation("%n0 spawned.", "en"));
            newDefinition.AddTranslation(new Translation("Un{n0~F?e:} %n0 a ete cree;{n0~F?e}.", "fr"));
            localizer.AddSentence(newDefinition);

            newDefinition = new Definition("ResourceAcquisition");
            newDefinition.AddTranslation(new Translation("%n0 obtained %n1 %n2.", "en"));
            newDefinition.AddTranslation(new Translation("%n0 a obtenu %n1 %n2.", "fr"));
            localizer.AddSentence(newDefinition);

            newDefinition = new Definition("PointAcquisition");
            newDefinition.AddTranslation(new Translation("%n0 received %n1 point{n1>1?s:}.", "en"));
            newDefinition.AddTranslation(new Translation("%n0 a recu %n1 point{n1>1?s}.", "fr"));
            localizer.AddSentence(newDefinition);
        }
        
        [Fact]
        public void TestFailsWhenFileDoesNotExist()
        {
            Init();

            Assert.Throws<FileNotFoundException>(() => localizer.LoadDictionary("tamaman123.xml"));
        }

        //[Fact]
        public void ValidXMLIsLoadedCorrectly()
        {
            Init();
            
            //Assert.DoesNotThrow(() => localizer.LoadDictionary("valid_definitions.xml"));
        }

        [Fact]
        public void NounsAreLoadedCorrectly()
        {
            Init();
            LoadValidDictionaryDynamically();

            Assert.Same("Smurf", localizer.GetNoun("Smurf"));
            Assert.Same("Smurfette", localizer.GetNoun("Smurfette"));
            Assert.Same("Jedihad", localizer.GetNoun("Jedihad"));

            localizer.CultureInfo = new CultureInfo(12);

            Assert.Same("Schtroumpf", localizer.GetNoun("Smurf"));
            Assert.Same("Schtroumpfette", localizer.GetNoun("Smurfette"));
            Assert.Same("Jedihad", localizer.GetNoun("Jedihad"));
        }

        [Fact]
        public void GendersAreLoadedCorrectly()
        {
            Init();
            LoadValidDictionaryDynamically();

            localizer.CultureInfo = new CultureInfo(12);

            Assert.Equal<Genders>
                (
                Genders.M,
                localizer.Nouns["Smurf"].GetTranslation(localizer.CultureInfo).Gender
                );

            Assert.Equal<Genders>
                (
                Genders.F,
                localizer.Nouns["Smurfette"].GetTranslation(localizer.CultureInfo).Gender
                );

            localizer.CultureInfo = new CultureInfo(1033);

            Assert.Equal<Genders>
                (
                Genders.Na,
                localizer.Nouns["Smurf"].GetTranslation(localizer.CultureInfo).Gender
                );

            Assert.Equal<Genders>
                (
                Genders.Na,
                localizer.Nouns["Smurfette"].GetTranslation(localizer.CultureInfo).Gender
                );
        }

        [Fact]
        public void SentencesAreLoadedCorrectly()
        {
            Init();
            LoadValidDictionaryDynamically();

            Assert.Same("UnitCreation", localizer.Sentences["UnitCreation"].Key);
            Assert.Same("ResourceAcquisition", localizer.Sentences["ResourceAcquisition"].Key);
            Assert.Same("PointAcquisition", localizer.Sentences["PointAcquisition"].Key);

            Assert.Same
                (
                "%n0 received %n1 point{n1>1?s:}."
                ,localizer.Sentences["PointAcquisition"].GetTranslation(localizer.CultureInfo).TranslatedString
                );

            Assert.Same
                (
                "%n0 a recu %n1 point{n1>1?s}."
                , localizer.Sentences["PointAcquisition"].GetTranslation(new CultureInfo(12)).TranslatedString
                );
        }
    }
}