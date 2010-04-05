using System;
using System.Linq;
using Orion.Engine;
using Orion.Engine.Gui;
using Orion.Engine.Networking;
using Orion.Game.Presentation;

namespace Orion.Game.Main
{
    /// <summary>
    /// Handles the initialization, updating and cleanup logic of
    /// the single player menus to setup a deatchmatch.
    /// </summary>
    public sealed class MultiplayerLobbyGameState : GameState
    {
        #region Fields
        private readonly GameGraphics graphics;
        private readonly SafeTransporter transporter;
        private readonly MultiplayerLobby ui;
        #endregion

        #region Constructors
        public MultiplayerLobbyGameState(GameStateManager manager, GameGraphics graphics)
            : base(manager)
        {
            Argument.EnsureNotNull(graphics, "graphics");

            this.graphics = graphics;
            this.transporter = new SafeTransporter(41223);
            this.ui = new MultiplayerLobby(transporter);
            this.ui.BackPressed += OnBackPressed;
            this.ui.HostPressed += new Action<MultiplayerLobby>(OnHostPressed);
            this.ui.JoinPressed += new Action<MultiplayerLobby, IPv4EndPoint>(OnJoinPressed);
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
            graphics.DispatchInputEvents();
            RootView.Update(timeDeltaInSeconds);
        }

        protected internal override void Draw(GameGraphics graphics)
        {
            RootView.Draw(graphics.Context);
        }

        public override void Dispose()
        {
            ui.Dispose();
            transporter.Dispose();
        }

        private void OnBackPressed(MultiplayerLobby sender)
        {
            Manager.Pop();
        }

        private void OnHostPressed(MultiplayerLobby sender)
        {
            Manager.Push(new MultiplayerDeathmatchSetupGameState(Manager, graphics, transporter, null));
        }

        private void OnJoinPressed(MultiplayerLobby sender, IPv4EndPoint targetEndPoint)
        {
            Manager.Push(new MultiplayerDeathmatchSetupGameState(Manager, graphics, transporter, targetEndPoint));
        }
        #endregion
    }
}
