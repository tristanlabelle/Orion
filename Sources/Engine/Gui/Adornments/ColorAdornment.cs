using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui2.Adornments
{
    /// <summary>
    /// A control adornment which fills the background of the control with a color.
    /// </summary>
    public sealed class ColorAdornment : IAdornment
    {
        #region Fields
        private readonly ColorRgba color;
        #endregion

        #region Constructors
        public ColorAdornment(ColorRgba color)
        {
            this.color = color;
        }
        #endregion

        #region Methods
        public void DrawBackground(GuiRenderer renderer, Control control)
        {
            renderer.DrawRectangle(control.Rectangle, color);
        }

        public void DrawForeground(GuiRenderer renderer, Control control) { }
        #endregion
    }
}
