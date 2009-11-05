using System;

using OpenTK.Math;
using Orion.Geometry;

namespace Orion.GameLogic.Tasks
{
    /// <summary>
    /// A <see cref="Task"/>, which makes a <see cref="Unit"/> follow another <see cref="Unit"/>.
    /// </summary>
    [Serializable]
    public sealed class Follow : Task
    {
        #region Fields
        private readonly Unit follower;
        private Vector2 oldPositionTarget;
        private readonly Unit target;
        private  Move moveTask;
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
        public Follow(Unit follower, Unit target)
        {
            Argument.EnsureNotNull(follower, "follower");
            if (!follower.HasSkill<Skills.Move>())
                throw new ArgumentException("Cannot follow without the move skill.", "follower");
            Argument.EnsureNotNull(target, "target");
            if (follower == target) throw new ArgumentException("Expected the follower and followee to be different.");

            this.follower = follower;
            this.target = target;
            this.oldPositionTarget = target.Position;
            moveTask = new Move(follower, target.Position);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the <see cref="Unit"/> that executes this task.
        /// </summary>
        public Unit Follower
        {
            get { return follower; }
        }

        /// <summary>
        /// Gets the target <see cref="Unit"/> that gets followed.
        /// </summary>
        public Unit Target
        {
            get { return target; }
        }

        public bool IsInRange
        {
            get { return (target.Position - follower.Position).Length <= 1;}
        }
        /// <summary>
        /// Gets the current distance remaining between this <see cref="Unit"/>
        /// and the followed <see cref="Unit"/>.
        /// </summary>
        public float CurrentDistance
        {
            get { return (target.Position - follower.Position).Length; }
        }

        /// <summary>
        /// Gets a value indicating if the following <see cref="Unit"/>
        /// is within the target range of its <see cref="target"/>.
        /// </summary>
       
        public override bool HasEnded
        {
            get
            {
                // This task never ends as even if we get in range at one point in time,
                // the target may move again later.
                return target == null || !target.IsAlive;
            }
        }

        public override string Description
        {
            get { return "following {0}".FormatInvariant(target); }
        }
        #endregion

        #region Methods
        public override void Update(float timeDelta)
        {
            /*if (oldPositionTarget != target.Position)
            {
                this.moveTask = new Move(follower, target.Position);
                oldPositionTarget = target.Position;
            }/*/
            moveTask.Update(timeDelta);
           

        }
        #endregion
    }
}
