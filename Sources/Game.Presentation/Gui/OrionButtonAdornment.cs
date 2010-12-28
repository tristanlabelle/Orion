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
    public sealed class OrionButtonAdornment : IControlAdornment
    {
        #region Fields
        private readonly Button button;
        private readonly Texture upTexture;
        private readonly Texture overTexture;
        private readonly Texture downTexture;
        #endregion

        #region Constructors
        public OrionButtonAdornment(Button button, GuiRenderer renderer)
        {
            Argument.EnsureNotNull(button, "button");
            Argument.EnsureNotNull(renderer, "renderer");

            this.button = button;
            upTexture = renderer.TryGetTexture("Button_Up");
            overTexture = renderer.TryGetTexture("Button_Over");
            downTexture = renderer.TryGetTexture("Button_Down");
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
            Debug.Assert(control == button);

            Texture texture;
            if (button.HasDescendant(button.Manager.HoveredControl))
                texture = button.IsDown ? downTexture : overTexture;
            else
                texture = upTexture;

            renderer.FillNinePart(control.Rectangle, texture, Colors.White);
        }

        public void DrawForeground(GuiRenderer renderer, Control control)
        {
            Debug.Assert(control == button);
        }
        #endregion
    }
}
