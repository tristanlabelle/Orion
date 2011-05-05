using System;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Simulation.Tasks
{    
    /// <summary>
    /// A <see cref="Task"/> which make attack an enemy <see cref="Entity"/>
    /// </summary>
    [Serializable]
    public sealed class AttackTask : Task
    {
        #region Fields
        private readonly Entity target;
        private readonly FollowTask follow;
        private bool isHitting;
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
            if (attacker.Components.Has<Mobile>()) this.follow = new FollowTask(attacker, target);
        }
        #endregion

        #region Properties
        public override string Description
        {
            get { return "Attacking {0}".FormatInvariant(target); }
        }

        public override Type PublicType
        {
            get { return isHitting ? typeof(AttackTask) : typeof(MoveTask); }
        }
        #endregion

        #region Methods
        protected override void DoUpdate(SimulationStep step)
        {
            Spatial spatial = Entity.Spatial;
            Attacker attacker = Entity.Components.TryGet<Attacker>();
            Spatial targetSpatial = target.Spatial;
            if (spatial == null || attacker == null || targetSpatial == null)
            {
                MarkAsEnded();
                return;
            }

            Faction faction = FactionMembership.GetFaction(Entity);
            if (!target.IsAlive || (faction != null && !faction.CanSee(target)))
            {
                // If the target has died while we weren't yet in attack range,
                // or if the unit moved out of sight,  but we're coming, complete the motion with a move task.
                if (follow != null && !attacker.IsInRange(target) && TaskQueue.Count == 1)
                    TaskQueue.OverrideWith(new MoveTask(Entity, (Point)targetSpatial.Center));
                MarkAsEnded();
                return;
            }

            if (attacker.IsInRange(target))
            {
                isHitting = true;

                spatial.LookAt(targetSpatial.Center);
                attacker.TryHit(target);
            }
            else
            {
                isHitting = false;

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