using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Graphics;
using Orion.Engine.Gui;

namespace Orion.Game.Presentation.Gui
{
    /// <summary>
    /// An adornment for Orion's <see cref="TextField"/>s.
    /// </summary>
    public sealed class OrionTextFieldAdornment : IAdornment
    {
        #region Fields
        private static readonly ColorRgb topLeftColor = ColorRgb.FromBytes(109, 122, 146);
        private static readonly ColorRgb bottomRightColor = ColorRgb.FromBytes(220, 223, 228);
        private static readonly ColorRgba fillColor = ColorRgba.FromBytes(255, 255, 255, 175);

        public static readonly OrionTextFieldAdornment Instance = new OrionTextFieldAdornment();
        #endregion

        #region Properties
        public Borders Padding
        {
            get { return new Borders(5, 3); }
        }
        #endregion

        #region Methods
        public void DrawBackground(GuiRenderer renderer, Control control)
        {
            Region rectangle = control.Rectangle;

            renderer.DrawRectangle(new Region(rectangle.MinX, rectangle.MinY, rectangle.Width, 1), bottomRightColor);
            renderer.DrawRectangle(new Region(rectangle.InclusiveMaxX, rectangle.MinY, 1, rectangle.Height), bottomRightColor);
            renderer.DrawRectangle(new Region(rectangle.MinX, rectangle.MinY + 1, 1, rectangle.Height - 1), topLeftColor);
            renderer.DrawRectangle(new Region(rectangle.MinX, rectangle.InclusiveMaxY, rectangle.Width - 1, 1), bottomRightColor);
            renderer.DrawRectangle(new Region(rectangle.MinX + 1, rectangle.MinY + 1, rectangle.Width - 2, rectangle.Height - 2), fillColor);
        }

        public void DrawForeground(GuiRenderer renderer, Control control) { }
        #endregion
    }
}
