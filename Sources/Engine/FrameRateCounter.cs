using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Orion.Engine
{
    /// <summary>
    /// Keeps track of the frame rate.
    /// </summary>
    public sealed class FrameRateCounter
    {
        #region Fields
        private Stopwatch stopwatch;
        private float timeAccumulator;
        private int frameAccumulator;
        private float peakTimeDelta;
        private float averageSecondsPerFrame;
        private float peakSecondsPerFrame;
        #endregion

        #region Properties
        public float AverageSecondsPerFrame
        {
            get { return averageSecondsPerFrame; }
        }

        public float AverageMillisecondsPerFrame
        {
            get { return averageSecondsPerFrame * 1000; }
        }

        public float AverageFramesPerSecond
        {
            get { return SpfToFps(averageSecondsPerFrame); }
        }

        public float PeakSecondsPerFrame
        {
            get { return peakSecondsPerFrame; }
        }

        public float PeakMillisecondsPerFrame
        {
            get { return peakSecondsPerFrame * 1000; }
        }

        public float PeakFramesPerSecond
        {
            get { return SpfToFps(peakSecondsPerFrame); }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Updates this <see cref="FrameRateCounter"/> for a frame.
        /// </summary>
        public void Update()
        {
            if (stopwatch == null)
            {
                stopwatch = Stopwatch.StartNew();
                return;
            }

            float timeDelta = (float)stopwatch.Elapsed.TotalSeconds;
            stopwatch.Reset();
            stopwatch.Start();
            
            ++frameAccumulator;
            timeAccumulator += timeDelta;
            if (timeDelta > peakTimeDelta) peakTimeDelta = timeDelta;

            if (timeAccumulator < 1) return;
            
            averageSecondsPerFrame = timeAccumulator / frameAccumulator;
            peakSecondsPerFrame = peakTimeDelta;
            timeAccumulator = 0;
            frameAccumulator = 0;
            peakTimeDelta = 0;
        }

        private static float SpfToFps(float spf)
        {
            return (spf == 0) ? 0 : 1 / spf;
        }

        public override string ToString()
        {
            return "{0:F2} frames per second, {1} milliseconds per frame"
                .FormatInvariant(AverageFramesPerSecond, (int)(AverageSecondsPerFrame * 1000));
        }
        #endregion
    }
}
