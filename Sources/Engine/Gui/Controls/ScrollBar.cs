using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui
{
    /// <summary>
    /// A <see cref="Control"/> that can be used by the user to scroll a display area.
    /// </summary>
    public sealed class ScrollBar : Control
    {
        #region Fields
        private const int MinimumThumbSize = 16;

        private readonly RepeatButton minButton;
        private readonly RepeatButton maxButton;
        private readonly Thumb thumb;
        private Orientation orientation = Orientation.Vertical;
        private double minimum;
        private double maximum = 100;
        private double value;
        private double length = 10;
        private double smallStep = 10;
        #endregion

        #region Constructors
        public ScrollBar()
        {
            minButton = new RepeatButton();
            minButton.Clicked += OnMinButtonClicked;
            AdoptChild(minButton);

            maxButton = new RepeatButton();
            maxButton.Clicked += OnMaxButtonClicked;
            AdoptChild(maxButton);

            thumb = new Thumb();
            thumb.Dragging += OnThumbDragging;
            AdoptChild(thumb);

            MinSize = new Size(16, 16);
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when this <see cref="ScrollBar"/> gets scrolled.
        /// </summary>
        public event Action<ScrollBar> Scrolled;
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the orientation of this <see cref="ScrollBar"/>.
        /// </summary>
        public Orientation Orientation
        {
            get { return orientation; }
            set
            {
                if (value == orientation) return;

                orientation = value;
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Accesses the minimum value of the track.
        /// </summary>
        public double Minimum
        {
            get { return minimum; }
            set
            {
                Argument.EnsureFinite(value, "Minimum");

                minimum = value;
                if (maximum < minimum) maximum = minimum;
                if (this.value < minimum) Value = minimum;
            }
        }

        /// <summary>
        /// Accesses the minimum value of the track.
        /// </summary>
        public double Maximum
        {
            get { return maximum; }
            set
            {
                Argument.EnsureFinite(value, "Maximum");

                maximum = value;
                if (minimum > maximum) minimum = maximum;
                if (this.value > maximum) Value = maximum;
            }
        }

        /// <summary>
        /// Accesses the length of the active portion of the track.
        /// </summary>
        public double Length
        {
            get { return length; }
            set
            {
                Argument.EnsurePositive(value, "Maximum");
                length = value;
            }
        }

        /// <summary>
        /// Accesses the current offset in the track.
        /// </summary>
        public double Value
        {
            get { return value; }
            set
            {
                if (value > maximum - length) value = maximum - length;
                if (value < minimum) value = minimum;
                if (value == this.value) return;

                this.value = value;
                Scrolled.Raise(this);
                InvalidateArrange();
            }
        }

        /// <summary>
        /// Accesses the value of a small movement in the track,
        /// such as the movement from the end buttons.
        /// </summary>
        public double SmallStep
        {
            get { return smallStep; }
            set
            {
                Argument.EnsurePositive(value, "SmallStep");

                smallStep = value;
            }
        }
        #endregion

        #region Methods
        protected override IEnumerable<Control> GetChildren()
        {
            yield return minButton;
            yield return thumb;
            yield return maxButton;
        }

        protected override Size MeasureSize(Size availableSize)
        {
            if (orientation == Orientation.Horizontal) throw new NotImplementedException();

            return new Size(MinSize.Width, MinSize.Width * 2);
        }

        protected override void ArrangeChildren()
        {
            if (orientation == Orientation.Horizontal) throw new NotImplementedException();

            Region rectangle = base.Rectangle;

            ArrangeChild(minButton, new Region(rectangle.MinX, rectangle.MinY, rectangle.Width, rectangle.Width));
            ArrangeChild(maxButton, new Region(rectangle.MinX, rectangle.ExclusiveMaxY - rectangle.Width, rectangle.Width, rectangle.Width));

            int trackLength = rectangle.Height - rectangle.Width * 2;
            if (trackLength <= 0)
            {
                ArrangeChild(thumb, new Region(rectangle.MinX, rectangle.MinY + rectangle.Width, rectangle.Width, 0));
            }
            else
            {
                int minY = rectangle.MinY + rectangle.Width + (int)(trackLength * (value - minimum) / (maximum - minimum));
                int height = Math.Min(trackLength, (int)(trackLength * length / Math.Abs(maximum - minimum)));
                ArrangeChild(thumb, new Region(rectangle.MinX, minY, rectangle.Width, height));
            }
        }

        private void OnMinButtonClicked(RepeatButton sender, int clickCount)
        {
            Value -= SmallStep;
        }

        private void OnMaxButtonClicked(RepeatButton sender, int clickCount)
        {
            Value += SmallStep;
        }

        private void OnThumbDragging(Thumb sender, Point delta)
        {
            if (orientation == Orientation.Horizontal) throw new NotImplementedException();

            Region rectangle = base.Rectangle;
            
            int trackLength = rectangle.Height - rectangle.Width * 2;
            if (trackLength <= 0) return;

            Value += (delta.Y / (double)trackLength) * (maximum - minimum);
        }
        #endregion
    }
}
