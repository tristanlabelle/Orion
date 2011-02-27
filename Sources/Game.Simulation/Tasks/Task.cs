using System;
using Orion.Engine;
using System.Diagnostics;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Simulation.Tasks
{
    /// <summary>
    /// Abstract base class for tasks, which represent the basic building blocks of <see cref="Unit"/> behavior.
    /// </summary>
    [Serializable]
    public abstract class Task : IDisposable
    {
        #region Fields
        private readonly Entity entity;
        private bool hasEnded;
        #endregion

        #region Constructors
        protected Task(Entity entity)
        {
            Argument.EnsureNotNull(entity, "unit");
            this.entity = entity;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the <see cref="Entity"/> accomplishing this task.
        /// </summary>
        public Entity Entity
        {
            get { return entity; }
        }

        [Obsolete("Move on to Entity please")]
        public Unit Unit
        {
            get { return (Unit)entity; }
        }

        /// <summary>
        /// Gets a value indicating if this <see cref="Task"/> has terminated its execution,
        /// rendering the unit idle.
        /// </summary>
        public bool HasEnded
        {
            get { return hasEnded; }
        }

        /// <summary>
        /// Gets a string describing this <see cref="Task"/>, for logging and debugging purposes.
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        /// Gets a value indicating the progress of this <see cref="Task"/>.
        /// This value is in the interval [0, 1], or <see cref="float.NaN"/> if the progress cannot be determined.
        /// </summary>
        public virtual float Progress
        {
            get { return float.NaN; }
        }

        /// <summary>
        /// Obtains the <see cref="TaskQueue"/> of the <see cref="Entity"/> doing this task.
        /// </summary>
        protected TaskQueue TaskQueue
        {
            get
            {
                // As it is TaskQueue which updates the Tasks,
                // it should be safe to assume that the entity has a TaskQueue component,
                // hence the Get instead of TryGet.
                return entity.Components.Get<TaskQueue>();
            }
        }

        protected World World
        {
            get { return entity.World; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Updates this <see cref="Task"/> for a frame.
        /// </summary>
        /// <param name="step">Information on the simulation step.</param>
        public void Update(SimulationStep step)
        {
            if (hasEnded)
            {
                Debug.Fail("Task was updated even though it was ended.");
                return;
            }
                
            DoUpdate(step);
        }

        /// <summary>
        /// Releases all resources used by this <see cref="Task"/>.
        /// </summary>
        public virtual void Dispose() { }

        public override sealed string ToString()
        {
            return Description;
        }

        /// <summary>
        /// Marks this task as having ended.
        /// </summary>
        protected void MarkAsEnded()
        {
            Debug.Assert(!hasEnded, "Task has ended more than once.");
            hasEnded = true;
        }

        protected abstract void DoUpdate(SimulationStep step);
        #endregion
    }
}
