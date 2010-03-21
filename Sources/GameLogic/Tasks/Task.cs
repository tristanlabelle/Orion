using System;
using Orion.Engine;

namespace Orion.GameLogic.Tasks
{
    /// <summary>
    /// Abstract base class for tasks, which represent the basic building blocks of <see cref="Unit"/> behavior.
    /// </summary>
    [Serializable]
    public abstract class Task : IDisposable
    {
        #region Fields
        private readonly Unit unit;
        #endregion

        #region Constructors
        protected Task(Unit unit)
        {
            Argument.EnsureNotNull(unit, "unit");
            this.unit = unit;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the <see cref="Unit"/> accomplishing this task.
        /// </summary>
        public Unit Unit
        {
            get { return unit; }
        }

        /// <summary>
        /// Gets a value indicating if this <see cref="Task"/> has terminated its execution,
        /// rendering the unit idle.
        /// </summary>
        public abstract bool HasEnded { get; }

        /// <summary>
        /// Gets a human-readable string describing this <see cref="Task"/>.
        /// </summary>
        public abstract string Description { get; }

        protected Faction Faction
        {
            get { return unit.Faction; }
        }

        protected World World
        {
            get { return unit.World; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Updates this <see cref="Task"/> for a frame.
        /// </summary>
        /// <param name="step">Information on the simulation step.</param>
        public void Update(SimulationStep step)
        {
            if (!HasEnded) DoUpdate(step);
        }

        protected abstract void DoUpdate(SimulationStep step);

        /// <summary>
        /// Releases all resources used by this <see cref="Task"/>.
        /// </summary>
        public virtual void Dispose() { }

        public override sealed string ToString()
        {
            return Description;
        }
        #endregion
    }
}
