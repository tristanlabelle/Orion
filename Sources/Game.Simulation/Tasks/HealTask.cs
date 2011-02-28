using System;
using System.Diagnostics;
using System.Linq;
using Orion.Engine;
using Orion.Game.Simulation.Skills;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Simulation.Tasks
{
    /// <summary>
    /// A <see cref="Task"/> which causes a <see cref="Entity"/> to repair a target to its full health
    /// or to complete its construction.
    /// </summary>
    [Serializable]
    public sealed class HealTask : Task
    {
        #region Fields
        private readonly Entity target;
        private readonly FollowTask follow;    
        #endregion

        #region Constructors
        public HealTask(Entity healer, Entity target)
            : base(healer)
        {
            Argument.EnsureNotNull(healer, "unit");
            Argument.EnsureNotNull(target, "target");
            if (!healer.Components.Has<Healer>())
                throw new ArgumentException("Cannot heal without the heal skill.", "unit");
            if (target == healer)
                throw new ArgumentException("A unit cannot heal itself.");
            if (((Unit)target).IsBuilding)
                throw new ArgumentException("Cannot heal buildings.", "target");

            this.target = target;
            if (healer.Components.Has<Move>()) this.follow = new FollowTask(healer, target);
        }
        #endregion

        #region Properties
        public override string Description
        {
            get { return "healing {0}".FormatInvariant(target); }
        }
        #endregion

        #region Methods
        protected override void DoUpdate(SimulationStep step)
        {
            Spatial spatial = Entity.Spatial;
            Healer healer = Entity.Components.TryGet<Healer>();
            Faction faction = FactionMembership.GetFaction(Entity);
            Health targetHealth = target.Components.TryGet<Health>();
            if (spatial == null
                || healer == null
                || faction == null
                || !faction.CanSee(target)
                || targetHealth == null)
            {
                MarkAsEnded();
                return;
            }

            if (!target.IsAliveInWorld)
            {
                // If the target has died while we weren't yet in attack range,
                // but were coming, complete the motion with a move task.
                if (follow != null && !healer.IsInRange(target) && TaskQueue.Count == 1)
                    TaskQueue.OverrideWith(new MoveTask(Entity, (Point)target.Center));
                MarkAsEnded();
                return;
            }

            if (healer.IsInRange(target))
            {
                spatial.LookAt(target.Center);
                int speed = (int)Entity.GetStatValue(Healer.SpeedStat);
                targetHealth.Value += speed * step.TimeDeltaInSeconds;
                if (targetHealth.Value == (int)target.GetStatValue(Health.MaximumValueStat)) MarkAsEnded();
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