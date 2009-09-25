﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK.Math;

namespace Orion.GameLogic.Tasks
{
    /// <summary>
    /// A <see cref="Task"/>, which makes a <see cref="Unit"/> follow another <see cref="Unit"/>.
    /// </summary>
    [Serializable]
    public sealed class Follow : Task
    {
        #region Fields
        private readonly Unit unit;
        private readonly Unit target;
        private readonly float targetDistance;
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
        public Follow(Unit unit, Unit target, float targetDistance)
        {
            Argument.EnsureNotNull(unit, "unit");
            Argument.EnsureNotNull(target, "target");
            if (unit == target) throw new ArgumentException("Expected the follower and followee to be different.");
            Argument.EnsurePositive(targetDistance, "targetDistance");

            this.unit = unit;
            this.target = target;
            this.targetDistance = targetDistance;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the <see cref="Unit"/> that executes this task.
        /// </summary>
        public Unit Follower
        {
            get { return unit; }
        }

        /// <summary>
        /// Gets the target <see cref="Unit"/> that gets followed.
        /// </summary>
        public Unit Target
        {
            get { return target; }
        }

        /// <summary>
        /// Gets the distance between the <see cref="Unit"/> accomplishing this <see cref="Task"/>
        /// and the target <see cref="Unit"/> when to stop.
        /// </summary>
        public float TargetDistance
        {
            get { return targetDistance; }
        }

        public override bool HasEnded
        {
            get { return (target.Position - unit.Position).Length <= targetDistance; }
        }

        public override string Description
        {
            get { return "following {0}".FormatInvariant(target); }
        }
        #endregion

        #region Methods
        public override void Update(float timeDelta)
        {
            Vector2 delta = target.Position - unit.Position;
            float distanceRemaining = delta.Length - targetDistance;
            if (distanceRemaining < 0) return;

            Vector2 direction = Vector2.Normalize(delta);

            float movementDistance = unit.Type.MovementSpeed * timeDelta;
            if (movementDistance > distanceRemaining)
                movementDistance = distanceRemaining;

            unit.Position += direction * movementDistance;
        }
        #endregion
    }
}