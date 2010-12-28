using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Gui2;
using Orion.Engine.Gui2.Adornments;
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
        private readonly GameGraphics graphics;
        private readonly MainMenuUI ui;
        #endregion

        #region Constructors
        public MainMenuGameState(GameStateManager manager, GameGraphics graphics)
            : base(manager)
        {
            Argument.EnsureNotNull(graphics, "graphics");

            this.graphics = graphics;

            ui = new MainMenuUI(graphics.GuiStyle);

            ui.SinglePlayerClicked += sender => Manager.Push(new SinglePlayerDeathmatchSetupGameState(Manager, graphics));
            ui.MultiplayerClicked += sender => Manager.Push(new MultiplayerLobbyGameState(Manager, graphics));
            ui.TowerDefenseClicked += sender => Manager.Push(new TowerDefenseGameState(Manager, graphics));
            ui.TypingDefenseClicked += sender => Manager.Push(new TypingDefenseGameState(Manager, graphics));
            ui.ReplayClicked += sender => Manager.Push(new ReplayBrowserGameState(Manager, graphics));
            ui.QuitClicked += sender => Manager.Pop();
        }
        #endregion

        #region Methods
        protected internal override void OnEntered()
        {
            graphics.UIManager.Content = ui;
        }

        protected internal override void OnShadowed()
        {
            graphics.UIManager.Content = null;
        }

        protected internal override void OnUnshadowed()
        {
            OnEntered();
        }

        protected internal override void Update(float timeDeltaInSeconds)
        {
            graphics.UpdateGui(timeDeltaInSeconds);
        }

        protected internal override void Draw(GameGraphics graphics)
        {
            graphics.DrawGui();
        }
        #endregion
    }
}
