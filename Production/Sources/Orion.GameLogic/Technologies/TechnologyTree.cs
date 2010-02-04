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
                Effects = new[] 
                {
                    new TechnologyEffect(unitType => unitType.Name == "Schtroumpf", UnitStat.MovementSpeed, 2),
                    new TechnologyEffect(unitType => unitType.Name == "Grand Schtroumpf", UnitStat.MovementSpeed, 2)
                }
            }.Build(handleGenerator());

            technologies.Add(increasedSchtroumpfSpeed);

            Technology importJeanMarc = new TechnologyBuilder
            {
                Name = "Rechercher Jean-Marc",
                AladdiumCost = 250,
                AlageneCost = 200,
                Effects = new[] { new TechnologyEffect(unitType => unitType.Name == "Jean-Marc", UnitStat.AttackPower, 2) }
            }.Build(handleGenerator());

            technologies.Add(importJeanMarc);

            Technology holyTrinity = new TechnologyBuilder
            {
                Name = "Sainte-trinité",
                AladdiumCost = 200,
                AlageneCost = 150,
                Effects = new[]
                {
                    new TechnologyEffect(unitType => unitType.Name == "Jésus", UnitStat.MaxHealth, 30),
                    new TechnologyEffect(unitType => unitType.Name == "Jésus", UnitStat.RangedArmor, 1),
                    new TechnologyEffect(unitType => unitType.Name == "Jésus", UnitStat.MeleeArmor, 1),
                    new TechnologyEffect(unitType => unitType.Name == "Jésus-Raptor", UnitStat.MaxHealth, 30),
                    new TechnologyEffect(unitType => unitType.Name == "Jésus-Raptor", UnitStat.RangedArmor, 1),
                    new TechnologyEffect(unitType => unitType.Name == "Jésus-Raptor", UnitStat.MeleeArmor, 1)
                }
            }.Build(handleGenerator());

            technologies.Add(holyTrinity);

            Technology strongSmurf = new TechnologyBuilder
            {
                Name = "Schtroumpf costaud",
                AladdiumCost = 150,
                AlageneCost = 100,
                Effects = new[]
                {
                    new TechnologyEffect(unitType => unitType.Name == "Schtroumpf", UnitStat.AttackPower, 1),
                    new TechnologyEffect(unitType => unitType.Name == "Schtroumpf", UnitStat.MaxHealth, 25),
                    new TechnologyEffect(unitType => unitType.Name == "Schtroumpf", UnitStat.MeleeArmor, 1),
                    new TechnologyEffect(unitType => unitType.Name == "Grand Schtroumpf", UnitStat.AttackPower, 1),
                    new TechnologyEffect(unitType => unitType.Name == "Grand Schtroumpf", UnitStat.MaxHealth, 25),
                    new TechnologyEffect(unitType => unitType.Name == "Grand Schtroumpf", UnitStat.MeleeArmor, 1)
                }
            }.Build(handleGenerator());

            technologies.Add(strongSmurf);

            Technology spaghettiAlfredo = new TechnologyBuilder
            {
                Name = "Spaghetti Alfredo",
                AladdiumCost = 200,
                AlageneCost = 150,
                Effects = new[]
                {
                    new TechnologyEffect(unitType => unitType.Name == "Flying Spaghetti Monster", UnitStat.MovementSpeed, 2),
                    new TechnologyEffect(unitType => unitType.Name == "Ta Mère", UnitStat.MovementSpeed, 2)
                }
            }.Build(handleGenerator());

            technologies.Add(spaghettiAlfredo);
        }

        public Technology FromHandle(Handle handle)
        {
            return technologies.FirstOrDefault(tech => tech.Handle == handle);
        }
        #endregion
    }
}
