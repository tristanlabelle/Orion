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
            button.Clicked += sender => IsChecked = !IsChecked;
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

        protected override Size MeasureInnerSize()
        {
            Size checkBoxSize = Manager.Renderer.GetCheckBoxSize(this);
            Size contentSize = Content == null ? Size.Zero : Content.MeasureOuterSize();
            return new Size(checkBoxSize.Width + buttonGap + contentSize.Width, Math.Max(checkBoxSize.Height, contentSize.Height));
        }

        protected override void ArrangeChildren()
        {
            Region innerRectangle;
            if (!TryGetInnerRectangle(out innerRectangle)) return;

            Size checkBoxSize = Manager.Renderer.GetCheckBoxSize(this);

            Region buttonRectangle = new Region(
                innerRectangle.MinX,
                innerRectangle.MinY + innerRectangle.Height / 2 - checkBoxSize.Height / 2,
                checkBoxSize.Width, checkBoxSize.Height);
            SetChildOuterRectangle(button, buttonRectangle);

            if (Content != null)
            {
                Size contentSize = Content.MeasureOuterSize();
                int contentHeight = Math.Min(contentSize.Height, innerRectangle.Height);

                Region contentRectangle = new Region(
                    innerRectangle.MinX + checkBoxSize.Width + buttonGap,
                    innerRectangle.MinY + innerRectangle.Height / 2 - contentHeight / 2,
                    Math.Max(0, innerRectangle.Width - checkBoxSize.Width - buttonGap),
                    contentHeight);
                SetChildOuterRectangle(Content, contentRectangle);
            }
        }
        #endregion
    }
}
