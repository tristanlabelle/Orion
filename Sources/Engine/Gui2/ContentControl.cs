using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui2
{
    /// <summary>
    /// Base class for controls which have a single child control as content.
    /// </summary>
    public class ContentControl : Control
    {
        #region Fields
        private Control content;
        private Borders padding;
        #endregion

        #region Constructors
        public ContentControl() { }

        public ContentControl(Control content)
        {
            if (content != null)
            {
                AdoptChild(content);
                this.content = content;
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the <see cref="Control"/> which forms the content of this <see cref="ContentControl"/>.
        /// </summary>
        public Control Content
        {
            get { return content; }
            set
            {
                if (value == content) return;
                if (value.Parent != null) throw new ArgumentException("Cannot add a parented control as the content of this control.");

                if (content != null)
                {
                    Control oldContent = content;
                    content = null;
                    AbandonChild(oldContent);
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
        /// Accesses the padding between the borders of this <see cref="ContentControl"/> and its contents.
        /// </summary>
        public Borders Padding
        {
            get { return padding; }
            set
            {
                if (value == padding) return;

                padding = value;
                InvalidateMeasure();
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Attempts to retreive the inner rectangle of space reserved to this <see cref="Control"/>, this value excludes the margins and the padding.
        /// This operation can fail if this <see cref="Control"/> has no manager or if it is completely clipped.
        /// </summary>
        /// <param name="innerRectangle">
        /// If the operation succeeds, outputs the rectangle of space within the padding of this <see cref="Control"/>.
        /// </param>
        /// <returns><c>True</c> if the rectangle could be retreived, <c>false</c> if not.</returns>
        public bool TryGetInnerRectangle(out Region innerRectangle)
        {
            Region rectangle;
            if (!TryGetRectangle(out rectangle))
            {
                innerRectangle = default(Region);
                return false;
            }

            return Borders.TryShrink(rectangle, padding, out innerRectangle);
        }

        protected override IEnumerable<Control> GetChildren()
        {
            if (content != null) yield return content;
        }

        protected override Size MeasureSize()
        {
            return MeasureInnerSize() + padding;
        }

        protected virtual Size MeasureInnerSize()
        {
            return content == null ? Size.Zero : content.MeasureOuterSize();
        }

        protected override void ArrangeChildren()
        {
            if (content == null) return;

            DefaultArrangeChild(content);
        }
        #endregion
    }
}
