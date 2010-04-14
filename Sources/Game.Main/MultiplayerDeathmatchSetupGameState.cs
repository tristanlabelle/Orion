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
using System.Diagnostics;
using Orion.Engine.Gui;
using Orion.Game.Matchmaking.Commands.Pipeline;

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
            this.hostEndPoint = hostEndPoint;

            this.playerSettings = new PlayerSettings();
            this.playerSettings.AddPlayer(new LocalPlayer(playerSettings.AvailableColors.First()));

            List<PlayerBuilder> builders = new List<PlayerBuilder>();
            builders.Add(new PlayerBuilder("Noop Computer", (name, color) => new AIPlayer(name, color)));

            this.ui = new MatchConfigurationUI(matchSettings, playerSettings, builders, IsHost);

            if (IsHost)
            {
                this.playerSettings.PlayerJoined += OnPlayerAdded;
                this.playerSettings.PlayerLeft += OnPlayerRemoved;
                this.playerSettings.PlayerChanged += OnColorChanged;
                this.ui.AddPlayerPressed += (sender, player) => playerSettings.AddPlayer(player);
                this.ui.KickPlayerPressed += (sender, player) => playerSettings.RemovePlayer(player);
                this.ui.StartGamePressed += OnStartGamePressed;
            }
            this.ui.PlayerColorChanged += OnColorChanged;
            this.ui.ExitPressed += OnExitPressed;

            this.networking.PacketReceived += OnPacketReceived;
        }
        #endregion

        #region Properties
        public bool IsHost
        {
            get { return !hostEndPoint.HasValue; }
        }

        public RootView RootView
        {
            get { return graphics.RootView; }
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
                    Commander commander = new AICommander(match, faction);
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
            if (matchSettings.AreRandomHeroesEnabled) commandPipeline.PushFilter(new RandomHeroTrainer(match));
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
            networking.Poll();
        }

        protected internal override void Draw(GameGraphics graphics)
        {
            RootView.Draw(graphics.Context);
        }

        public override void Dispose()
        {
            ui.Dispose();
        }
        #endregion

        #region Events Handling
        private void OnPacketReceived(GameNetworking networking, GamePacketEventArgs args)
        {
            if (IsHost)
                HandleFromHostPerspective(args.Packet, args.SenderEndPoint);
            else if (args.SenderEndPoint == hostEndPoint.Value)
                HandleFromClientPerspective(args.Packet);
        }

        #region UI Events
        private void OnColorChanged(MatchConfigurationUI ui, Player player, ColorRgb newColor)
        {
            if (IsHost)
                networking.Send(new ColorChangePacket(IndexOfPlayer(player), newColor), Clients);
            else if (player is LocalPlayer)
                networking.Send(new ColorChangeRequestPacket(newColor), hostEndPoint.Value);
        }

        private void OnExitPressed(MatchConfigurationUI ui)
        {
            Manager.Pop();

            if (IsHost)
            {
                foreach (RemotePlayer remotePlayer in playerSettings.Players.OfType<RemotePlayer>())
                    networking.Send(new RemovePlayerPacket(IndexOfPlayer(remotePlayer)), remotePlayer.EndPoint);
                networking.Broadcast(new RemovePlayerPacket());
            }
            else
                networking.Send(new RemovePlayerPacket(), hostEndPoint.Value);
        }

        private void OnStartGamePressed(MatchConfigurationUI ui)
        {
            networking.Send(new StartingMatchPacket(), Clients);
            StartMatch();
        }
        #endregion

        #region Player Settings
        private void OnColorChanged(PlayerSettings settings, Player changingPlayer, int index)
        {
            networking.Send(new ColorChangePacket(index, changingPlayer.Color), Clients);
        }

        private void OnPlayerRemoved(PlayerSettings settings, Player leavingPlayer, int index)
        {
            RemovePlayerPacket removePlayer = new RemovePlayerPacket(index);
            networking.Send(removePlayer, Clients);
            if (leavingPlayer is RemotePlayer)
            {
                RemotePlayer remote = (RemotePlayer)leavingPlayer;
                networking.Send(removePlayer, remote.EndPoint);
            }
        }

        private void OnPlayerAdded(PlayerSettings sender, Player newPlayer)
        {
            AddPlayerPacket addPlayer = new AddPlayerPacket(newPlayer);
            if (newPlayer is RemotePlayer)
            {
                RemotePlayer remote = (RemotePlayer)newPlayer;
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
                matchSettings.AreCheatsEnabled = newSettings.AreCheatsEnabled;
                matchSettings.AreRandomHeroesEnabled = newSettings.AreRandomHeroesEnabled;
                matchSettings.FoodLimit = newSettings.FoodLimit;
                matchSettings.InitialAladdiumAmount = newSettings.InitialAladdiumAmount;
                matchSettings.InitialAlageneAmount = newSettings.InitialAlageneAmount;
                matchSettings.MapSize = newSettings.MapSize;
                matchSettings.RandomSeed = newSettings.RandomSeed;
                matchSettings.RevealTopology = newSettings.RevealTopology;
                matchSettings.StartNomad = newSettings.StartNomad;
                return;
            }

            if (packet is PlayerSettingsPacket)
            {
                PlayerSettingsPacket settingsPacket = (PlayerSettingsPacket)packet;
                PlayerSettings newSettings = settingsPacket.Settings;

                int indexOfSelf = settingsPacket.RecipientIndex;
                Player[] allPlayers = newSettings.Players.ToArray();
                allPlayers[0] = new RemotePlayer(hostEndPoint.Value, allPlayers[0].Color);
                allPlayers[indexOfSelf] = new LocalPlayer(allPlayers[indexOfSelf].Color);

                foreach (Player player in allPlayers)
                    playerSettings.RemovePlayer(player);

                foreach (Player player in newSettings.Players)
                    playerSettings.AddPlayer(player);

                return;
            }

            if (packet is RemovePlayerPacket)
            {
                RemovePlayerPacket remove = (RemovePlayerPacket)packet;
                Player target = playerSettings.Players.ElementAt(remove.PlayerIndex);
                if (target is LocalPlayer)
                {
                    Instant.DisplayAlert(ui, "Vous avez été déconnecté.", () => Manager.Pop());
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
                    AdvertizeMatchPacket answer = new AdvertizeMatchPacket(matchName, playerSettings.MaximumNumberOfPlayers - playerSettings.PlayersCount);
                    networking.Send(answer, client);
                }
                else if (packet is JoinRequestPacket)
                {
                    if (playerSettings.PlayersCount < playerSettings.MaximumNumberOfPlayers)
                    {
                        RemotePlayer player = new RemotePlayer(client, playerSettings.AvailableColors.First());
                        playerSettings.AddPlayer(player);
                        networking.Send(new JoinResponsePacket(true), client);
                    }
                    else networking.Send(new JoinResponsePacket(false), client);
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
    }
}
