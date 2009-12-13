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
                Name = "Increased Schtroumpf Speed",
                AladdiumCost = 100,
                AlageneCost = 100,
                Effects = new[] { new TechnologyEffect(unitType => unitType.Name == "Schtroumpf", UnitStat.MovementSpeed, 4) }
            }.Build(handleGenerator());

            technologies.Add(increasedSchtroumpfSpeed);

            Technology importJeanMarc = new TechnologyBuilder
            {
                Name = "Import Jean-Marc",
                AladdiumCost = 500,
                AlageneCost = 200,
                Effects = new[] { new TechnologyEffect(unitType => unitType.Name == "Jean-Marc", UnitStat.AttackPower, 4) }
            }.Build(handleGenerator());

            technologies.Add(importJeanMarc);
        }

        public Technology FromHandle(Handle handle)
        {
            return technologies.FirstOrDefault(tech => tech.Handle == handle);
        }
        #endregion
    }
}
