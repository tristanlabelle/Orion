using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion
{
    /// <summary>
    /// Provides information on a frame of the game.
    /// </summary>
    [Serializable]
    public struct GameFrameInfo
    {
        #region Fields
        private readonly int frameNumber;
        private readonly float totalTime;
        private readonly float timeDelta;
        #endregion

        #region Constructors
        public GameFrameInfo(int frameNumber, float totalTime, float timeDelta)
        {
            this.frameNumber = frameNumber;
            this.totalTime = totalTime;
            this.timeDelta = timeDelta;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the in-game frame number.
        /// </summary>
        public int FrameNumber
        {
            get { return frameNumber; }
        }

        /// <summary>
        /// Gets the total game time elapsed since the start of the game.
        /// </summary>
        public float TotalTime
        {
            get { return totalTime; }
        }

        /// <summary>
        /// Gets the game time elapsed since the last frame.
        /// </summary>
        public float TimeDelta
        {
            get { return timeDelta; }
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return "Frame #{0}, td {1}".FormatInvariant(frameNumber, timeDelta);
        }
        #endregion
    }
}
