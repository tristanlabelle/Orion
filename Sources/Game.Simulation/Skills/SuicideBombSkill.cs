using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Orion.Engine.Collections;
using Orion.Engine;

namespace Orion.Game.Simulation.Skills
{
    /// <summary>
    /// A skill that allows units of a <see cref="Entity"/> to explode when in contact with another unit.
    /// </summary>
    [Serializable]
    public sealed class SuicideBombSkill : UnitSkill
    {
        #region Fields
        public static readonly UnitStat RadiusStat = new UnitStat(typeof(SuicideBombSkill), "Radius");
        public static readonly UnitStat DamageStat = new UnitStat(typeof(SuicideBombSkill), "Damage");
        private static readonly Func<string, bool> itemValidator = item => item != null;

        private ICollection<string> targets
            = new ValidatingCollection<string>(new HashSet<string>(), itemValidator);
        private int radius = 1;
        private int damage;
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the collection of the names of explosion targets.
        /// </summary>
        public ICollection<string> Targets
        {
            get { return targets; }
        }

        /// <summary>
        /// Accesses the radius of the explosion, in world units.
        /// </summary>
        public int Radius
        {
            get { return radius; }
            set
            {
                EnsureNotFrozen();
                Argument.EnsureStrictlyPositive(value, "ExplosionRadius");
                radius = value;
            }
        }

        /// <summary>
        /// Accesses the damage inflicted at the center of the explosion, in health points.
        /// </summary>
        public int Damage
        {
            get { return damage; }
            set
            {
                EnsureNotFrozen();
                Argument.EnsureStrictlyPositive(value, "ExplosionDamage");
                damage = value;
            }
        }
        #endregion

        #region Methods
        protected override void DoFreeze()
        {
            targets = targets.ToList().AsReadOnly();
        }

        protected override UnitSkill Clone()
        {
            return new SuicideBombSkill
            {
                targets = new ValidatingCollection<string>(new HashSet<string>(targets), itemValidator),
                damage = damage,
                radius = radius
            };
        }

        public bool IsExplodingTarget(Unit target)
        {
            Argument.EnsureNotNull(target, "target");
            return targets.Contains(target.Name);
        }

        public override int GetStat(UnitStat stat)
        {
            if (stat == RadiusStat) return radius;
            if (stat == DamageStat) return damage;
            return base.GetStat(stat);
        }

        protected override void DoSetStat(UnitStat stat, int value)
        {
            if (stat == RadiusStat) Radius = value;
            else if (stat == DamageStat) Damage = value;
            else base.DoSetStat(stat, value);
        }
        #endregion
    }
}
