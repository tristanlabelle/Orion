using System;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Game.Simulation.Skills;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Simulation.Tasks
{    
    /// <summary>
    /// A <see cref="Task"/> which make attack an enemy <see cref="Unit"/>
    /// </summary>
    [Serializable]
    public sealed class AttackTask : Task
    {
        #region Fields
        private readonly Entity target;
        private readonly FollowTask follow;
        #endregion

        #region Constructors
        public AttackTask(Entity attacker, Entity target)
            : base(attacker)
        {
            Argument.EnsureNotNull(attacker, "attacker");
            if (!attacker.Components.Has<Attacker>())
                throw new ArgumentException("Cannot attack without the attack skill.", "attacker");
            Argument.EnsureNotNull(target, "target");
            
            this.target = target;
            if (attacker.Components.Has<Move>()) this.follow = new FollowTask(attacker, (Unit)target);
        }
        #endregion

        #region Properties
        public override string Description
        {
            get { return "Attacking {0}".FormatInvariant(target); }
        }
        #endregion

        #region Methods
        protected override void DoUpdate(SimulationStep step)
        {
            Spatial spatial = Entity.Spatial;
            Attacker attacker = Entity.Components.TryGet<Attacker>();
            if (spatial == null || attacker == null)
            {
                MarkAsEnded();
                return;
            }

            Faction faction = FactionMembership.GetFaction(Entity);
            TaskQueue taskQueue = Entity.Components.Get<TaskQueue>();

            if (!target.IsAliveInWorld || (faction != null && !faction.CanSee(target)))
            {
                // If the target has died while we weren't yet in attack range,
                // or if the unit moved out of sight,  but we're coming, complete the motion with a move task.
                if (follow != null && !attacker.IsInRange(target) && taskQueue.Count == 1)
                    taskQueue.OverrideWith(new MoveTask(Entity, (Point)target.Center));
                MarkAsEnded();
                return;
            }

            if (attacker.IsInRange(target))
            {
                spatial.LookAt(target.Center);
                attacker.TryHit(target);
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