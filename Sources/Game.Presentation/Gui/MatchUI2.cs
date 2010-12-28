using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Gui2;
using Orion.Game.Presentation.Renderers;
using Orion.Engine;
using Orion.Engine.Gui2.Adornments;

namespace Orion.Game.Presentation.Gui
{
    /// <summary>
    /// Provides the in-match user interface.
    /// </summary>
    public sealed class MatchUI2 : ContentControl
    {
        #region Fields
        private readonly GameGraphics gameGraphics;
        private readonly UserInputManager userInputManager;
        #endregion

        #region Constructors
        public MatchUI2(GameGraphics gameGraphics, UserInputManager userInputManager,
            IMatchRenderer matchRenderer)
        {
            Argument.EnsureNotNull(gameGraphics, "gameGraphics");
            Argument.EnsureNotNull(userInputManager, "userInputManager");
            Argument.EnsureNotNull(matchRenderer, "matchRenderer");

            this.gameGraphics = gameGraphics;
            this.userInputManager = userInputManager;

            var style = gameGraphics.GuiStyle;
            DockPanel mainDockPanel = style.Create<DockPanel>();
            mainDockPanel.Dock(CreateTopBar(), Direction.MaxY);

            Content = mainDockPanel;
        }
        #endregion

        #region Properties
        #endregion

        #region Methods
        private Control CreateTopBar()
        {
            var style = gameGraphics.GuiStyle;

            ContentControl topBar = new ContentControl();
            topBar.HorizontalAlignment = Alignment.Center;
            topBar.MinWidth = 500;
            NinePartTextureAdornment adornment = new NinePartTextureAdornment(style.TryGetTexture("Header"));
            topBar.Adornment = adornment;
            topBar.Padding = new Borders(adornment.Texture.Width / 2 - 10, 8);

            DockPanel dockPanel = style.Create<DockPanel>();
            topBar.Content = dockPanel;

            ImageBox aladdiumImageBox = style.Create<ImageBox>();
            dockPanel.Dock(aladdiumImageBox, Direction.MinX);
            aladdiumImageBox.Texture = gameGraphics.GetMiscTexture("Aladdium");
            aladdiumImageBox.VerticalAlignment = Alignment.Center;
            aladdiumImageBox.Width = 30;
            aladdiumImageBox.Height = 30;

            Label aladdiumAmountLabel = style.CreateLabel("0");
            dockPanel.Dock(aladdiumAmountLabel, Direction.MinX);
            aladdiumAmountLabel.VerticalAlignment = Alignment.Center;
            aladdiumAmountLabel.MinXMargin = 5;
            aladdiumAmountLabel.Color = Colors.White;

            ImageBox alageneImageBox = style.Create<ImageBox>();
            dockPanel.Dock(alageneImageBox, Direction.MinX);
            alageneImageBox.Texture = gameGraphics.GetMiscTexture("Alagene");
            alageneImageBox.VerticalAlignment = Alignment.Center;
            alageneImageBox.Width = 30;
            alageneImageBox.Height = 30;
            alageneImageBox.MinXMargin = 20;

            Label alageneAmountLabel = style.CreateLabel("0");
            dockPanel.Dock(alageneAmountLabel, Direction.MinX);
            alageneAmountLabel.VerticalAlignment = Alignment.Center;
            alageneAmountLabel.MinXMargin = 5;
            alageneAmountLabel.Color = Colors.White;

            Button pauseButton = style.CreateTextButton("Pause");
            dockPanel.Dock(pauseButton, Direction.MaxX);
            pauseButton.VerticalAlignment = Alignment.Center;

            Button diplomacyButton = style.CreateTextButton("Diplomatie");
            dockPanel.Dock(diplomacyButton, Direction.MaxX);
            diplomacyButton.VerticalAlignment = Alignment.Center;
            diplomacyButton.MaxXMargin = 10;

            return topBar;
        }
        #endregion
    }
}
