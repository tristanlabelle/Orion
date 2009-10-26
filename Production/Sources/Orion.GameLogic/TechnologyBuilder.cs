using System;
using System.Collections.Generic;

namespace Orion.GameLogic
{
    /// <summary>
    /// Provides a mutable equivalent to <see cref="Technology"/> that can be used to build such objects.
    /// </summary>
    [Serializable]
    public sealed class TechnologyBuilder
    {
        #region Fields
        private string name;
        private HashSet<Technology> requiredTechnologies = new HashSet<Technology>();
        private int aladdiumCost;
        private int alageneCost;
        private HashSet<TechnologyEffect> effects = new HashSet<TechnologyEffect>();
        #endregion

        #region Properties
        public string Name
        {
            get { return name; }
            set
            {
                Argument.EnsureNotNullNorBlank(value, "Name");
                name = value;
            }
        }

        public ICollection<Technology> RequiredTechnologies
        {
            get { return requiredTechnologies; }
        }

        public int AladdiumCost
        {
            get { return aladdiumCost; }
            set
            {
                Argument.EnsurePositive(value, "AladdiumCost");
                aladdiumCost = value;
            }
        }

        public int AlageneCost
        {
            get { return alageneCost; }
            set
            {
                Argument.EnsurePositive(value, "AlageneCost");
                alageneCost = value;
            }
        }

        public ICollection<TechnologyEffect> Effects
        {
            get { return effects; }
        }
        #endregion

        #region Methods
        public void Reset()
        {
            name = null;
            requiredTechnologies.Clear();
            aladdiumCost = 0;
            alageneCost = 0;
            effects.Clear();
        }

        public TechnologyRequirements BuildRequirements()
        {
            return new TechnologyRequirements(requiredTechnologies, aladdiumCost, alageneCost);
        }

        public Technology Build()
        {
            if (name == null) throw new InvalidOperationException("A name must be set before the technology can be built.");

            TechnologyRequirements requirements = BuildRequirements();
            return new Technology(name, requirements, effects);
        }
        #endregion
    }
}
