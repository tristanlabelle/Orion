using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Game.Simulation.Components.Serialization;
using System.Diagnostics;
using Orion.Engine;

namespace Orion.Game.Simulation.Components
{
    /// <summary>
    /// Marks an <see cref="Entity"/> as being under construction
    /// and tracks the progress of it. When completed, adds
    /// a <see cref="TaskQueue"/> component to the <see cref="Entity"/>,
    /// allowing it to be assigned tasks.
    /// </summary>
    public sealed class BuildProgress : Component
    {
        #region Fields
        public static readonly Stat RequiredTimeStat = new Stat(typeof(BuildProgress), StatType.Real, "RequiredTime");

        private float timeSpent;
        private float requiredTime = 1;
        #endregion

        #region Constructors
        public BuildProgress(Entity entity)
            : base(entity)
        { }
        #endregion

        #region Properties
        public float TimeSpent
        {
            get { return timeSpent; }
        }

        [Mandatory]
        public float RequiredTime
        {
            get { return requiredTime; }
            set
            {
                Argument.EnsurePositive(value, "RequiredTime");
                requiredTime = value;
            }
        }

        /// <summary>
        /// Gets the progress of the build within range [0,1].
        /// </summary>
        public float Progress
        {
            get { return Math.Min(1, timeSpent / (float)Entity.GetStatValue(RequiredTimeStat)); }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds some time to the build time accumulator.
        /// </summary>
        /// <param name="time">The time to be added.</param>
        public void SpendTime(float time)
        {
            Argument.EnsurePositive(time, "time");
            if (time == 0) return;

            timeSpent += time;

            Identity identity = Entity.Identity;
            Health health = Entity.Components.TryGet<Health>();

            if (health != null)
            {
                health.Damage -= time / requiredTime * (float)Entity.GetStatValue(Health.MaximumValueStat);
            }
        }

        public override void Update(SimulationStep step)
        {
            if (Entity.Components.Has<TaskQueue>())
            {
                Debug.Fail("{0} has a task queue while being built.".FormatInvariant(Entity));
            }

            if (timeSpent >= (float)Entity.GetStatValue(RequiredTimeStat))
                Complete();
        }

        /// <summary>
        /// Completes the construction of this <see cref="Entity"/>.
        /// </summary>
        public void Complete()
        {
            Health health = Entity.Components.TryGet<Health>();
            if (health != null) health.Damage = 0;

            Entity.Components.Add(new TaskQueue(Entity));
            Entity.Components.Remove(this);
            ((Unit)Entity).OnConstructionCompleted();
        }
        #endregion
    }
}
