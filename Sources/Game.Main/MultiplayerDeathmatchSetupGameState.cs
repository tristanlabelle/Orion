using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Orion.Engine;
using Orion.Engine.Networking;
using Orion.Game.Matchmaking;
using Orion.Game.Matchmaking.Commands.Pipeline;
using Orion.Game.Matchmaking.Networking;
using Orion.Game.Matchmaking.Networking.Packets;
using Orion.Game.Presentation;
using Orion.Game.Presentation.Gui;
using Orion.Game.Simulation;

namespace Orion.Game.Main
{
    /// <summar>y
    /// A game state in which the user can tweak settings before starting a multiplayer game.
    /// </summary>
    public sealed class MultiplayerDeathmatchSetupGameState : GameState
    {
        #region Instance
        #region Fields
        private readonly GameGraphics graphics;
        private readonly MatchSettings matchSettings;
        private readonly PlayerSettings playerSettings;
        private readonly MatchConfigurationUI ui;
        private readonly GameNetworking networking;
        private readonly IMatchAdvertizer advertizer;

        /// <remarks>Defined only for clients.</remarks>
        private readonly IPv4EndPoint? hostEndPoint;

        /// <remarks>Defined only for the host.</remarks>
        private readonly string matchName;

        private readonly string playerName;

        private TimeSpan elapsedTimeSinceLastAdvertize;
        #endregion

        #region Constructors
        private MultiplayerDeathmatchSetupGameState(GameStateManager manager, GameGraphics graphics,
            GameNetworking networking, IMatchAdvertizer advertizer, string matchName, string playerName, IPv4EndPoint? hostEndPoint)
            : base(manager)
        {
            Argument.EnsureNotNull(manager, "manager");
            Argument.EnsureNotNull(graphics, "graphics");
            Argument.EnsureNotNull(networking, "networking");
            Argument.EnsureNotNull(advertizer, "advertizer");
            Argument.EnsureNotNull(playerName, "playerName");

            this.networking = networking;
            this.graphics = graphics;
            this.advertizer = advertizer;
            this.matchSettings = new MatchSettings();
            this.hostEndPoint = hostEndPoint;
            this.matchName = matchName;
            this.playerName = playerName;

            this.ui = new MatchConfigurationUI(graphics.GuiStyle)
            {
                CanChangeSettings = IsHost,
                CanStart = IsHost,
                NeedsReadying = false
            };
            this.ui.Exited += sender => Exit();

            this.ui.AddBooleanSetting("Codes de triche", () => matchSettings.AreCheatsEnabled);
            this.ui.AddBooleanSetting("Début nomade", () => matchSettings.StartNomad);
            this.ui.AddBooleanSetting("Héros aléatoires", () => matchSettings.AreRandomHeroesEnabled);
            this.ui.AddBooleanSetting("Topologie révélée", () => matchSettings.RevealTopology);

            this.playerSettings = new PlayerSettings();
            this.playerSettings.PlayerJoined += (sender, player) => this.ui.Players.Add(player);
            this.playerSettings.PlayerLeft += (sender, player, index) => this.ui.Players.Remove(player);

            if (IsHost)
            {
                this.playerSettings.AddPlayer(new LocalPlayer(playerName, playerSettings.AvailableColors.First()));

                this.playerSettings.PlayerJoined += (sender, player) => SendAddPlayer(player);
                this.playerSettings.PlayerLeft += (sender, player, index) => SendPlayerRemoved(player, index);
                this.playerSettings.PlayerChanged += (sender, player, index) => SendPlayerColorChange(player, index);

                this.matchSettings.Changed += sender => SendMatchSettings();

                this.ui.AddPlayerBuilder("Ramasseur", () =>
                {
                    if (!playerSettings.AvailableColors.Any()) return;
                    playerSettings.AddPlayer(new AIPlayer("Ramasseur", playerSettings.AvailableColors.First()));
                });
                this.ui.PlayerColorChanged += (sender, player, color) => ChangePlayerColor(player, color);
                this.ui.PlayerKicked += (sender, player) => playerSettings.RemovePlayer(player);
                this.ui.MatchStarted += sender => DelistAndStartMatch();
            }

            this.networking.PacketReceived += (sender, packet) => HandlePacket(packet);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets a value indicating if the host of the game is the local player.
        /// </summary>
        public bool IsHost
        {
            get { return !hostEndPoint.HasValue; }
        }

        private IEnumerable<IPv4EndPoint> Clients
        {
            get { return playerSettings.Players.OfType<RemotePlayer>().Select(p => p.EndPoint); }
        }
        #endregion

        #region Methods
        private void StartMatch()
        {
            Random random = new MersenneTwister(matchSettings.RandomSeed);

            Terrain terrain = Terrain.Generate(matchSettings.MapSize, random);
            World world = new World(terrain, random, matchSettings.FoodLimit);

            Match match = new Match(world, random);
            match.AreRandomHeroesEnabled = matchSettings.AreRandomHeroesEnabled;

            SlaveCommander localCommander = null;
            List<Commander> aiCommanders = new List<Commander>();
            List<FactionEndPoint> peers = new List<FactionEndPoint>();
            foreach (Player player in playerSettings.Players)
            {
                Faction faction = world.CreateFaction(Colors.GetName(player.Color), player.Color);
                faction.AladdiumAmount = matchSettings.InitialAladdiumAmount;
                faction.AlageneAmount = matchSettings.InitialAlageneAmount;
                if (matchSettings.RevealTopology) faction.LocalFogOfWar.Reveal();

                if (player is LocalPlayer)
                {
                    localCommander = new SlaveCommander(match, faction);
                }
                else if (player is RemotePlayer)
                {
                    FactionEndPoint endPoint = new FactionEndPoint(networking, faction, ((RemotePlayer)player).EndPoint);
                    peers.Add(endPoint);
                }
                else if (player is AIPlayer)
                {
                    Commander commander = new HarvestingAICommander(match, faction);
                    aiCommanders.Add(commander);
                }
                else
                {
                    throw new InvalidOperationException("Multiplayer games only support remote, local and AI players");
                }
            }
            Debug.Assert(localCommander != null, "No local player slot.");

            WorldGenerator.Generate(world, match.UnitTypes, random, !matchSettings.StartNomad);

            CommandPipeline commandPipeline = new CommandPipeline(match);
            if (matchSettings.AreCheatsEnabled) commandPipeline.PushFilter(new CheatCodeExecutor(match));
            ReplayRecorder replayRecorder = ReplayRecorder.TryCreate(matchSettings, playerSettings);
            if (replayRecorder != null) commandPipeline.PushFilter(replayRecorder);

            ICommandSink aiCommandSink = commandPipeline.TopMostSink;
            commandPipeline.PushFilter(new CommandSynchronizer(match, networking, peers));
            commandPipeline.PushFilter(new CommandOptimizer());

            aiCommanders.ForEach(commander => commandPipeline.AddCommander(commander, aiCommandSink));
            commandPipeline.AddCommander(localCommander);

            GameState targetGameState = new DeathmatchGameState(Manager, graphics,
                match, commandPipeline, localCommander);
            Manager.Push(targetGameState);
        }

        #region Overrides
        protected internal override void OnEntered()
        {
            if (IsHost) AdvertizeMatch();
            else networking.Send(MatchSettingsRequestPacket.Instance, hostEndPoint.Value);

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
            networking.Poll();
            if (IsHost)
            {
                elapsedTimeSinceLastAdvertize += TimeSpan.FromSeconds(timeDeltaInSeconds);
                if (elapsedTimeSinceLastAdvertize > advertizePeriod)
                {
                    elapsedTimeSinceLastAdvertize = TimeSpan.Zero;
                    AdvertizeMatch();
                }
            }
        }

        protected internal override void Draw(GameGraphics graphics)
        {
            graphics.DrawGui();
        }
        #endregion

        #region Events Handling
        private void SendMatchSettings()
        {
            networking.Send(new MatchSettingsPacket(matchSettings), Clients);
        }

        private void HandlePacket(GamePacketEventArgs args)
        {
            if (IsHost)
                HandleFromHostPerspective(args.Packet, args.SenderEndPoint);
            else if (args.SenderEndPoint == hostEndPoint.Value)
                HandleFromClientPerspective(args.Packet);
        }

        #region UI Events
        private void ChangePlayerColor(Player player, ColorRgb newColor)
        {
            if (IsHost)
            {
                player.Color = newColor;
                networking.Send(new ColorChangePacket(IndexOfPlayer(player), newColor), Clients);
            }
            else if (player is LocalPlayer)
            {
                networking.Send(new ColorChangeRequestPacket(newColor), hostEndPoint.Value);
            }
        }

        private void Exit()
        {
            if (IsHost)
            {
            	networking.Send(CancelMatchPacket.Instance, Clients);
                advertizer.Delist(matchName);
            }
            else
            {
            	networking.Send(new RemovePlayerPacket(), hostEndPoint.Value);
            }

            Manager.Pop();
        }

        private void DelistAndStartMatch()
        {
            networking.Broadcast(DelistMatchPacket.Instance);
            networking.Send(StartingMatchPacket.Instance, Clients);
            StartMatch();
        }
        #endregion

        #region Player Settings
        private void SendPlayerColorChange(Player player, int index)
        {
            networking.Send(new ColorChangePacket(index, player.Color), Clients);
        }

        private void SendPlayerRemoved(Player player, int index)
        {
            RemovePlayerPacket removePlayer = new RemovePlayerPacket(index);
            networking.Send(removePlayer, Clients);
            if (player is RemotePlayer)
            {
                RemotePlayer remote = (RemotePlayer)player;
                networking.Send(removePlayer, remote.EndPoint);
            }
        }

        private void SendAddPlayer(Player player)
        {
            AddPlayerPacket addPlayer = new AddPlayerPacket(player);
            if (player is RemotePlayer)
            {
                RemotePlayer remote = (RemotePlayer)player;
                foreach(IPv4EndPoint endPoint in Clients)
                    if(endPoint != remote.EndPoint)
                        networking.Send(addPlayer, endPoint);
            }
            else
                networking.Send(addPlayer, Clients);
        }
        #endregion
        #endregion

        #region Client Perspective
        private void HandleFromClientPerspective(GamePacket packet)
        {
            if (packet is MatchSettingsPacket)
            {
                MatchSettings newSettings = ((MatchSettingsPacket)packet).Settings;
                matchSettings.CopyFrom(newSettings);
                return;
            }

            if (packet is PlayerSettingsPacket)
            {
                PlayerSettingsPacket settingsPacket = (PlayerSettingsPacket)packet;
                PlayerSettings newSettings = settingsPacket.Settings;

                int indexOfSelf = settingsPacket.RecipientIndex;
                Player[] allPlayers = newSettings.Players.ToArray();
                allPlayers[0] = new RemotePlayer(hostEndPoint.Value, allPlayers[0].Name, allPlayers[0].Color);
                allPlayers[indexOfSelf] = new LocalPlayer(playerName, allPlayers[indexOfSelf].Color);

                foreach (Player player in playerSettings.Players.ToArray())
                    playerSettings.RemovePlayer(player);

                foreach (Player player in allPlayers)
                    playerSettings.AddPlayer(player);

                return;
            }
            
            if (packet is CancelMatchPacket)
            {
                // TODO: Display a proper message box
                Debug.Fail("The match was canceled");
                Manager.Pop();
            	return;
            }

            if (packet is RemovePlayerPacket)
            {
                RemovePlayerPacket remove = (RemovePlayerPacket)packet;
                Player target = playerSettings.Players.ElementAt(remove.PlayerIndex);
                if (target is LocalPlayer)
                {
                    // TODO: Display a proper message box
                    Debug.Fail("You've been disconnected");
                    Manager.Pop();
                    return;
                }
                playerSettings.RemovePlayer(target);
                return;
            }

            if (packet is AddPlayerPacket)
            {
                Player target = ((AddPlayerPacket)packet).Player;
                playerSettings.AddPlayer(target);
                return;
            }

            if (packet is ColorChangePacket)
            {
                ColorChangePacket colorPacket = (ColorChangePacket)packet;
                playerSettings.Players.ElementAt(colorPacket.Index).Color = colorPacket.Color;
                return;
            }

            if (packet is StartingMatchPacket)
            {
                StartMatch();
                return;
            }
        }
        #endregion

        #region Host Perspective
        private void HandleFromHostPerspective(GamePacket packet, IPv4EndPoint client)
        {
            if (!playerSettings.Players.OfType<RemotePlayer>().Select(p => p.EndPoint).Contains(client))
            {
                if (packet is ExploreMatchesPacket)
                {
                    AdvertizeMatch();
                }
                else if (packet is JoinRequestPacket)
                {
                    if (playerSettings.PlayerCount < playerSettings.MaximumNumberOfPlayers)
                    {
                        string playerName = ((JoinRequestPacket)packet).PlayerName;
                        RemotePlayer player = new RemotePlayer(client, playerName, playerSettings.AvailableColors.First());
                        playerSettings.AddPlayer(player);
                        networking.Send(JoinResponsePacket.Accepted, client);
                        AdvertizeMatch();
                    }
                    else networking.Send(JoinResponsePacket.Refused, client);
                }
                return;
            }

            if (packet is ColorChangeRequestPacket)
            {
                ColorChangeRequestPacket colorPacket = (ColorChangeRequestPacket)packet;
                ColorRgb color = colorPacket.Color;
                int index = IndexOfClient(client);
                Player player = playerSettings.Players.ElementAt(index);
                if (playerSettings.AvailableColors.Contains(color))
                {
                    networking.Send(new ColorChangePacket(index, color), Clients);
                    player.Color = color;
                }
                else
                    networking.Send(new ColorChangePacket(index, player.Color), client);
                return;
            }

            if (packet is MatchSettingsRequestPacket)
            {
                networking.Send(new MatchSettingsPacket(matchSettings), client);
                networking.Send(new PlayerSettingsPacket(playerSettings, IndexOfClient(client)), client);
                return;
            }

            if (packet is RemovePlayerPacket)
            {
                RemovePlayerPacket remove = (RemovePlayerPacket)packet;
                Player target = playerSettings.Players.OfType<RemotePlayer>().First(p => p.EndPoint == client);
                if (target is RemotePlayer && ((RemotePlayer)target).EndPoint == client)
                    playerSettings.RemovePlayer(target);
                return;
            }
        }

        private void AdvertizeMatch()
        {
            int openSlotsCount = playerSettings.MaximumNumberOfPlayers - playerSettings.PlayerCount;
            advertizer.Advertize(matchName, openSlotsCount);
        }

        private int IndexOfClient(IPv4EndPoint endPoint)
        {
            int index = -1;
            foreach (Player player in playerSettings.Players)
            {
                index++;
                RemotePlayer remotePlayer = player as RemotePlayer;
                if (remotePlayer == null) continue;
                if (remotePlayer.EndPoint == endPoint) break;
            }
            return index;
        }

        private int IndexOfPlayer(Player player)
        {
            int index = -1;
            foreach (Player iteratedPlayer in playerSettings.Players)
            {
                index++;
                if (player == iteratedPlayer)
                    break;
            }
            return index;
        }
        #endregion
        #endregion
        #endregion

        #region Static
        #region Fields
        /// <summary>
        /// The time to wait before re-advertizing games.
        /// </summary>
        private static readonly TimeSpan advertizePeriod = TimeSpan.FromSeconds(5);
        #endregion

        #region Methods
        public static MultiplayerDeathmatchSetupGameState CreateAsHost(
            GameStateManager manager, GameGraphics graphics,
            GameNetworking networking, string matchName, string playerName)
        {
            Argument.EnsureNotNull(matchName, "matchName");
            Argument.EnsureNotNull(playerName, "playerName");

            CompositeMatchAdvertizer advertizer = new CompositeMatchAdvertizer();
            advertizer.AddAdvertiser(new LocalNetworkAdvertizer(networking));
            advertizer.AddAdvertiser(new MasterServerAdvertizer("http://www.laissemoichercherca.com/ets/orion.php"));
            return new MultiplayerDeathmatchSetupGameState(manager, graphics,
                networking, advertizer, matchName, playerName, null);
        }

        public static MultiplayerDeathmatchSetupGameState CreateAsClient(
            GameStateManager manager, GameGraphics graphics,
            GameNetworking networking, string playerName, IPv4EndPoint hostEndPoint)
        {
            Argument.EnsureNotNull(playerName, "playerName");

            return new MultiplayerDeathmatchSetupGameState(manager, graphics,
                networking, NullMatchAdvertizer.Instance, null, playerName, hostEndPoint);
        }
        #endregion
        #endregion
    }
}
