using System;
using System.ComponentModel;

namespace Orion
{
    /// <summary>
    /// Objects of this structure encapsulate the data of a frame update.
    /// </summary>
    [Serializable]
    [ImmutableObject(true)]
    public struct UpdateEventArgs
    {
        #region Fields
        private readonly float timeDeltaInSeconds;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructs a new Update event arguments structure with a given time delta.
        /// </summary>
        /// <param name="timeDelta">The time in seconds since the last Update event was triggered.</param>
        public UpdateEventArgs(float timeDeltaInSeconds)
        {
            this.timeDeltaInSeconds = timeDeltaInSeconds;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the time, in seconds, since the last Update event was triggered.
        /// </summary>
        public float TimeDeltaInSeconds
        {
            get { return timeDeltaInSeconds; }
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return "Update Event Args delta={0}".FormatInvariant(TimeDeltaInSeconds);
        }
        #endregion
    }
}
