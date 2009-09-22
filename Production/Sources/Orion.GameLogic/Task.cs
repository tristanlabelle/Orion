using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.GameLogic
{
    /// <summary>
    /// Abstract base class for tasks, which represent the basic building blocks of <see cref="Unit"/> behavior.
    /// </summary>
    [Serializable]
    public abstract class Task
    {
        #region Properties
        /// <summary>
        /// Gets a value indicating if this <see cref="Task"/> is in an idle state.
        /// </summary>
        public virtual bool IsIdle
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a human-readable string describing this <see cref="Task"/>.
        /// </summary>
        public abstract string Description { get; }
        #endregion

        #region Methods
        /// <summary>
        /// Updates this <see cref="Task"/> for a frame.
        /// </summary>
        /// <param name="timeDelta">The time elapsed since the last frame, in seconds.</param>
        public abstract void Update(float timeDelta);

        /// <summary>
        /// Aborts this <see cref="Task"/>, indicating that it will no longer be used.
        /// </summary>
        public abstract void Abort();

        public override sealed string ToString()
        {
            return Description;
        }
        #endregion
    }
}
