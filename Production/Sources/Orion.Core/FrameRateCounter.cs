using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Orion
{
    /// <summary>
    /// Keeps track of the frame rate.
    /// </summary>
    public sealed class FrameRateCounter
    {
        #region Fields
        private Stopwatch stopwatch;
        private int frameCount;
        private float secondsPerFrame;
        #endregion

        #region Properties
        public float SecondsPerFrame
        {
            get { return secondsPerFrame; }
        }

        public float MillisecondsPerFrame
        {
            get { return secondsPerFrame * 1000; }
        }

        public float FramesPerSecond
        {
            get { return (secondsPerFrame == 0) ? 0 : 1 / secondsPerFrame; }
        }
        #endregion

        #region Methods
        public void Update()
        {
            if (stopwatch == null)
            {
                stopwatch = Stopwatch.StartNew();
            }
            else
            {
                ++frameCount;
                if (stopwatch.ElapsedMilliseconds >= 1000)
                {
                    secondsPerFrame = (float)stopwatch.Elapsed.TotalSeconds / frameCount;
                    stopwatch.Reset();
                    stopwatch.Start();
                    frameCount = 0;
                }
            }
        }

        public override string ToString()
        {
            return "{0:F2} frames per second, {1} milliseconds per frame"
                .FormatInvariant(FramesPerSecond, (int)(SecondsPerFrame * 1000));
        }
        #endregion
    }
}
