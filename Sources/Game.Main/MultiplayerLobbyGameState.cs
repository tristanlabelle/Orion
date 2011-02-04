using System;
using System.Diagnostics;
using System.Net;
using Orion.Engine;
using Orion.Engine.Networking;
using Orion.Game.Matchmaking.Networking;
using Orion.Game.Presentation;
using Orion.Game.Presentation.Gui;

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
        public MultiplayerLobbyGameState(GameStateManager manager)
            : base(manager)
        {
            this.graphics = manager.Graphics;
            this.networking = new GameNetworking();
            this.lobby = new MultiplayerLobby(this.networking);
            this.ui = new MultiplayerLobbyUI(graphics);
            this.ui.PlayerName = Dns.GetHostName();

            this.lobby.Disable();

            this.lobby.MatchesChanged += sender => RefreshMatches();
            this.lobby.JoinResponseReceived += (sender, response) => HandleJoinResponse(response);

            this.ui.Exited += sender => Manager.Pop();
            this.ui.Joined += (sender, match) => Join(match.EndPoint);
            this.ui.Hosted += (sender, name) => Host(name);
            this.ui.JoinedByIP += (sender, endPoint) => Join(endPoint);
            this.ui.PingedIP += (sender, endPoint) => Ping(endPoint);
        }
        #endregion

        #region Methods
        protected internal override void OnEntered()
        {
            OnUnshadowed();
        }

        protected internal override void OnShadowed()
        {
            lobby.Disable();
            ui.ClearMatches();
            graphics.UIManager.Content = null;
        }

        protected internal override void OnUnshadowed()
        {
            graphics.UIManager.Content = ui;
            lobby.Enable();
        }

        protected internal override void Update(float timeDeltaInSeconds)
        {
            lobby.Update();
            graphics.UpdateGui(timeDeltaInSeconds);
        }

        protected internal override void Draw(GameGraphics graphics)
        {
            graphics.DrawGui();
        }

        public override void Dispose()
        {
            OnShadowed();
            networking.Dispose();
        }

        private void RefreshMatches()
        {
            ui.ClearMatches();
            foreach (AdvertizedMatch match in lobby.Matches)
                ui.AddMatch(match);
        }

        private void HandleJoinResponse(JoinResponseEventArgs args)
        {
            if (args.WasAccepted)
            {
                var gameState = MultiplayerDeathmatchSetupGameState.CreateAsClient(
                    Manager, networking, ui.PlayerName, args.HostEndPoint);
                Manager.Push(gameState);
            }
            else
            {
                Debug.Fail("Failed to join " + args.HostEndPoint);
                ui.HasEnabledFlag = true;
            }
        }

        private void Host(string matchName)
        {
            var gameState = MultiplayerDeathmatchSetupGameState.CreateAsHost(
                Manager, networking, matchName, ui.PlayerName);
            Manager.Push(gameState);
        }

        private void Ping(IPv4EndPoint endPoint)
        {
            if (endPoint.Port == 0) endPoint = new IPv4EndPoint(endPoint.Address, networking.PortNumber);
            networking.Ping(endPoint);
        }

        private void Join(IPv4EndPoint endPoint)
        {
            if (endPoint.Port == 0) endPoint = new IPv4EndPoint(endPoint.Address, networking.PortNumber);
            ui.HasEnabledFlag = false;
            lobby.BeginJoining(endPoint, ui.PlayerName);
        }
        #endregion
    }
}
