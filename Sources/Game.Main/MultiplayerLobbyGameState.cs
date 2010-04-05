using System;
using System.Linq;
using Orion.Engine;
using Orion.Engine.Gui;
using Orion.Engine.Networking;
using Orion.Game.Presentation;
using System.Net.Sockets;

namespace Orion.Game.Main
{
    /// <summary>
    /// Handles the initialization, updating and cleanup logic of
    /// the single player menus to setup a deatchmatch.
    /// </summary>
    public sealed class MultiplayerLobbyGameState : GameState
    {
        #region Fields
        private static readonly int defaultPort = 41223;

        private readonly GameGraphics graphics;
        private readonly SafeTransporter transporter;
        private readonly MultiplayerLobbyUI ui;
        #endregion

        #region Constructors
        public MultiplayerLobbyGameState(GameStateManager manager, GameGraphics graphics)
            : base(manager)
        {
            Argument.EnsureNotNull(graphics, "graphics");

            this.graphics = graphics;
            try { this.transporter = new SafeTransporter(defaultPort); }
            catch (SocketException) { this.transporter = new SafeTransporter(); }
            this.ui = new MultiplayerLobbyUI(transporter);
            this.ui.BackPressed += OnBackPressed;
            this.ui.HostPressed += OnHostPressed;
            this.ui.JoinPressed += OnJoinPressed;
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
            transporter.Dispose();
        }

        private void OnBackPressed(MultiplayerLobbyUI sender)
        {
            Manager.Pop();
        }

        private void OnHostPressed(MultiplayerLobbyUI sender)
        {
            Manager.Push(new MultiplayerDeathmatchSetupGameState(Manager, graphics, transporter, null));
        }

        private void OnJoinPressed(MultiplayerLobbyUI sender, IPv4EndPoint targetEndPoint)
        {
            Manager.Push(new MultiplayerDeathmatchSetupGameState(Manager, graphics, transporter, targetEndPoint));
        }
        #endregion
    }
}
