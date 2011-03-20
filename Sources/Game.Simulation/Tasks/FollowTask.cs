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
        /// <param name="unit">The <see cref="Entity"/> that follows.</param>
        /// <param name="followee">The <see cref="Entity"/> that gets followed.</param>
        /// <param name="targetDistance">
        /// The distance to reach between the <paramref name="follower"/> and the <see cref="followee"/>.
        /// </param>
        public FollowTask(Entity follower, Entity target)
            : base(follower)
        {
            if (!follower.Components.Has<Mobile>())
                throw new ArgumentException("Cannot follow without the move skill.", "follower");
            Argument.EnsureNotNull(target, "target");
            if (follower == target) throw new ArgumentException("Expected the follower and followee to be different.");

            this.target = target;
            this.moveTask = new MoveTask(follower, (Point)target.Center);
            this.oldTargetPosition = target.Position;
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

            float targetDisplacementLength = (target.Position - oldTargetPosition).LengthFast;
            float distanceToTarget = (target.Position - spatial.Position).LengthFast;
            if (targetDisplacementLength > distanceToTarget * 0.1f)
            {
                moveTask = new MoveTask(Entity, (Point)target.Center);
                oldTargetPosition = target.Position;
            }

            if (!moveTask.HasEnded) moveTask.Update(step);
        }
        #endregion
    }
}
