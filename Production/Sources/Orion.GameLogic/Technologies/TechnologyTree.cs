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
                Effects = new[] { new TechnologyEffect(unitType => unitType.Name == "Schtroumpf", UnitStat.MovementSpeed, 2) }
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
                Name = "Sainte-Trinité",
                AladdiumCost = 200,
                AlageneCost = 150,
                Effects = new[]
                {
                    new TechnologyEffect(unitType => unitType.Name == "Jésus", UnitStat.RangedArmor, 1),
                    new TechnologyEffect(unitType => unitType.Name == "Jésus", UnitStat.MeleeArmor, 1)
                }
            }.Build(handleGenerator());

            technologies.Add(holyTrinity);
        }

        public Technology FromHandle(Handle handle)
        {
            return technologies.FirstOrDefault(tech => tech.Handle == handle);
        }
        #endregion
    }
}
