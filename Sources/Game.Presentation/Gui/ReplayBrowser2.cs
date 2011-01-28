using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Gui2;
using Orion.Engine;

namespace Orion.Game.Presentation.Gui
{
    public sealed class ReplayBrowser2 : ContentControl
    {
        #region Fields
        private readonly Button viewButton;
        #endregion

        #region Constructors
        public ReplayBrowser2(GameGraphics graphics)
        {
            Argument.EnsureNotNull(graphics, "graphics");

            Padding = 5;

            DockLayout dock = new DockLayout()
            {
                LastChildFill = true
            };

            DockLayout buttonDock = new DockLayout();
            
            Button backButton = graphics.GuiStyle.CreateTextButton("Retour");
            buttonDock.Dock(backButton, Direction.NegativeX);

            viewButton = graphics.GuiStyle.CreateTextButton("Visionner");
            buttonDock.Dock(viewButton, Direction.PositiveX);

            dock.Dock(buttonDock, Direction.PositiveY);

            ScrollPanel replayScrollPanel = new ScrollPanel();
            dock.Dock(replayScrollPanel, Direction.NegativeY);

            Content = dock;
        }
        #endregion

        #region Properties
        #endregion

        #region Methods
        #endregion
    }
}
