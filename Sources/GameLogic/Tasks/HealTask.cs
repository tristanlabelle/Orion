using System;
using System.Diagnostics;
using System.Linq;

namespace Orion.GameLogic.Tasks
{
    /// <summary>
    /// A <see cref="Task"/> which causes a <see cref="Unit"/> to repair a target to its full health
    /// or to complete its construction.
    /// </summary>
    [Serializable]
    public sealed class HealTask : Task
    {
        #region Fields
        private readonly Unit target;
        private readonly MoveTask move;     
       
        #endregion

        #region Constructors
        public HealTask(Unit healer, Unit target)
            : base(healer)
        {
            Argument.EnsureNotNull(healer, "unit");
            Argument.EnsureNotNull(target, "target");
            if (!healer.HasSkill(UnitSkill.Heal))
                throw new ArgumentException("Cannot heal without the heal skill.", "unit");
            if (target == healer)
                throw new ArgumentException("A unit cannot heal itself.");

            // TODO: check against repairability itself instead of the Building type,
            // since otherwise mechanical units could be repaired too
            if (target.Type.IsBuilding)
                throw new ArgumentException("Can only heal non buildings units.", "target");
            Debug.Assert(healer.Faction.GetDiplomaticStance(target.Faction) == DiplomaticStance.Ally);

            this.target = target;
            this.move = MoveTask.ToNearRegion(healer, target.GridRegion);
        }
        #endregion

        #region Properties

        public bool IsWithinRange
        {
            get { return (Unit.Center - target.Center).LengthFast <= Unit.GetStat(UnitStat.HealRange); }
        }
        public Unit Target
        {
            get { return target; }
        }

        public override string Description
        {
            get { return "healing {0}".FormatInvariant(target); }
        }

        public override bool HasEnded
        {
            get
            {
                if (!target.IsAlive) return true;
                if (move.HasEnded && !IsWithinRange) return true;
                return target.Health >= target.MaxHealth;
            }
        }
        #endregion

        #region Methods
        protected override void DoUpdate(SimulationStep step)
        {
            if (IsWithinRange)
            {
                Unit.LookAt(target.Center);
                int speed = Unit.GetStat(UnitStat.HealSpeed);
                target.Health += speed * step.TimeDeltaInSeconds;
            }
            else if (!move.HasEnded)
            {
                move.Update(step);
                return;
            }
            
           
        }

        #endregion
    }
}