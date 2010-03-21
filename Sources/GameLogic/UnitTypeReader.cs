using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Reflection;
using System.Globalization;
using Orion.Engine;

namespace Orion.GameLogic
{
    /// <summary>
    /// Reads <see cref="UnitType"/>s from their definitions in files.
    /// </summary>
    public static class UnitTypeReader
    {
        #region Methods
        public static UnitTypeBuilder Read(string path)
        {
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                return Read(stream);
        }

        public static UnitTypeBuilder Read(Stream stream)
        {
            Argument.EnsureNotNull(stream, "stream");
            Argument.EnsureEqual(stream.CanRead, true, "stream.CanRead");

            XmlDocument document = new XmlDocument();
            try { document.Load(stream); }
            catch (XmlException e)
            {
                throw new InvalidDataException("Invalid unit type file.", e);
            }

            UnitTypeBuilder unitTypeBuilder = new UnitTypeBuilder();

            XmlElement unitTypeElement = document.DocumentElement;
            if (unitTypeElement.Name != "UnitType")
                throw new InvalidDataException("Expected a root <UnitType> tag.");

            ReadUnitTypeAttributes(unitTypeElement, unitTypeBuilder);
            ReadSkills(unitTypeElement, unitTypeBuilder);

            return unitTypeBuilder;
        }

        private static void ReadUnitTypeAttributes(XmlElement unitTypeElement, UnitTypeBuilder unitTypeBuilder)
        {
            foreach (XmlAttribute attribute in unitTypeElement.Attributes)
            {
                PropertyInfo property = typeof(UnitTypeBuilder).GetProperty(attribute.Name,
                    BindingFlags.Public | BindingFlags.Instance);
                if (property == null)
                    throw new InvalidDataException("Invalid UnitType attribute {0}.".FormatInvariant(attribute.Name));

                object value;
                try { value = Convert.ChangeType(attribute.Value, property.PropertyType, CultureInfo.InvariantCulture); }
                catch (InvalidCastException e)
                {
                    throw new InvalidDataException(
                        "Invalid value for UnitType attribute {0}, expected type {1}."
                        .FormatInvariant(attribute.Name, property.PropertyType.Name), e);
                }

                property.SetValue(unitTypeBuilder, value, null);
            }
        }

        private static void ReadSkills(XmlElement unitTypeElement, UnitTypeBuilder unitTypeBuilder)
        {
            foreach (XmlElement skillElement in unitTypeElement.ChildNodes)
            {
                string skillName = skillElement.Name;
                UnitSkill? skill = null;
                if (skillName != "Stats")
                {
                    try { skill = (UnitSkill)Enum.Parse(typeof(UnitSkill), skillName); }
                    catch (ArgumentException e)
                    {
                        throw new InvalidDataException(
                            "Invalid unit skill {0}.".FormatInvariant(skillName), e);
                    }
                }

                if (skill.HasValue) unitTypeBuilder.Skills.Add(skill.Value);

                ReadSkillStats(skill, skillElement, unitTypeBuilder);
                ReadSkillTargets(skill, skillElement, unitTypeBuilder);
            }
        }

        private static void ReadSkillStats(UnitSkill? skill, XmlElement skillElement, UnitTypeBuilder unitTypeBuilder)
        {
            foreach (XmlAttribute statAttribute in skillElement.Attributes)
            {
                UnitStat unitStat = UnitStat.Values
                    .FirstOrDefault(stat => stat.Name == statAttribute.Name
                        || stat.Name == skillElement.Name + statAttribute.Name);
                if (unitStat == null)
                {
                    throw new InvalidDataException(
                        "Invalid unit stat {0} for skill {1}.".FormatInvariant(statAttribute.Name, skillElement.Name));
                }

                UnitSkill? associatedSkill = unitStat.HasAssociatedSkill ? (UnitSkill?)unitStat.AssociatedSkill : null;
                if (associatedSkill != skill)
                {
                    throw new InvalidDataException(
                        "Invalid unit stat {0} for skill {1}.".FormatInvariant(statAttribute.Name, skillElement.Name));
                }

                int value;
                if (!int.TryParse(statAttribute.Value, NumberStyles.None, NumberFormatInfo.InvariantInfo, out value))
                {
                    throw new InvalidDataException(
                        "{0} stat value is not a positive integer.".FormatInvariant(statAttribute.Name));
                }

                unitTypeBuilder.Stats.Add(unitStat, value);
            }
        }

        private static void ReadSkillTargets(UnitSkill? skill, XmlElement skillElement, UnitTypeBuilder unitTypeBuilder)
        {
            foreach (XmlElement targetElement in skillElement.SelectNodes("Target"))
            {
                PropertyInfo property = null;
                if (skill.HasValue)
                {
                    property = typeof(UnitTypeBuilder)
                        .GetProperty(skill.Value + "Targets", BindingFlags.Public | BindingFlags.Instance);
                }

                if (property == null)
                {
                    throw new InvalidDataException(
                        "{0} skill does not support targets."
                        .FormatInvariant(skill.HasValue ? skill.Value.ToString() : "Default"));
                }

                var targets = (ICollection<string>)property.GetValue(unitTypeBuilder, null);
                targets.Add(targetElement.InnerText);
            }
        }
        #endregion
    }
}
