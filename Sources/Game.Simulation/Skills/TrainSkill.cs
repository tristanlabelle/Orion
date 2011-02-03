using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Orion.Engine.Collections;
using Orion.Engine;

namespace Orion.Game.Simulation.Skills
{
    /// <summary>
    /// A <see cref="UnitSkill"/> which permits a <see cref="Unit"/> to train other units.
    /// </summary>
    [Serializable]
    public sealed class TrainSkill : UnitSkill
    {
        #region Fields
        public static readonly UnitStat SpeedStat = new UnitStat(typeof(TrainSkill), "Speed", "Vitesse d'entraînement");
        private static readonly Func<string, bool> itemValidator = item => item != null;

        private ICollection<string> targets
            = new ValidatingCollection<string>(new HashSet<string>(), itemValidator);
        private int speed = 1;
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the collection of the names of the training targets.
        /// </summary>
        public ICollection<string> Targets
        {
            get { return targets; }
        }

        /// <summary>
        /// Gets the training speed associated with this skill.
        /// </summary>
        public int Speed
        {
            get { return speed; }
            set
            {
                EnsureNotFrozen();
                speed = value;
            }
        }
        #endregion

        #region Methods
        protected override void DoFreeze()
        {
            targets = new ReadOnlyCollection<string>(targets.ToList());
        }

        protected override UnitSkill Clone()
        {
            return new TrainSkill
            {
                targets = new ValidatingCollection<string>(new HashSet<string>(targets), itemValidator),
                speed = speed
            };
        }

        public bool Supports(Unit unitType)
        {
            Argument.EnsureNotNull(unitType, "unitType");
            return !unitType.IsBuilding && targets.Contains(unitType.Name);
        }

        public override int GetStat(UnitStat stat)
        {
            if (stat == SpeedStat) return speed;
            return base.GetStat(stat);
        }

        protected override void DoSetStat(UnitStat stat, int value)
        {
            if (stat == SpeedStat) Speed = value;
            else base.DoSetStat(stat, value);
        }
        #endregion
    }
}
