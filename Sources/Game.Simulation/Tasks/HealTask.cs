using System;
using System.Diagnostics;
using System.Linq;
using Orion.Engine;
using Orion.Game.Simulation.Skills;

namespace Orion.Game.Simulation.Tasks
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
        private readonly FollowTask follow;    
        #endregion

        #region Constructors
        public HealTask(Unit healer, Unit target)
            : base(healer)
        {
            Argument.EnsureNotNull(healer, "unit");
            Argument.EnsureNotNull(target, "target");
            if (!healer.HasSkill<HealSkill>())
                throw new ArgumentException("Cannot heal without the heal skill.", "unit");
            if (target == healer)
                throw new ArgumentException("A unit cannot heal itself.");
            if (target.Type.IsBuilding)
                throw new ArgumentException("Cannot heal buildings.", "target");

            this.target = target;
            if (healer.HasSkill<MoveSkill>()) this.follow = new FollowTask(healer, target);
        }
        #endregion

        #region Properties
        public Unit Target
        {
            get { return target; }
        }

        public override string Description
        {
            get { return "healing {0}".FormatInvariant(target); }
        }
        #endregion

        #region Methods
        protected override void DoUpdate(SimulationStep step)
        {
            if (!Unit.Faction.CanSee(target))
            {
                MarkAsEnded();
                return;
            }

            if (!target.IsAliveInWorld)
            {
                // If the target has died while we weren't yet in attack range,
                // but were coming, complete the motion with a move task.
                if (follow != null && !Unit.IsWithinHealingRange(target) && Unit.TaskQueue.Count == 1)
                    Unit.TaskQueue.OverrideWith(new MoveTask(Unit, (Point)target.Center));
                MarkAsEnded();
                return;
            }

            if (Unit.IsWithinHealingRange(target))
            {
                Unit.LookAt(target.Center);
                int speed = Unit.GetStat(HealSkill.SpeedStat);
                target.Health += speed * step.TimeDeltaInSeconds;
                if (target.Health == target.MaxHealth) MarkAsEnded();
                return;
            }
            else
            {
                if (follow == null || follow.HasEnded)
                {
                    MarkAsEnded();
                    return;
                }

                follow.Update(step);
            }
        }
        #endregion
    }
}