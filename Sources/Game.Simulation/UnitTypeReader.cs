using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Reflection;
using System.Globalization;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Game.Simulation.Skills;
using System.Diagnostics;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Simulation
{
    /// <summary>
    /// Reads <see cref="Entity"/>s from their definitions in files.
    /// </summary>
    public static class UnitTypeReader
    {
        #region Methods
        public static UnitTypeBuilder Read(string path)
        {
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                UnitTypeBuilder unitTypeBuilder = Read(stream);

#if DEBUG
                if (unitTypeBuilder.Name != Path.GetFileNameWithoutExtension(path))
                {
                    Debug.Fail("Unit {0} is defined in a file with a different name.".FormatInvariant(unitTypeBuilder.Name));
                }
#endif

                return unitTypeBuilder;
            }
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

            UnitTypeBuilder builder = new UnitTypeBuilder();

            XmlElement unitTypeElement = document.DocumentElement;
            if (unitTypeElement.Name != "UnitType")
                throw new InvalidDataException("Expected a root <UnitType> tag.");

            ReadUnitTypeAttributes(unitTypeElement, builder);

            XmlElement skillsElement = (XmlElement)unitTypeElement.SelectSingleNode("Skills");
            if (skillsElement != null) ReadSkills(skillsElement, builder);

            XmlElement upgradesElement = (XmlElement)unitTypeElement.SelectSingleNode("Upgrades");
            if (upgradesElement != null) ReadUpgrades(upgradesElement, builder);

            return builder;
        }

        private static void ReadUnitTypeAttributes(XmlElement unitTypeElement, UnitTypeBuilder builder)
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
                        .FormatInvariant(attribute.Name, property.PropertyType.FullName), e);
                }

                property.SetValue(builder, value, null);
            }
        }

        #region Skills
        private static void ReadSkills(XmlElement skillsElement, UnitTypeBuilder builder)
        {
            foreach (XmlElement skillElement in skillsElement.ChildNodes)
            {
                string skillName = skillElement.Name;

                UnitSkill skill = null;
                if (skillName == UnitSkill.GetTypeName(typeof(BasicSkill)))
                {
                    skill = builder.BasicSkill;
                }
                else
                {
                    Type skillType = UnitSkill.GetTypeFromName(skillName);
                    if (skillType == null)
                    {
                        throw new InvalidDataException(
                            "Invalid unit skill {0}.".FormatInvariant(skillName));
                    }

                    if (builder.Skills.Any(s => s.GetType() == skillType))
                    {
                        throw new InvalidDataException("Redefined unit skill {0}."
                            .FormatInvariant(skillName));
                    }

                    skill = (UnitSkill)Activator.CreateInstance(skillType);
                    builder.Skills.Add(skill);
                }

                ReadSkillStats(skill, skillElement, builder);
                ReadSkillTargets(skill, skillElement, builder);
            }
        }

        private static void ReadSkillStats(UnitSkill skill, XmlElement skillElement, UnitTypeBuilder builder)
        {
            foreach (XmlAttribute statAttribute in skillElement.Attributes)
            {
                if (statAttribute.Name == "EffectiveAgainst")
                {
                    if (statAttribute.Value.Length > 0)
                    {
                        var types = statAttribute.Value.Split(' ')
                            .Select(str => (ArmorType)Enum.Parse(typeof(ArmorType), str, true));
                        ((AttackSkill)skill).SuperEffectiveAgainst.AddRange(types);
                    }
                }
                else if (statAttribute.Name == "IneffectiveAgainst")
                {
                    if (statAttribute.Value.Length > 0)
                    {
                        var types = statAttribute.Value.Split(' ')
                            .Select(str => (ArmorType)Enum.Parse(typeof(ArmorType), str, true));
                        ((AttackSkill)skill).IneffectiveAgainst.AddRange(types);
                    }
                }
                else
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
                        if (statAttribute.Name == "ArmorType")
                        {
                            value = (int)Enum.Parse(typeof(ArmorType), statAttribute.Value, true);
                        }
                        else
                        {
                            throw new InvalidDataException(
                                "{0} stat value is not valid a positive integer."
                                .FormatInvariant(statAttribute.Name));
                        }
                    }

                    skill.SetStat(stat, value);
                }
            }
        }

        private static void ReadSkillTargets(UnitSkill skill, XmlElement skillElement, UnitTypeBuilder builder)
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

        #region Upgrades
        private static void ReadUpgrades(XmlElement upgradesElement, UnitTypeBuilder builder)
        {
            var parameters = typeof(UnitTypeUpgrade)
                .GetConstructors()[0]
                .GetParameters();
            object[] values = new object[parameters.Length];

            foreach (XmlElement upgradeElement in upgradesElement.SelectNodes("Upgrade"))
            {
                Array.Clear(values, 0, values.Length);

                foreach (XmlAttribute attribute in upgradeElement.Attributes)
                {
                    string constructorParameterName = char.ToLowerInvariant(attribute.Name[0]) + attribute.Name.Substring(1);

                    int parameterIndex = parameters.IndexOf(p => p.Name == constructorParameterName);
                    if (parameterIndex == -1)
                    {
                        throw new InvalidDataException(
                            "Invalid unit type upgrade element, unrecognized attribute {0}."
                            .FormatInvariant(attribute.Name));
                    }

                    if (values[parameterIndex] != null)
                    {
                        throw new InvalidDataException(
                            "Invalid unit type upgrade element, attribute {0} already specified."
                            .FormatInvariant(attribute.Name));
                    }

                    var parameter = parameters[parameterIndex];
                    try
                    {
                        values[parameterIndex] = Convert.ChangeType(attribute.Value,
                            parameter.ParameterType,
                            CultureInfo.InvariantCulture);
                    }
                    catch (InvalidCastException e)
                    {
                        throw new InvalidDataException(
                            "Invalid value for upgrade attribute {0}, expected type {1}."
                            .FormatInvariant(attribute.Name, parameter.ParameterType.FullName), e);
                    }
                }

                int firstUnspecifiedParameterIndex = values.IndexOf((object)null);
                if (firstUnspecifiedParameterIndex != -1)
                {
                    string unspecifiedParameterName = parameters[firstUnspecifiedParameterIndex].Name;
                    string unspecifiedAttributeName = char.ToUpperInvariant(unspecifiedParameterName[0]) + unspecifiedParameterName.Substring(1);
                    throw new InvalidDataException(
                        "Upgrade does not specify a value for attribute {0}."
                        .FormatInvariant(unspecifiedAttributeName));
                }

                UnitTypeUpgrade upgrade = (UnitTypeUpgrade)Activator.CreateInstance(typeof(UnitTypeUpgrade), values);
                builder.Upgrades.Add(upgrade);
            }
        }
        #endregion
        #endregion
    }
}
