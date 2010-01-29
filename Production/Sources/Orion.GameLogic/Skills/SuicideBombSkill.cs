using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.GameLogic.Skills
{
    public sealed class SuicideBombSkill : Skill
    {
        #region Fields
        private readonly Func<UnitType, bool> targetTester;
        private readonly int explosionRadius;
        private readonly int explosionDamage;
        #endregion

        #region Constructors
        public SuicideBombSkill(Func<UnitType, bool> targetTester, int explosionRadius, int explosionDamage)
        {
            Argument.EnsureNotNull(targetTester, "targetTester");
            Argument.EnsureStrictlyPositive(explosionRadius, "explosionRadius");
            Argument.EnsureStrictlyPositive(explosionDamage, "explosionDamage");

            this.targetTester = targetTester;
            this.explosionRadius = explosionRadius;
            this.explosionDamage = explosionDamage;
        }
        #endregion

        #region Properties
        public int ExplosionRadius
        {
            get { return explosionRadius; }
        }

        public int ExplosionDamage
        {
            get { return explosionDamage; }
        }
        #endregion

        #region Methods
        public bool IsExplodingTarget(UnitType target)
        {
            Argument.EnsureNotNull(target, "target");

            return targetTester(target);
        }

        public override int? TryGetBaseStat(UnitStat stat)
        {
            return null;
        }
        #endregion
    }
}
