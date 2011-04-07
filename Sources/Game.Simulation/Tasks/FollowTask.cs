using System;
using OpenTK;
using Orion.Engine;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Simulation.Tasks
{
    /// <summary>
    /// A <see cref="Task"/>, which makes a <see cref="Entity"/> follow another <see cref="Entity"/>.
    /// </summary>
    [Serializable]
    public sealed class FollowTask : Task
    {
        #region Fields
        private readonly Entity target;
        private Vector2 oldTargetPosition;
        private MoveTask moveTask;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="Follow"/> task from the <see cref="Entity"/>
        /// that follows, the one that is followed and the distance when to stop.
        /// </summary>
        /// <param name="entity">The <see cref="Entity"/> that follows.</param>
        /// <param name="target">The <see cref="Entity"/> that gets followed.</param>
        public FollowTask(Entity entity, Entity target)
            : base(entity)
        {
            if (!entity.Components.Has<Mobile>())
                throw new ArgumentException("Cannot follow without the mobile component.", "entity");
            Argument.EnsureNotNull(target, "target");
            if (entity == target) throw new ArgumentException("Expected the follower and followee to be different.");

            this.target = target;

            Spatial targetSpatial = target.Spatial;
            this.moveTask = new MoveTask(entity, (Point)targetSpatial.Center);
            this.oldTargetPosition = targetSpatial.Position;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the target <see cref="Entity"/> that is being followed.
        /// </summary>
        public Entity Target
        {
            get { return target; }
        }

        public override string Description
        {
            get { return "following {0}".FormatInvariant(target); }
        }
        #endregion

        #region Methods
        protected override void DoUpdate(SimulationStep step)
        {
            Spatial spatial = Entity.Spatial;
            Spatial targetSpatial = Target.Spatial;
            Faction faction = FactionMembership.GetFaction(Entity);
            if (spatial == null
                || !target.IsAliveInWorld
                || (faction != null && !faction.CanSee(target)))
            {
                MarkAsEnded();
                return;
            }

            if (Region.AreAdjacentOrIntersecting(spatial.GridRegion, targetSpatial.GridRegion))
                return;

            float targetDisplacementLength = (targetSpatial.Position - oldTargetPosition).LengthFast;
            float distanceToTarget = (targetSpatial.Position - spatial.Position).LengthFast;
            if (targetDisplacementLength > distanceToTarget * 0.1f)
            {
                moveTask = new MoveTask(Entity, (Point)targetSpatial.Center);
                oldTargetPosition = targetSpatial.Position;
            }

            if (!moveTask.HasEnded) moveTask.Update(step);
        }
        #endregion
    }
}
