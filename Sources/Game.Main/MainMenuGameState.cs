using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Gui;
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
            this.ui = new MainMenuUI(graphics);
            this.ui.SinglePlayerSelected += OnSinglePlayerSelected;
            this.ui.MultiplayerSelected += OnMultiplayerSelected;
            this.ui.TowerDefenseSelected += OnTowerDefenseSelected;
            this.ui.TypingDefenseSelected += OnTypingDefenseSelected;
            this.ui.ViewReplaySelected += OnViewReplaySelected;
            this.ui.QuitGameSelected += OnQuitGameSelected;
        }
        #endregion

        #region Properties
        public RootView RootView
        {
            get { return graphics.RootView; }
        }
        #endregion

        #region Methods
        protected internal override void OnEntered()
        {
            RootView.Children.Add(ui);
        }

        protected internal override void OnShadowed()
        {
            RootView.Children.Remove(ui);
        }

        protected internal override void OnUnshadowed()
        {
            OnEntered();
        }

        protected internal override void Update(float timeDeltaInSeconds)
        {
            graphics.UpdateRootView(timeDeltaInSeconds);
        }

        protected internal override void Draw(GameGraphics graphics)
        {
            RootView.Draw(graphics.Context);
        }

        public override void Dispose()
        {
            ui.Dispose();
        }

        private void OnSinglePlayerSelected(MainMenuUI sender)
        {
            Manager.Push(new SinglePlayerDeathmatchSetupGameState(Manager, graphics));
        }

        private void OnMultiplayerSelected(MainMenuUI sender)
        {
            Manager.Push(new MultiplayerLobbyGameState(Manager, graphics));
        }

        private void OnTowerDefenseSelected(MainMenuUI sender)
        {
            Manager.Push(new TowerDefenseGameState(Manager, graphics));
        }

        private void OnTypingDefenseSelected(MainMenuUI sender)
        {
            Manager.Push(new TypingDefenseGameState(Manager, graphics));
        }

        private void OnViewReplaySelected(MainMenuUI sender)
        {
            Manager.Push(new ReplayBrowserGameState(Manager, graphics));
        }

        private void OnQuitGameSelected(MainMenuUI sender)
        {
            Manager.Pop();
        }
        #endregion
    }
}
