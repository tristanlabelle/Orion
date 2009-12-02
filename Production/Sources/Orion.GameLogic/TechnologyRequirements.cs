using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Orion.GameLogic
{
    /// <summary>
    /// Stores the preconditions that need to be fulfilled in order to develop a technology.
    /// </summary>
    [Serializable]
    public sealed class TechnologyRequirements
    {
        #region Fields
        private readonly ReadOnlyCollection<Technology> technologies;
        private readonly int aladdiumCost;
        private readonly int alageneCost;
        #endregion

        #region Constructors
        public TechnologyRequirements(IEnumerable<Technology> technologies, int aladdiumCost, int alageneCost)
        {
            //Argument.EnsureNotNullNorEmpty(technologies, "technologies");
            Argument.EnsurePositive(aladdiumCost, "aladdiumCost");
            Argument.EnsurePositive(alageneCost, "alageneCost");

            if(technologies!= null)
                this.technologies = technologies.Distinct().ToList().AsReadOnly();
            
            //Argument.EnsurePositive(this.technologies.Count, "technologies.Count");
            this.aladdiumCost = aladdiumCost;
            this.alageneCost = alageneCost;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the sequence of <see cref="Technology">technologies</see> required.
        /// </summary>
        public IEnumerable<Technology> Technologies
        {
            get { return technologies; }
        }

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
        #endregion

        #region Methods
        public override string ToString()
        {
            return "{0} alladium, {1} alagene".FormatInvariant(aladdiumCost, alageneCost);
        }
        #endregion
    }
}
