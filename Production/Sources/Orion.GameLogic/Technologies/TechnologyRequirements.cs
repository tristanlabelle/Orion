using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Orion.GameLogic.Technologies
{
    /// <summary>
    /// Stores the preconditions that need to be fulfilled in order to develop a technology.
    /// </summary>
    [Serializable]
    public sealed class TechnologyRequirements
    {
        #region Fields
        private readonly int aladdiumCost;
        private readonly int alageneCost;
        private readonly ReadOnlyCollection<Technology> technologies;
        #endregion

        #region Constructors
        public TechnologyRequirements(int aladdiumCost, int alageneCost, IEnumerable<Technology> technologies)
        {
            Argument.EnsurePositive(aladdiumCost, "aladdiumCost");
            Argument.EnsurePositive(alageneCost, "alageneCost");
            Argument.EnsureNotNull(technologies, "technologies");

            this.aladdiumCost = aladdiumCost;
            this.alageneCost = alageneCost;
            this.technologies = technologies.Distinct().ToList().AsReadOnly();
        }

        public TechnologyRequirements(int aladdiumCost, int alageneCost, params Technology[] technologies)
            : this(aladdiumCost, alageneCost, (IEnumerable<Technology>)technologies) { }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the number of aladdium resource points required.
        /// </summary>
        public int AladdiumCost
        {
            get { return aladdiumCost; }
        }

        /// <summary>
        /// Gets the number of alagene resource points required.
        /// </summary>
        public int AlageneCost
        {
            get { return alageneCost; }
        }
        /// <summary>
        /// Gets the sequence of <see cref="Technology">technologies</see> required.
        /// </summary>
        public IEnumerable<Technology> Technologies
        {
            get { return technologies; }
        }

        #endregion

        #region Methods
        public override string ToString()
        {
            return "{0} aladdium, {1} alagene".FormatInvariant(aladdiumCost, alageneCost);
        }
        #endregion
    }
}
