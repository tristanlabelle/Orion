using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Reflection;
using System.Globalization;
using Orion.Engine;

namespace Orion.GameLogic.Technologies
{
    /// <summary>
    /// Reads <see cref="Technology">technologies</see> from their definitions in files.
    /// </summary>
    public static class TechnologyReader
    {
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
                UnitStat stat = UnitStat.Parse(effectElement.GetAttribute("Stat"));
                int change = int.Parse(effectElement.GetAttribute("Change"), NumberFormatInfo.InvariantInfo);

                TechnologyEffect effect = new TechnologyEffect(stat, change);
                technologyBuilder.Effects.Add(effect);
            }
        }
        #endregion
    }
}
