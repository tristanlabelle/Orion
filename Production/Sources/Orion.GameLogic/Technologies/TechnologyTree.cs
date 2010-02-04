using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.GameLogic.Technologies
{
    /// <summary>
    /// Stores the technologies that can be developped by factions in a game.
    /// </summary>
    public sealed class TechnologyTree
    {
        #region Fields
        private readonly HashSet<Technology> technologies = new HashSet<Technology>();
        private readonly Func<Handle> handleGenerator = Handle.CreateGenerator();
        #endregion

        #region Properties
        public IEnumerable<Technology> Technologies
        {
            get { return technologies; }
        }
        #endregion

        #region Methods
        public void PopulateWithBaseTechnologies()
        {
            Technology increasedSchtroumpfSpeed = new TechnologyBuilder
            {
                Name = "Augmenter la vitesse des schtroumpfs",
                AladdiumCost = 200,
                AlageneCost = 50,
                Predicate = (type => type.Name == "Schtroumpf" || type.Name == "Grand Schtroumpf"),
                Effects = new[] { new TechnologyEffect(UnitStat.MovementSpeed, 2) }
            }.Build(handleGenerator());

            technologies.Add(increasedSchtroumpfSpeed);

            Technology importJeanMarc = new TechnologyBuilder
            {
                Name = "Import Jean-Marc",
                AladdiumCost = 250,
                AlageneCost = 200,
                Predicate = (type => type.Name == "Jean-Marc"),
                Effects = new[] { new TechnologyEffect(UnitStat.AttackPower, 2) }
            }.Build(handleGenerator());

            technologies.Add(importJeanMarc);

            Technology holyTrinity = new TechnologyBuilder
            {
                Name = "Sainte-trinité",
                AladdiumCost = 200,
                AlageneCost = 150,
                Predicate = (type => type.Name == "Jésus" || type.Name == "Jésus-Raptor"),
                Effects = new[]
                {
                    new TechnologyEffect(UnitStat.MaxHealth, 30),
                    new TechnologyEffect(UnitStat.RangedArmor, 1),
                    new TechnologyEffect(UnitStat.MeleeArmor, 1)
                }
            }.Build(handleGenerator());

            technologies.Add(holyTrinity);

            Technology strongSmurf = new TechnologyBuilder
            {
                Name = "Schtroumpf costaud",
                AladdiumCost = 150,
                AlageneCost = 100,
                Predicate = (type => type.Name == "Schtroumpf" || type.Name == "Grand Schtroumpf"),
                Effects = new[]
                {
                    new TechnologyEffect(UnitStat.AttackPower, 1),
                    new TechnologyEffect(UnitStat.MaxHealth, 25),
                    new TechnologyEffect(UnitStat.MeleeArmor, 1)
                }
            }.Build(handleGenerator());

            technologies.Add(strongSmurf);

            Technology spaghettiAlfredo = new TechnologyBuilder
            {
                Name = "Spaghetti Alfredo",
                AladdiumCost = 200,
                AlageneCost = 150,
                Predicate = (type => type.Name == "Flying Spaghetti Monster" || type.Name == "Ta Mère"),
                Effects = new[] { new TechnologyEffect(UnitStat.MovementSpeed, 2) }
            }.Build(handleGenerator());

            technologies.Add(spaghettiAlfredo);

            Technology islamForce = new TechnologyBuilder
            {
                Name = "Islam Force",
                AladdiumCost = 100,
                AlageneCost = 250,
                Predicate = (type => type.Name == "Jedihad" || type.Name == "Allah Skywalker"),
                Effects = new[]
                {
                    new TechnologyEffect(UnitStat.AttackPower, 2),
                    new TechnologyEffect(UnitStat.AttackDelay, -2),
                }
            }.Build(handleGenerator());

            technologies.Add(islamForce);
        }

        public Technology FromHandle(Handle handle)
        {
            return technologies.FirstOrDefault(tech => tech.Handle == handle);
        }
        #endregion
    }
}
