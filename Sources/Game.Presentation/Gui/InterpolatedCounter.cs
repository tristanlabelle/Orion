using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;

namespace Orion.Game.Presentation.Gui
{
    /// <summary>
    /// Provides interpolations between values displayed in animated gui counters.
    /// </summary>
    public sealed class InterpolatedCounter
    {
        #region Fields
        private double minimumSpeed = 40;

        private double currentValue;
        private int targetValue;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new counter from its initial value.
        /// </summary>
        /// <param name="value">The value to be set.</param>
        public InterpolatedCounter(int value)
        {
            currentValue = value;
            targetValue = value;
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when the value displayed by the counter changes.
        /// </summary>
        public event Action<InterpolatedCounter> DisplayedValueChanged;

        /// <summary>
        /// Raised when the target value of the counter changes.
        /// </summary>
        public event Action<InterpolatedCounter> TargetValueChanged;
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the value displayed by the counter.
        /// </summary>
        public int DisplayedValue
        {
            get { return (int)Math.Round(currentValue); }
            set
            {
                if (value == DisplayedValue) return;
                currentValue = value;
                DisplayedValueChanged.Raise(this);
            }
        }

        /// <summary>
        /// Accesses the target value of the counter.
        /// </summary>
        public int TargetValue
        {
            get { return targetValue; }
            set
            {
                if (value == targetValue) return;
                targetValue = value;
                TargetValueChanged.Raise(this);
            }
        }

        /// <summary>
        /// Gets a value indicating if the displayed value matches the target value.
        /// </summary>
        public bool IsTargetReached
        {
            get { return TargetValue == DisplayedValue; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Updates the interpolation of this counter value for a frame.
        /// </summary>
        /// <param name="elapsedTime">The time elapsed since the previous frame.</param>
        public void Update(TimeSpan elapsedTime)
        {
            if (elapsedTime <= TimeSpan.Zero) return;

            int previouslyDisplayedValue = DisplayedValue;

            double delta = targetValue - currentValue;
            double speed = Math.Max(minimumSpeed, Math.Abs(delta) * 5);
            double step = speed * elapsedTime.TotalSeconds;

            if (step >= Math.Abs(delta)) currentValue = targetValue;
            else currentValue += step * Math.Sign(delta);

            if (DisplayedValue != previouslyDisplayedValue)
                DisplayedValueChanged.Raise(this);
        }
        #endregion
    }
}
