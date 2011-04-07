using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Orion.Engine
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
        public SimulationStep(int number, TimeSpan time, TimeSpan timeDelta)
        {
            this.number = number;
            this.timeInSeconds = (float)time.TotalSeconds;
            this.timeDeltaInSeconds = (float)timeDelta.TotalSeconds;
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

        /// <summary>
        /// Obtains the game time elapsed since the last simulation step.
        /// </summary>
        public TimeSpan TimeDelta
        {
            get { return TimeSpan.FromSeconds(timeDeltaInSeconds); }
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return "Update {0} at {1} with time delta of {2:F3} seconds"
                .FormatInvariant(number, Time, timeDeltaInSeconds);
        }
        #endregion
    }
}
