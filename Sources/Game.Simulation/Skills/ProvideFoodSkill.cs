using System;
using Orion.Engine;

namespace Orion.Game.Simulation.Skills
{
    /// <summary>
    /// A <see cref="UnitSkill"/> which permits a <see cref="Unit"/> to provide food.
    /// </summary>
    [Serializable]
    public sealed class ProvideFoodSkill : UnitSkill
    {
        #region Fields
        public static readonly UnitStat AmountStat = new UnitStat(typeof(ProvideFoodSkill), "Amount", "Nourriture fournie");

        private int amount = 1;
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the amount of food that is provided, in food points.
        /// </summary>
        public int Amount
        {
            get { return amount; }
            set
            {
                EnsureNotFrozen();
                Argument.EnsureStrictlyPositive(value, "Capacity");
                amount = value;
            }
        }
        #endregion

        #region Methods
        protected override UnitSkill Clone()
        {
            return new ProvideFoodSkill { amount = amount };
        }

        public override int GetStat(UnitStat stat)
        {
            if (stat == AmountStat) return amount;
            return base.GetStat(stat);
        }

        protected override void DoSetStat(UnitStat stat, int value)
        {
            if (stat == AmountStat) Amount = value;
            else base.DoSetStat(stat, value);
        }
        #endregion
    }
}
