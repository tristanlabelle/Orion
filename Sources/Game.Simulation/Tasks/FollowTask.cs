﻿using System;
using OpenTK.Math;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Game.Simulation.Skills;

namespace Orion.Game.Simulation.Tasks
{
    /// <summary>
    /// A <see cref="Task"/>, which makes a <see cref="Unit"/> follow another <see cref="Unit"/>.
    /// </summary>
    [Serializable]
    public sealed class FollowTask : Task
    {
        #region Fields
        private readonly Unit target;
        private Vector2 oldTargetPosition;
        private MoveTask moveTask;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="Follow"/> task from the <see cref="Unit"/>
        /// that follows, the one that is followed and the distance when to stop.
        /// </summary>
        /// <param name="unit">The <see cref="Unit"/> that follows.</param>
        /// <param name="followee">The <see cref="Unit"/> that gets followed.</param>
        /// <param name="targetDistance">
        /// The distance to reach between the <paramref name="follower"/> and the <see cref="followee"/>.
        /// </param>
        public FollowTask(Unit follower, Unit target)
            : base(follower)
        {
            if (!follower.HasSkill<MoveSkill>())
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
        /// Gets the target <see cref="Unit"/> that gets followed.
        /// </summary>
        public Unit Target
        {
            get { return target; }
        }

        /// <summary>
        /// Gets the current distance remaining between this <see cref="Unit"/>
        /// and the followed <see cref="Unit"/>.
        /// </summary>
        public float CurrentDistance
        {
            get { return Region.Distance(Unit.GridRegion, target.GridRegion); }
        }

        public override string Description
        {
            get { return "following {0}".FormatInvariant(target); }
        }
        #endregion

        #region Methods
        protected override void DoUpdate(SimulationStep step)
        {
            if (!target.IsAliveInWorld || !Unit.Faction.CanSee(target))
            {
                MarkAsEnded();
                return;
            }

            if (Region.AreAdjacentOrIntersecting(Unit.GridRegion, target.GridRegion))
                return;

            float targetDisplacementLength = (target.Position - oldTargetPosition).LengthFast;
            float distanceToTarget = (target.Position - Unit.Position).LengthFast;
            if (targetDisplacementLength > distanceToTarget * 0.1f)
            {
                moveTask = new MoveTask(Unit, (Point)target.Center);
                oldTargetPosition = target.Position;
            }

            if (!moveTask.HasEnded) moveTask.Update(step);
        }
        #endregion
    }
}
