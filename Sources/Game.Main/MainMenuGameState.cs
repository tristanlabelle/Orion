using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Game.Presentation;
using Orion.Engine.Gui;

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
            this.ui.ViewReplaySelected += OnViewReplaySelected;
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
            RootView.PushDisplay(ui);
        }

        protected internal override void OnShadowed()
        {
            RootView.PopDisplayWithoutDisposing(ui);
        }

        protected internal override void OnUnshadowed()
        {
            OnEntered();
        }

        protected internal override void Update(float timeDelta)
        {
            RootView.Update(timeDelta);
        }

        public override void Dispose()
        {
            ui.Dispose();
        }

        private void OnSinglePlayerSelected(MainMenuUI sender)
        {
            Manager.Push(new MainMenuGameState(Manager, graphics));
        }

        private void OnMultiplayerSelected(MainMenuUI sender)
        {
            Manager.Pop();
        }

        private void OnTowerDefenseSelected(MainMenuUI sender)
        {
            throw new NotImplementedException();
        }

        private void OnViewReplaySelected(MainMenuUI sender)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
