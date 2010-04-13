using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Reflection;
using System.Globalization;
using Orion.Engine;
using Orion.Game.Simulation.Skills;

namespace Orion.Game.Simulation
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

            XmlElement skillsElement = (XmlElement)unitTypeElement.SelectSingleNode("Skills");
            if (skillsElement != null) ReadSkills(skillsElement, unitTypeBuilder);

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

        private static void ReadSkills(XmlElement skillsElement, UnitTypeBuilder unitTypeBuilder)
        {
            foreach (XmlElement skillElement in skillsElement.ChildNodes)
            {
                string skillName = skillElement.Name;

                UnitSkill skill = null;
                if (skillName == UnitSkill.GetTypeName(typeof(BasicSkill)))
                {
                    skill = unitTypeBuilder.BasicSkill;
                }
                else
                {
                    Type skillType = UnitSkill.GetTypeFromName(skillName);
                    if (skillType == null)
                    {
                        throw new InvalidDataException(
                            "Invalid unit skill {0}.".FormatInvariant(skillName));
                    }

                    if (unitTypeBuilder.Skills.Any(s => s.GetType() == skillType))
                    {
                        throw new InvalidDataException("Redefined unit skill {0}."
                            .FormatInvariant(skillName));
                    }

                    skill = (UnitSkill)Activator.CreateInstance(skillType);
                    unitTypeBuilder.Skills.Add(skill);
                }

                ReadSkillStats(skill, skillElement, unitTypeBuilder);
                ReadSkillTargets(skill, skillElement, unitTypeBuilder);
            }
        }

        private static void ReadSkillStats(UnitSkill skill, XmlElement skillElement, UnitTypeBuilder unitTypeBuilder)
        {
            foreach (XmlAttribute statAttribute in skillElement.Attributes)
            {
                UnitStat stat = UnitSkill.GetStat(skill, statAttribute.Name);
                if (stat == null)
                {
                    throw new InvalidDataException(
                        "Invalid unit stat {0} for skill {1}."
                        .FormatInvariant(statAttribute.Name, UnitSkill.GetTypeName(skill)));
                }

                int value;
                if (!int.TryParse(statAttribute.Value, NumberStyles.None, NumberFormatInfo.InvariantInfo, out value))
                {
                    throw new InvalidDataException(
                        "{0} stat value is not valid a positive integer."
                        .FormatInvariant(statAttribute.Name));
                }

                skill.SetStat(stat, value);
            }
        }

        private static void ReadSkillTargets(UnitSkill skill, XmlElement skillElement, UnitTypeBuilder unitTypeBuilder)
        {
            PropertyInfo property = skill.GetType()
                .GetProperty("Targets", BindingFlags.Public | BindingFlags.Instance);
            var targets = property == null ? null : (ICollection<string>) property.GetValue(skill, null);

            foreach (XmlElement targetElement in skillElement.SelectNodes("Target"))
            {
                if (targets == null)
                {
                    throw new InvalidDataException(
                        "{0} skill does not support targets."
                        .FormatInvariant(UnitSkill.GetTypeName(skill)));
                }

                targets.Add(targetElement.InnerText);
            }
        }
        #endregion
    }
}
