using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Orion
{
    /// <summary>
    /// Regroups informations on an update step of the game.
    /// </summary>
    [Serializable]
    [ImmutableObject(true)]
    public struct SimulationStep
    {
        #region Fields
        private readonly int number;
        private readonly float timeInSeconds;
        private readonly float timeDeltaInSeconds;
        #endregion

        #region Constructors
        public SimulationStep(int number, float timeInSeconds, float timeDeltaInSeconds)
        {
            this.number = number;
            this.timeInSeconds = timeInSeconds;
            this.timeDeltaInSeconds = timeDeltaInSeconds;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the number of this step.
        /// </summary>
        public int Number
        {
            get { return number; }
        }

        /// <summary>
        /// Gets the game time, in seconds, which has elapsed since the beginning of the game.
        /// </summary>
        public float TimeInSeconds
        {
            get { return timeInSeconds; }
        }

        /// <summary>
        /// Gets the game time which has elapsed since the beginning of the game.
        /// </summary>
        public TimeSpan Time
        {
            get { return TimeSpan.FromSeconds(timeInSeconds); }
        }

        /// <summary>
        /// Gets the game time, in seconds, which has elapsed since the last step.
        /// </summary>
        public float TimeDeltaInSeconds
        {
            get { return timeDeltaInSeconds; }
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return "Update {0} at {1:F3} with time delta of {2:F3} seconds"
                .FormatInvariant(number, Time, timeDeltaInSeconds);
        }
        #endregion
    }
}
