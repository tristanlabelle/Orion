using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Gui2;
using Orion.Engine.Graphics;
using Orion.Engine;
using System.Diagnostics;

namespace Orion.Game.Presentation.Gui
{
    /// <summary>
    /// An adornment for Orion buttons.
    /// </summary>
    public sealed class OrionButtonAdornment : IAdornment
    {
        #region Fields
        private readonly Texture upTexture;
        private readonly Texture overTexture;
        private readonly Texture downTexture;
        private readonly Texture disabledTexture;
        #endregion

        #region Constructors
        public OrionButtonAdornment(GuiRenderer renderer)
        {
            Argument.EnsureNotNull(renderer, "renderer");

            upTexture = renderer.GetTexture("Gui/Button_Up");
            overTexture = renderer.GetTexture("Gui/Button_Over");
            downTexture = renderer.GetTexture("Gui/Button_Down");
            disabledTexture = renderer.GetTexture("Gui/Button_Disabled");
        }
        #endregion

        #region Properties
        public Borders Padding
        {
            get { return new Borders(upTexture.Width / 2 - 1, upTexture.Height / 2 - 1); }
        }

        public Size MinSize
        {
            get { return new Size(upTexture.Width - 2, upTexture.Height - 2); }
        }
        #endregion

        #region Methods
        public void DrawBackground(GuiRenderer renderer, Control control)
        {
            Button button = control as Button;
            if (button == null) return;

            Texture texture;
            if (button.IsEnabled)
            {
                if (button.IsUnderMouse)
                    texture = button.IsDown ? downTexture : overTexture;
                else
                    texture = upTexture;
            }
            else
            {
                texture = disabledTexture;
            }

            renderer.DrawNinePart(control.Rectangle, texture, Colors.White);
        }

        public void DrawForeground(GuiRenderer renderer, Control control) { }
        #endregion
    }
}
