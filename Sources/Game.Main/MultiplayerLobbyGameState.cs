using System;
using System.Linq;
using System.Net.Sockets;
using Orion.Engine;
using Orion.Engine.Gui;
using Orion.Engine.Networking;
using Orion.Game.Presentation;
using Orion.Game.Presentation.Gui;
using Orion.Game.Matchmaking.Networking;
using Orion.Game.Matchmaking.Networking.Packets;
using System.Diagnostics;
using System.Net;

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
        private readonly GameNetworking networking;
        private readonly MultiplayerLobby lobby;
        private readonly MultiplayerLobbyUI ui;
        #endregion

        #region Constructors
        public MultiplayerLobbyGameState(GameStateManager manager, GameGraphics graphics)
            : base(manager)
        {
            Argument.EnsureNotNull(graphics, "graphics");

            this.graphics = graphics;
            this.networking = new GameNetworking();
            this.lobby = new MultiplayerLobby(this.networking);
            this.ui = new MultiplayerLobbyUI(Dns.GetHostName());

            this.lobby.IsEnabled = false;

            this.lobby.MatchesChanged += OnLobbyMatchesChanged;
            this.lobby.JoinResponseReceived += OnJoinResponseReceived;

            this.ui.BackPressed += OnBackPressed;
            this.ui.HostPressed += OnHostPressed;
            this.ui.JoinPressed += OnJoinPressed;
            this.ui.JoinByIpPressed += OnJoinByIPPressed;
            this.ui.PingPressed += OnPingPressed;
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
            OnUnshadowed();
        }

        protected internal override void OnShadowed()
        {
            lobby.IsEnabled = false;
            ui.ClearMatches();
            RootView.Children.Remove(ui);
        }

        protected internal override void OnUnshadowed()
        {
            RootView.Children.Add(ui);
            ui.IsEnabled = true;
            lobby.IsEnabled = true;
            lobby.Explore();
        }

        protected internal override void Update(float timeDeltaInSeconds)
        {
            lobby.Update();
            graphics.UpdateRootView(timeDeltaInSeconds);
        }

        protected internal override void Draw(GameGraphics graphics)
        {
            RootView.Draw(graphics.Context);
        }

        public override void Dispose()
        {
            OnShadowed();
            ui.Dispose();
            networking.Dispose();
        }

        private void OnLobbyMatchesChanged(MultiplayerLobby sender)
        {
            ui.ClearMatches();
            foreach (AdvertizedMatch match in lobby.Matches)
                ui.AddMatch(match);
        }

        private void OnJoinResponseReceived(MultiplayerLobby sender, JoinResponseEventArgs args)
        {
            if (args.WasAccepted)
            {
                var gameState = MultiplayerDeathmatchSetupGameState.CreateAsClient(
                    Manager, graphics, networking, args.HostEndPoint);
                Manager.Push(gameState);
            }
            else
            {
                Instant.DisplayAlert(ui, "Impossible de rejointer {0}.".FormatInvariant(args.HostEndPoint),
                    () => ui.IsEnabled = true);
            }
        }

        private void OnBackPressed(MultiplayerLobbyUI sender)
        {
            Manager.Pop();
        }

        private void OnHostPressed(MultiplayerLobbyUI sender, string matchName)
        {
            var gameState = MultiplayerDeathmatchSetupGameState.CreateAsHost(Manager, graphics, networking, matchName);
            Manager.Push(gameState);
        }

        private void OnJoinPressed(MultiplayerLobbyUI sender, AdvertizedMatch match)
        {
            Join(match.EndPoint);
        }

        private void OnJoinByIPPressed(MultiplayerLobbyUI sender, IPv4EndPoint endPoint)
        {
            if (endPoint.Port == 0) endPoint = new IPv4EndPoint(endPoint.Address, networking.PortNumber);
            Join(endPoint);
        }

        private void OnPingPressed(MultiplayerLobbyUI sender, IPv4EndPoint endPoint)
        {
            if (endPoint.Port == 0) endPoint = new IPv4EndPoint(endPoint.Address, networking.PortNumber);
            networking.Ping(endPoint);
        }

        private void Join(IPv4EndPoint endPoint)
        {
            ui.IsEnabled = false;
            lobby.BeginJoining(endPoint);
        }
        #endregion
    }
}
