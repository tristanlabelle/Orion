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
        private TimeSpan timeSpent = TimeSpan.Zero;
        private TimeSpan requiredTime = TimeSpan.FromSeconds(1);
        #endregion

        #region Constructors
        public BuildProgress(Entity entity)
            : base(entity)
        { }
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the amount of time that has been spent building this <see cref="Entity"/>.
        /// Completion is not checked when setting this property, as it could be problematic
        /// when deserializing.
        /// </summary>
        public TimeSpan TimeSpent
        {
            get { return timeSpent; }
            set
            {
                Argument.EnsurePositive(value.Ticks, "TimeSpent");
                timeSpent = value;
            }
        }

        /// <summary>
        /// Accesses the total required amount of time before this <see cref="Entity"/> is considered fully built.
        /// </summary>
        public TimeSpan RequiredTime
        {
            get { return requiredTime; }
            set
            {
                Argument.EnsureStrictlyPositive(value.Ticks, "RequiredTime");
                requiredTime = value;
            }
        }

        /// <summary>
        /// Gets a value indicating if the build has completed.
        /// </summary>
        public bool IsComplete
        {
            get { return timeSpent >= requiredTime; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds some time to the build time accumulator.
        /// </summary>
        /// <param name="time">The time to be added.</param>
        public void SpendTime(TimeSpan time)
        {
            Argument.EnsurePositive(time.Ticks, "time");
            if (time == TimeSpan.Zero) return;

            timeSpent += time;

            Health health = Entity.Components.TryGet<Health>();
            if (health != null)
            {
                float ratio = (float)(time.TotalSeconds / requiredTime.TotalSeconds);
                health.Heal(ratio * (float)Entity.GetStatValue(Health.MaxValueStat));
            }

            CheckCompletion();
        }

        protected override void Update(SimulationStep step)
        {
            if (Entity.Components.Has<TaskQueue>())
            {
                Debug.Fail("{0} has a task queue while being built.".FormatInvariant(Entity));
            }

            CheckCompletion();
        }

        /// <summary>
        /// Completes the construction of this <see cref="Entity"/>.
        /// </summary>
        public void Complete()
        {
            Health health = Entity.Components.TryGet<Health>();
            if (health != null) health.FullyHeal();

            Entity.Components.Remove(this);
            Entity.Components.Add(new TaskQueue(Entity));

            World.RaiseBuildingConstructed(Entity);
        }

        private void CheckCompletion()
        {
            if (IsComplete) Complete();
        }
        #endregion
    }
}
