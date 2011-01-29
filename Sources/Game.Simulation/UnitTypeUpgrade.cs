using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;

namespace Orion.Game.Simulation
{
    /// <summary>
    /// Represents a way a unit type can be upgraded to another type.
    /// </summary>
    public sealed class UnitTypeUpgrade
    {
        #region Fields
        private readonly string target;
        private readonly int aladdiumCost;
        private readonly int alageneCost;
        #endregion

        #region Constructors
        public UnitTypeUpgrade(string target, int aladdiumCost, int alageneCost)
        {
            Argument.EnsureNotNull(target, "target");
            Argument.EnsurePositive(aladdiumCost, "aladdiumCost");
            Argument.EnsurePositive(alageneCost, "alageneCost");
            
            this.target = target;
            this.aladdiumCost = aladdiumCost;
            this.alageneCost = alageneCost;
        }

        public UnitTypeUpgrade(string target)
            : this(target, 0, 0)
        { }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the name of the unit type which is the target of this upgrade.
        /// </summary>
        public string Target
        {
            get { return target; }
        }

        /// <summary>
        /// Gets the cost of the upgrade, in aladdium points.
        /// </summary>
        public int AladdiumCost
        {
            get { return aladdiumCost; }
        }

        /// <summary>
        /// Gets the cost of the upgrade, in alagene points.
        /// </summary>
        public int AlageneCost
        {
            get { return alageneCost; }
        }

        /// <summary>
        /// Gets a value indicating if this upgrade is free.
        /// </summary>
        public bool IsFree
        {
            get { return aladdiumCost + alageneCost == 0; }
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return "Upgrade to{0} {1} for {2} aladdium and {3} alagene"
                .FormatInvariant(target, aladdiumCost, alageneCost);
        }
        #endregion
    }
}
