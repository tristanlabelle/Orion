using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Gui;
using Orion.Engine.Gui.Adornments;
using Orion.Game.Presentation;
using Orion.Game.Presentation.Gui;

namespace Orion.Game.Main
{
    /// <summary>
    /// Handles the updating logic of the game when in the main menu.
    /// </summary>
    public sealed class MainMenuGameState : GameState
    {
        #region Fields
        private readonly MainMenuUI ui;
        #endregion

        #region Constructors
        public MainMenuGameState(GameStateManager manager)
            : base(manager)
        {
            ui = new MainMenuUI(Graphics);

            ui.SinglePlayerClicked += sender => Manager.Push(new SinglePlayerDeathmatchSetupGameState(Manager));
            ui.MultiplayerClicked += sender => Manager.Push(new MultiplayerLobbyGameState(Manager));
            ui.ReplayClicked += sender => Manager.Push(new ReplayBrowserGameState(Manager));
            ui.QuitClicked += sender => Manager.Pop();
        }
        #endregion

        #region Methods
        protected internal override void OnEntered()
        {
            Graphics.UIManager.Content = ui;
        }

        protected internal override void OnShadowed()
        {
            Graphics.UIManager.Content = null;
        }

        protected internal override void OnUnshadowed()
        {
            OnEntered();
        }

        protected internal override void Update(float timeDeltaInSeconds)
        {
            Graphics.UpdateGui(timeDeltaInSeconds);
        }

        protected internal override void Draw(GameGraphics graphics)
        {
            graphics.DrawGui();
        }
        #endregion
    }
}
