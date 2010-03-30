using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Game.Presentation;
using Orion.Game.Matchmaking;
using Orion.Engine.Gui;

namespace Orion.Game.Main
{
    /// <summary>
    /// Handles the initialization, updating and cleanup logic of
    /// the single player menus to setup a deatchmatch.
    /// </summary>
    public sealed class SinglePlayerDeathmatchSetupGameState : GameState
    {
        #region Fields
        private readonly GameGraphics graphics;
        private readonly MatchSettings matchSettings;
        private readonly SinglePlayerMatchConfigurationUI ui;
        #endregion

        #region Constructors
        public SinglePlayerDeathmatchSetupGameState(GameStateManager manager, GameGraphics graphics)
            : base(manager)
        {
            Argument.EnsureNotNull(graphics, "graphics");

            this.graphics = graphics;
            this.matchSettings = new MatchSettings();
            this.ui = new SinglePlayerMatchConfigurationUI(matchSettings);
            this.ui.ExitPressed += OnExitPressed;
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

        private void OnExitPressed(MatchConfigurationUI sender)
        {
            Manager.Pop();
        }
        #endregion
    }
}
