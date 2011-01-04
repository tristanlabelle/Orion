using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Gui2;
using Orion.Engine.Graphics;

namespace Orion.Game.Presentation.Gui
{
    partial class SingleEntitySelectionPanel
    {
        private sealed class TodoButton : Button
        {
            #region Fields
            private readonly SingleEntitySelectionPanel panel;
            private readonly ImageBox imageBox;
            #endregion

            #region Constructors
            public TodoButton(SingleEntitySelectionPanel panel)
            {
                this.panel = panel;
                panel.Style.ApplyStyle(this);
                SetSize(32, 32);

                Content = imageBox = new ImageBox();
            }
            #endregion

            #region Properties
            public Texture Texture
            {
                get { return imageBox.Texture; }
                set { imageBox.Texture = value; }
            }
            #endregion
        }
    }
}
