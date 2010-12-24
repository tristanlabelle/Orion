using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Orion.Engine.Gui2
{
    /// <summary>
    /// Interface for classes which can draw a GUI.
    /// </summary>
    public interface IGuiRenderer
    {
        /// <summary>
        /// Measures a given string.
        /// </summary>
        /// <param name="control">The element displaying the text.</param>
        /// <param name="text">The string to be measured.</param>
        /// <returns>The size of the string, in pixels.</returns>
        Size MeasureText(Control control, string text);

        /// <summary>
        /// Gets the size of an image.
        /// </summary>
        /// <param name="control">The element displaying the image.</param>
        /// <param name="source">A value identifying an image.</param>
        /// <returns>The size of the image, in pixels.</returns>
        Size GetImageSize(Control control, object source);

        /// <summary>
        /// Gets the size of the button part of a <see cref="CheckBox"/>.
        /// </summary>
        /// <param name="checkBox">The <see cref="CheckBox"/> for which the button size should be retrieved.</param>
        /// <returns>The size of the checkbox's button, in pixels.</returns>
        Size GetCheckBoxSize(CheckBox checkBox);

        /// <summary>
        /// Begins drawing a given <see cref="Control"/>.
        /// This is called before the children are drawn.
        /// </summary>
        /// <param name="control">The <see cref="Control"/> to be drawn.</param>
        void BeginDraw(Control control);

        /// <summary>
        /// Ends drawing a given <see cref="Control"/>.
        /// This is called after the children have been drawn.
        /// </summary>
        /// <param name="control">The <see cref="Control"/> to be drawn.</param>
        void EndDraw(Control control);
    }
}
