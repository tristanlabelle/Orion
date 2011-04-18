using System;
using Orion.Engine;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Simulation.Tasks
{
    [Serializable]
    public sealed class LoadTask : Task
    {
        #region Fields
        private readonly Entity target;
        private readonly FollowTask follow;
        #endregion

        #region Constructors
        public LoadTask(Entity entity, Entity target)
            : base(entity)
        {
            Argument.EnsureNotNull(entity, "entity");
            Argument.EnsureNotNull(target, "target");
            if (!entity.Components.Has<Transporter>())
                throw new ArgumentException("Cannot transport without the transporter component.", "entity");
            if (target.Components.Has<Transporter>())
                throw new ArgumentException("An entity cannot transport another transporter.", "entity");

            this.target = target;
            if (entity.Components.Has<Mobile>()) this.follow = new FollowTask(entity, target);
        }
        #endregion

        #region Properties
        public override string Description
        {
            get { return "reaching {0} for transport".FormatInvariant(target); }
        }
        #endregion

        #region Methods
        protected override void DoUpdate(SimulationStep step)
        {
            Spatial spatial = Entity.Spatial;
            Transporter transporter = Entity.Components.TryGet<Transporter>();
            Faction faction = FactionMembership.GetFaction(Entity);
            Spatial targetSpatial = target.Components.TryGet<Spatial>();
            if (spatial == null
                || transporter == null
                || targetSpatial == null
                || faction == null
                || !target.IsAlive
                || targetSpatial == null
                || !transporter.CanEmbark(target)
                || !faction.CanSee(target))
            {
                MarkAsEnded();
                return;
            }

            if (transporter.IsInRange(target))
            {
                spatial.LookAt(targetSpatial.Center);
                transporter.Embark(target);
                MarkAsEnded();
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