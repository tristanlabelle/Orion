using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using Orion.Engine;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Simulation.Technologies
{
    /// <summary>
    /// Reads <see cref="Technology">technologies</see> from their definitions in files.
    /// </summary>
    public static class TechnologyReader
    {
        #region Fields
        private static readonly Regex statNameRegex = new Regex(
            @"\A ([a-zA-Z]+) \. ([a-zA-Z]+) \Z",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace);
        #endregion

        #region Methods
        public static TechnologyBuilder Read(string path)
        {
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                return Read(stream);
        }

        public static TechnologyBuilder Read(Stream stream)
        {
            Argument.EnsureNotNull(stream, "stream");
            Argument.EnsureEqual(stream.CanRead, true, "stream.CanRead");

            XmlDocument document = new XmlDocument();
            try { document.Load(stream); }
            catch (XmlException e)
            {
                throw new InvalidDataException("Invalid technology file.", e);
            }

            TechnologyBuilder technologyBuilder = new TechnologyBuilder();

            XmlElement technologyElement = document.DocumentElement;
            if (technologyElement.Name != "Technology")
                throw new InvalidDataException("Expected a root <Technology> tag.");

            ReadTechnologyAttributes(technologyElement, technologyBuilder);
            ReadTargets(technologyElement, technologyBuilder);
            ReadEffects(technologyElement, technologyBuilder);

            return technologyBuilder;
        }

        private static void ReadTechnologyAttributes(XmlElement technologyElement, TechnologyBuilder technologyBuilder)
        {
            foreach (XmlAttribute attribute in technologyElement.Attributes)
            {
                PropertyInfo property = typeof(TechnologyBuilder).GetProperty(attribute.Name,
                    BindingFlags.Public | BindingFlags.Instance);
                if (property == null)
                    throw new InvalidDataException("Invalid technology attribute {0}.".FormatInvariant(attribute.Name));

                object value;
                try { value = Convert.ChangeType(attribute.Value, property.PropertyType, CultureInfo.InvariantCulture); }
                catch (InvalidCastException e)
                {
                    throw new InvalidDataException(
                        "Invalid value for technology attribute {0}, expected type {1}."
                        .FormatInvariant(attribute.Name, property.PropertyType.Name), e);
                }

                property.SetValue(technologyBuilder, value, null);
            }
        }

        private static void ReadTargets(XmlElement technologyElement, TechnologyBuilder technologyBuilder)
        {
            foreach (XmlElement targetElement in technologyElement.SelectNodes("Target"))
                technologyBuilder.Targets.Add(targetElement.InnerText);
        }

        private static void ReadEffects(XmlElement technologyElement, TechnologyBuilder technologyBuilder)
        {
            foreach (XmlElement effectElement in technologyElement.SelectNodes("Effect"))
            {
                string fullStatName = effectElement.GetAttribute("Stat");
                Match match = statNameRegex.Match(fullStatName);
                if (!match.Success)
                {
                    Debug.Fail("Invalid stat name format: " + fullStatName);
                    continue;
                }

                string componentName = match.Groups[1].Value;
                Type componentType = Assembly.GetExecutingAssembly().GetType(typeof(Identity).Namespace + "." + componentName, false, false);
                if (componentType == null)
                {
                    Debug.Fail("No such component for stat " + fullStatName);
                    continue;
                }

                string statName = match.Groups[2].Value;
                FieldInfo statField = componentType.GetField(statName + "Stat", BindingFlags.Public | BindingFlags.Static);
                if (statField == null)
                {
                    Debug.Fail("No such component stat for stat " + fullStatName);
                    continue;
                }

                Stat stat = (Stat)statField.GetValue(null);

                string deltaString = effectElement.GetAttribute("Delta");
                if (deltaString == null)
                {
                    Debug.Fail("Effect has no Delta attribute");
                    continue;
                }

                StatValue delta;
                try
                {
                    delta = stat.Type == StatType.Integer
                        ? StatValue.CreateInteger(int.Parse(deltaString, NumberFormatInfo.InvariantInfo))
                        : StatValue.CreateReal(float.Parse(deltaString, NumberFormatInfo.InvariantInfo));
                }
                catch (FormatException)
                {
                    Debug.Fail("Invalid effect Delta value:" + deltaString);
                    continue;
                }

                TechnologyEffect effect = new TechnologyEffect(stat, delta);
                technologyBuilder.Effects.Add(effect);
            }
        }
        #endregion
    }
}
