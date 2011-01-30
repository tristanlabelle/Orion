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
        private readonly Texture uncheckedTexture;
        private readonly Texture checkedTexture;
        private readonly Texture disabledTexture;
        #endregion

        #region Constructors
        public OrionCheckBoxButtonAdornment(GuiRenderer renderer)
        {
            Argument.EnsureNotNull(renderer, "renderer");

            uncheckedTexture = renderer.GetTexture("Gui/CheckBox_Unchecked");
            checkedTexture = renderer.GetTexture("Gui/CheckBox_Checked");
            disabledTexture = renderer.GetTexture("Gui/CheckBox_Disabled");
        }
        #endregion

        #region Methods
        public void DrawBackground(GuiRenderer renderer, Control control)
        {
            Button button = control as Button;
            if (button == null) return;

            CheckBox checkBox = button.Parent as CheckBox;
            if (checkBox == null) return;

            Texture texture = checkBox.IsChecked ? checkedTexture : uncheckedTexture;
            var sprite = new GuiSprite(button.Rectangle, texture);
            renderer.DrawSprite(ref sprite);
        }

        public void DrawForeground(GuiRenderer renderer, Control control) { }
        #endregion
    }
}
