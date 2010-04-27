using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;

namespace Orion.Game.Simulation.Skills
{
    /// <summary>
    /// A skill which allows a <see cref="UnitType"/> to be sold.
    /// </summary>
    [Serializable]
    public sealed class SellableSkill : UnitSkill
    {
        #region Fields
        public static readonly UnitStat AlageneValueStat = new UnitStat(typeof(SellableSkill), "AlageneValue", "Valeur en alagène");
        public static readonly UnitStat AladdiumValueStat = new UnitStat(typeof(SellableSkill), "AladdiumValue", "Valeur en aladdium");

        private int alageneValue;
        private int aladdiumValue;
        #endregion

        #region Properties
        /// <summary>
        /// Gets value of this unit type when reselling, in alagene points.
        /// </summary>
        public int AlageneValue
        {
            get { return alageneValue; }
            set
            {
                EnsureNotFrozen();
                Argument.EnsurePositive(value, "AlageneValue");
                alageneValue = value;
            }
        }

        /// <summary>
        /// Gets value of this unit type when reselling, in aladdium points.
        /// </summary>
        public int AladdiumValue
        {
            get { return aladdiumValue; }
            set
            {
                EnsureNotFrozen();
                Argument.EnsurePositive(value, "AladdiumValue");
                aladdiumValue = value;
            }
        }
        #endregion

        #region Methods
        protected override UnitSkill Clone()
        {
            return new SellableSkill
            {
                aladdiumValue = aladdiumValue,
                alageneValue = alageneValue
            };
        }

        public override int GetStat(UnitStat stat)
        {
            if (stat == AlageneValueStat) return alageneValue;
            if (stat == AladdiumValueStat) return aladdiumValue;
            return base.GetStat(stat);
        }

        protected override void DoSetStat(UnitStat stat, int value)
        {
            if (stat == AlageneValueStat) AlageneValue = value;
            else if (stat == AladdiumValueStat) AladdiumValue = value;
            else base.DoSetStat(stat, value);
        }
        #endregion
    }
}
