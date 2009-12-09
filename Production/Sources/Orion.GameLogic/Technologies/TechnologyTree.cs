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
            List<Technology> techsToAdd = new List<Technology>();

            //Tier 1 of the tree
            List<TechnologyEffect> techEffects = new List<TechnologyEffect>();

            //HP increasing technology
            techEffects.Add(new TechnologyEffect("hp", UnitStat.MaxHealth, 10));
            TechnologyRequirements techRequirements = new TechnologyRequirements(null, 100, 100);
            techsToAdd.Add(new Technology("Increased Health 1", techRequirements, techEffects, handleGenerator()));
            techEffects.Clear();

            //Attack Power increasing technology
            techEffects.Add(new TechnologyEffect("ap", UnitStat.AttackPower, 2));
            techRequirements = new TechnologyRequirements(null, 120, 100);
            techsToAdd.Add(new Technology("Increased Attack Power 1", techRequirements, techEffects, handleGenerator()));
            techEffects.Clear();

            //Tier 2 of the tree
            foreach (Technology tech in techsToAdd)
                technologies.Add(tech);

            techsToAdd.Clear();

            foreach (Technology tech in technologies)
            {
                if (tech.Name == "Increased Health 1")
                {
                    techEffects.Add(new TechnologyEffect("hp", UnitStat.MaxHealth, 20));
                    List<Technology> required = new List<Technology>();
                    required.Add(tech);
                    techRequirements = new TechnologyRequirements(required, 200, 200);
                    techsToAdd.Add(new Technology("Increased Health 2", techRequirements, techEffects, handleGenerator()));
                    techEffects.Clear();
                }
            }

            foreach (Technology tech in techsToAdd)
                technologies.Add(tech);
        }

        public Technology FindTechnologyFromHandle(Handle handle)
        {
            return technologies.Where(tech => tech.Handle == handle).First();
        }
        #endregion
    }
}
