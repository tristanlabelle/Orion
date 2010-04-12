using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Game.Presentation;
using Orion.Game.Matchmaking;
using Orion.Game.Presentation.Gui;
using Orion.Engine;
using Orion.Game.Simulation;
using Orion.Game.Matchmaking.Networking;
using Orion.Engine.Networking;
using Orion.Game.Matchmaking.Networking.Packets;

namespace Orion.Game.Main
{
    public class MultiplayerDeathmatchSetupGameState : GameState
    {
        #region Fields
        private readonly GameGraphics graphics;
        private readonly MatchSettings matchSettings;
        private readonly PlayerSettings playerSettings;
        private readonly MatchConfigurationUI ui;
        private readonly GameNetworking networking;
        private readonly IPv4EndPoint? hostEndPoint;
        private readonly string matchName = "Foo";
        #endregion

        #region Constructors
        public MultiplayerDeathmatchSetupGameState(GameStateManager manager, GameGraphics graphics, GameNetworking networking, IPv4EndPoint? hostEndPoint)
            : base(manager)
        {
            Argument.EnsureNotNull(manager, "manager");
            Argument.EnsureNotNull(graphics, "graphics");
            Argument.EnsureNotNull(networking, "networking");

            this.networking = networking;
            this.graphics = graphics;
            this.matchSettings = new MatchSettings();
            this.matchSettings.AreCheatsEnabled = true;
            this.hostEndPoint = hostEndPoint;

            this.playerSettings = new PlayerSettings();
            this.playerSettings.AddPlayer(new LocalPlayer(playerSettings.AvailableColors.First()));

            List<PlayerBuilder> builders = new List<PlayerBuilder>();
            builders.Add(new PlayerBuilder("Noop Computer", (name, color) => new AIPlayer(name, color)));

            this.ui = new MatchConfigurationUI(matchSettings, playerSettings, builders, IsHost);

            if (IsHost)
            {
                this.ui.AddPlayerPressed += (sender, player) => playerSettings.AddPlayer(player);
                this.ui.KickPlayerPressed += (sender, player) => playerSettings.RemovePlayer(player);
                this.ui.StartGamePressed += OnStartGamePressed;
            }
            this.ui.PlayerColorChanged += OnPlayerColorChanged;
            this.ui.ExitPressed += OnExitPressed;

            this.networking.PacketReceived += OnPacketReceived;
        }
        #endregion

        #region Properties
        public bool IsHost
        {
            get { return !hostEndPoint.HasValue; }
        }
        #endregion

        #region Methods
        #region Events Handling
        private void OnPacketReceived(GameNetworking networking, GamePacketEventArgs args)
        {
            if (IsHost)
                AnswerFromHostPerspective(args.Packet, args.SenderEndPoint);
        }

        private void AnswerFromHostPerspective(GamePacket packet, IPv4EndPoint client)
        {
            if (!playerSettings.Players.OfType<RemotePlayer>().Select(p => p.EndPoint).Contains(client))
            {
                if (packet is ExploreMatchesPacket)
                {
                    AdvertizeMatchPacket answer = new AdvertizeMatchPacket(matchName, playerSettings.MaximumNumberOfPlayers - playerSettings.PlayersCount);
                    networking.Send(answer, client);
                }
                else if (packet is JoinRequestPacket)
                {
                    if (playerSettings.PlayersCount < playerSettings.MaximumNumberOfPlayers)
                    {
                        networking.Send(new JoinResponsePacket(true), client);
                        playerSettings.AddPlayer(new RemotePlayer(client, playerSettings.AvailableColors.First()));
                    }
                    else networking.Send(new JoinResponsePacket(false), client);
                }

                return;
            }

            if (packet is MatchSettingsRequestPacket)
            {
                int index = -1;
                foreach (Player player in playerSettings.Players)
                {
                    index++;
                    RemotePlayer remotePlayer = player as RemotePlayer;
                    if (remotePlayer == null) continue;
                    if (remotePlayer.EndPoint == client) break;
                }
                networking.Send(new MatchSettingsPacket(playerSettings, matchSettings, index), client);
                return;
            }
        }
        #endregion
        #endregion
    }
}
