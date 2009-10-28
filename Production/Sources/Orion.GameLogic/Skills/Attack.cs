using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.GameLogic.Skills
{
    /// <summary>
    /// A <see cref="Skill"/> which permits a <see cref="UnitType"/> to attack.
    /// </summary>
    [Serializable]
    public sealed class Attack : Skill
    {
        #region Instance
        #region Fields
        private readonly int power;
        private readonly int minRange;
        private readonly int maxRange;
        private readonly int splashDamageRange;
        #endregion

        #region Constructors
        public Attack(int power, int minRange, int maxRange, int splashDamageRange)
        {
            Argument.EnsureStrictlyPositive(power, "power");
            Argument.EnsurePositive(minRange, "minRange");
            if (maxRange < minRange) throw new ArgumentException("The maximum range must be bigger or equal to the minimum range.", "maxRange");
            Argument.EnsurePositive(splashDamageRange, "splashDamageRange");

            this.power = power;
            this.minRange = minRange;
            this.maxRange = maxRange;
            this.splashDamageRange = splashDamageRange;
        }
        #endregion

        #region Properties
        public int Power
        {
            get { return power; }
        }

        public bool IsMelee
        {
            get { return maxRange == 0; }
        }

        public bool IsRanged
        {
            get { return maxRange > 0; }
        }

        public int MinRange
        {
            get { return minRange; }
        }

        public int MaxRange
        {
            get { return maxRange; }
        }

        public bool DoesSplashDamage
        {
            get { return splashDamageRange > 0; }
        }

        public int SplashDamageRange
        {
            get { return splashDamageRange; }
        }
        #endregion
        #endregion

        #region Static
        #region Methods
        public static Attack CreateMelee(int power)
        {
            return new Attack(power, 0, 0, 0);
        }
        #endregion
        #endregion
    }
}
