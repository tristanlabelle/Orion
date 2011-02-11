using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Graphics;
using Orion.Engine.Gui;
using Orion.Engine;

namespace Orion.Game.Presentation.Gui
{
    /// <summary>
    /// An adornment for Orion check box buttons.
    /// </summary>
    public sealed class OrionCheckBoxButtonAdornment : IAdornment
    {
        #region Fields
        private readonly Texture buttonTexture;
        private readonly Texture checkTexture;
        private readonly Texture disabledCheckTexture;
        #endregion

        #region Constructors
        public OrionCheckBoxButtonAdornment(GameGraphics graphics)
        {
            Argument.EnsureNotNull(graphics, "graphics");

            buttonTexture = graphics.GetGuiTexture("CheckBox_Button");
            checkTexture = graphics.GetGuiTexture("CheckBox_Check");
            disabledCheckTexture = graphics.GetGuiTexture("CheckBox_Check_Disabled");
        }
        #endregion

        #region Methods
        public void DrawBackground(GuiRenderer renderer, Control control)
        {
            Button button = control as Button;
            if (button == null) return;

            CheckBox checkBox = button.Parent as CheckBox;
            if (checkBox == null) return;

            var sprite = new GuiSprite(button.Rectangle, buttonTexture);
            renderer.DrawSprite(ref sprite);

            if (checkBox.IsChecked)
            {
                sprite = new GuiSprite(button.Rectangle, button.IsEnabled ? checkTexture : disabledCheckTexture);
                renderer.DrawSprite(ref sprite);
            }
        }

        public void DrawForeground(GuiRenderer renderer, Control control) { }
        #endregion
    }
}
