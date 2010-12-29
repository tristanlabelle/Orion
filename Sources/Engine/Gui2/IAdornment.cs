using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui2
{
    /// <summary>
    /// Interface for classes which can visually enhance a control.
    /// </summary>
    public interface IAdornment
    {
        /// <summary>
        /// Draws the background of a given <see cref="Control"/>.
        /// This gets called before the child <see cref="Control"/>s are drawn.
        /// </summary>
        /// <param name="renderer">The <see cref="GuiRenderer"/> to be used.</param>
        /// <param name="control">The <see cref="Control"/> for which a background should be drawn.</param>
        void DrawBackground(GuiRenderer renderer, Control control);

        /// <summary>
        /// Draws the foreground of a given <see cref="Control"/>.
        /// This gets called after the child <see cref="Control"/>s are drawn.
        /// </summary>
        /// <param name="renderer">The <see cref="GuiRenderer"/> to be used.</param>
        /// <param name="control">The <see cref="Control"/> for which a foreground should be drawn.</param>
        void DrawForeground(GuiRenderer renderer, Control control);
    }
}
