using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui2
{
    /// <summary>
    /// A checkbox <see cref="Control"/>.
    /// </summary>
    public sealed partial class CheckBox : ContentControl
    {
        #region Fields
        private readonly Button button;
        private bool isChecked;
        private int buttonGap = 5;
        #endregion

        #region Constructors
        public CheckBox()
        {
            button = new Button();
            button.Clicked += (sender, @event) => IsChecked = !IsChecked;
            AdoptChild(button);
        }

        public CheckBox(string text)
            : this()
        {
            Argument.EnsureNotNull(text, "text");

            Content = new Label(text);
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when the state of the checkbox changes.
        /// </summary>
        public event Action<CheckBox> StateChanged;
        #endregion

        #region Properties
        /// <summary>
        /// Accesses a value indicating if this <see cref="CheckBox"/> is currently checked.
        /// </summary>
        [PropertyChangedEvent("StateChanged")]
        public bool IsChecked
        {
            get { return isChecked; }
            set
            {
                if (value == isChecked) return;

                isChecked = value;
                StateChanged.Raise(this);
            }
        }

        /// <summary>
        /// Accesses the gap between the button and the contents.
        /// </summary>
        public int ButtonGap
        {
            get { return buttonGap; }
            set
            {
                if (value == buttonGap) return;

                Argument.EnsurePositive(value, "ButtonGap");
                buttonGap = value;
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Gets the button used to tick the checkbox.
        /// </summary>
        public Button Button
        {
            get { return button; }
        }
        #endregion

        #region Methods
        protected override IEnumerable<Control> GetChildren()
        {
            yield return button;
            if (Content != null) yield return Content;
        }

        protected override Size MeasureInnerSize(Size availableSize)
        {
            Size buttonSize = button.Measure(Size.MaxValue);
            if (Content == null) return buttonSize;

            Size contentSize = Content.Measure(Size.MaxValue);
            return new Size(buttonSize.Width + buttonGap + contentSize.Width, Math.Max(buttonSize.Height, contentSize.Height));
        }

        protected override void ArrangeChildren()
        {
            Region innerRectangle = InnerRectangle;

            Size buttonSize = button.DesiredOuterSize;

            Region buttonRectangle = new Region(
                innerRectangle.MinX,
                innerRectangle.MinY + innerRectangle.Height / 2 - buttonSize.Height / 2,
                buttonSize.Width, buttonSize.Height);
            ArrangeChild(button, buttonRectangle);

            if (Content != null)
            {
                Size contentSize = Content.DesiredOuterSize;
                int contentHeight = Math.Min(contentSize.Height, innerRectangle.Height);

                Region contentRectangle = new Region(
                    innerRectangle.MinX + buttonSize.Width + buttonGap,
                    innerRectangle.MinY + innerRectangle.Height / 2 - contentHeight / 2,
                    Math.Max(0, innerRectangle.Width - buttonSize.Width - buttonGap),
                    contentHeight);
                ArrangeChild(Content, contentRectangle);
            }
        }
        #endregion
    }
}
