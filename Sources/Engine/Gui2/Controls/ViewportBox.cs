using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui2
{
    /// <summary>
    /// A <see cref="Control"/> which serves as a viewport for user-drawn contents.
    /// </summary>
    public sealed class ViewportBox : Control
    {
        #region Events
        /// <summary>
        /// Raised when the contents of the viewport should be rendered.
        /// </summary>
        public event Action<Control> Rendering;
        #endregion

        #region Methods
        protected override Size MeasureSize(Size availableSize) { return Size.Zero; }

        protected override void ArrangeChildren() { }

        protected internal override void Draw()
        {
            Rendering.Raise(this);
        }
        #endregion
    }
}
