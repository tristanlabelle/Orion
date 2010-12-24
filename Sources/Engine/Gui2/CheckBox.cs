using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui2
{
    /// <summary>
    /// A checkbox <see cref="UIElement"/>.
    /// </summary>
    public sealed partial class CheckBox : UIElement
    {
        #region Fields
        private readonly ChildCollection children;
        private readonly Button button;
        private UIElement content;
        private bool isChecked;
        private int buttonGap = 5;
        #endregion

        #region Constructors
        public CheckBox()
        {
            children = new ChildCollection(this);
            button = new Button();
            button.Clicked += sender => IsChecked = !IsChecked;
            AdoptChild(button);
        }

        public CheckBox(string text)
            : this()
        {
            Argument.EnsureNotNull(text, "text");

            content = new Label(text);
            AdoptChild(content);
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
        /// Accesses the element within this <see cref="CheckBox"/>.
        /// </summary>
        public UIElement Content
        {
            get { return content; }
            set
            {
                if (value == content) return;

                if (content != null)
                {
                    AbandonChild(content);
                    content = null;
                }

                if (value != null)
                {
                    AdoptChild(value);
                    content = value;
                }

                InvalidateMeasure();
            }
        }

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
        #endregion

        #region Methods
        protected override ICollection<UIElement> GetChildren()
        {
            return children;
        }

        protected override Size MeasureWithoutMargin()
        {
            Size checkBoxSize = Manager.Renderer.GetCheckBoxSize(this);
            Size contentSize = content == null ? Size.Zero : content.Measure();
            return new Size(checkBoxSize.Width + buttonGap + contentSize.Width, Math.Max(checkBoxSize.Height, contentSize.Height));
        }

        protected override void ArrangeChildren()
        {
            Region internalRectangle;
            if (!TryGetInternalRectangle(out internalRectangle)) return;

            Size checkBoxSize = Manager.Renderer.GetCheckBoxSize(this);

            Region buttonRectangle = new Region(
                internalRectangle.MinX,
                internalRectangle.MinY + internalRectangle.Height / 2 - checkBoxSize.Height / 2,
                checkBoxSize.Width,
                checkBoxSize.Height);
            SetChildRectangle(button, buttonRectangle);

            if (content != null)
            {
                Size contentSize = content.Measure();
                int contentHeight = Math.Min(contentSize.Height, internalRectangle.Height);

                Region contentRectangle = new Region(
                    internalRectangle.MinX + checkBoxSize.Width + buttonGap,
                    internalRectangle.MinY + internalRectangle.Height / 2 - contentHeight / 2,
                    Math.Max(0, internalRectangle.Width - checkBoxSize.Width - buttonGap),
                    contentHeight);
                SetChildRectangle(content, contentRectangle);
            }
        }
        #endregion
    }
}
