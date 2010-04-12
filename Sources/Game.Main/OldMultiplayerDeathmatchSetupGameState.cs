using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Engine.Gui;
using Orion.Engine.Networking;
using Orion.Game.Matchmaking;
using Orion.Game.Matchmaking.Commands.Pipeline;
using Orion.Game.Matchmaking.Networking;
using Orion.Game.Presentation;
using Orion.Game.Presentation.Gui;
using Orion.Game.Simulation;
using Orion.Game.Matchmaking.Networking.Packets;

namespace Orion.Game.Main
{
    /// <summary>
    /// Handles the initialization, updating and cleanup logic of
    /// the multiplayer menu to setup a deatchmatch.
    /// </summary>
    public sealed class MultiplayerDeathmatchSetupGameState : GameState
    {
        #region Fields
        private static readonly int maximumPlayerCount = Faction.Colors.Count() - 1;

        private readonly GameGraphics graphics;
        private readonly GameNetworking networking;
        private readonly IPv4EndPoint? hostEndPoint;
        private readonly PlayerSettings playerSettings;
        private readonly MatchSettings matchSettings;
        private readonly MatchConfigurationUI ui;
        private Action<GameNetworking, GamePacketEventArgs> packetReceivedEventHandler;
        private Action<GameNetworking, IPv4EndPoint> peerTimedOutEventHandler;
        #endregion

        #region Constructors
        public MultiplayerDeathmatchSetupGameState(GameStateManager manager, GameGraphics graphics,
            GameNetworking networking, IPv4EndPoint? hostEndPoint)
            : base(manager)
        {
            Argument.EnsureNotNull(graphics, "graphics");
            Argument.EnsureNotNull(networking, "networking");

            this.graphics = graphics;
            this.networking = networking;
            this.hostEndPoint = hostEndPoint;

            this.playerSettings = new PlayerSettings();
            this.matchSettings = new MatchSettings();
            
            List<PlayerBuilder> builders = new List<PlayerBuilder>();

            builders.Add(new PlayerBuilder("Noop Computer", (name, color) => new AIPlayer(name, color)));
            this.ui = new MatchConfigurationUI(matchSettings, playerSettings, builders);
            this.packetReceivedEventHandler = OnPacketReceived;
            this.peerTimedOutEventHandler = OnPeerTimedOut;
        }
        #endregion

        #region Properties
        public RootView RootView
        {
            get { return graphics.RootView; }
        }

        private bool IsHost
        {
            get { return !hostEndPoint.HasValue; }
        }

        private IEnumerable<IPv4EndPoint> PlayerEndPoints
        {
            get { return playerSettings.Players.OfType<RemotePlayer>().Select(p => p.EndPoint); }
        }
        #endregion

        #region Methods
        #region Overrides
        protected internal override void OnEntered()
        {
            OnUnshadowed();

            if (!IsHost)
            {
                networking.Send(MatchSettingsRequestPacket.Instance, hostEndPoint.Value);
            }
        }

        protected internal override void OnShadowed()
        {
            networking.PacketReceived -= packetReceivedEventHandler;
            networking.PeerTimedOut -= peerTimedOutEventHandler;
            RootView.Children.Remove(ui);
        }

        protected internal override void OnUnshadowed()
        {
            RootView.Children.Add(ui);
            this.networking.PacketReceived += this.packetReceivedEventHandler;
            this.networking.PeerTimedOut += this.peerTimedOutEventHandler;
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
            OnShadowed();
            ui.Dispose();
        }
        #endregion

        #region StartGame
        private void StartGame()
        {
            Random random = new MersenneTwister(matchSettings.RandomSeed);

            Terrain terrain = Terrain.Generate(matchSettings.MapSize, random);
            World world = new World(terrain, random, matchSettings.FoodLimit);

            Match match = new Match(world, random);

            SlaveCommander localCommander = null;
            List<Commander> aiCommanders = new List<Commander>();
            List<FactionEndPoint> peers = new List<FactionEndPoint>();
            int colorIndex = 0;
            foreach (Player slot in playerSettings.Players)
            {
                ColorRgb color = Faction.Colors[colorIndex];
                colorIndex++;

                string factionName = Colors.GetName(color);
                Faction faction = world.CreateFaction(factionName, color);
                faction.AladdiumAmount = matchSettings.InitialAladdiumAmount;
                faction.AlageneAmount = matchSettings.InitialAlageneAmount;
                if (matchSettings.RevealTopology) faction.LocalFogOfWar.Reveal();

                if (slot is LocalPlayer)
                {
                    localCommander = new SlaveCommander(match, faction);
                }
                else if (slot is AIPlayer)
                {
                    Commander commander = new AICommander(match, faction);
                    aiCommanders.Add(commander);
                }
                else if (slot is RemotePlayer) // no commanders for remote players
                {
                    RemotePlayer remotePlayerSlot = (RemotePlayer)slot;
                    IPv4EndPoint endPoint = remotePlayerSlot.EndPoint;
                    FactionEndPoint peer = new FactionEndPoint(networking, faction, endPoint);
                    peers.Add(peer);
                }
                else
                {
                    throw new InvalidOperationException("Multiplayer games only support remote, local and AI players");
                }
            }
            Debug.Assert(localCommander != null);

            WorldGenerator.Generate(world, match.UnitTypes, random, !matchSettings.StartNomad);

            CommandPipeline commandPipeline = new CommandPipeline(match);
            if (matchSettings.AreCheatsEnabled) commandPipeline.PushFilter(new CheatCodeExecutor(match));
            if (matchSettings.AreRandomHeroesEnabled) commandPipeline.PushFilter(new RandomHeroTrainer(match));
            ReplayRecorder replayRecorder = ReplayRecorder.TryCreate(matchSettings, world);
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
        #endregion

        #region Event Handlers
        private void OnPacketReceived(GameNetworking sender, GamePacketEventArgs args)
        {
            if (IsHost) HandlePacketAsHost(args);
            else HandlePacketAsClient(args);
        }

        private void OnPeerTimedOut(GameNetworking sender, IPv4EndPoint timedOutHostEndPoint)
        {
            if (IsHost)
            {
                if (PlayerEndPoints.Contains(timedOutHostEndPoint))
                    TryLeave(timedOutHostEndPoint);
            }
            else
            {
                if (timedOutHostEndPoint == hostEndPoint) ForceExit();
            }
        }

        private void OnStartGamePressed(MatchConfigurationUI sender)
        {
            Debug.Assert(IsHost);

            networking.Broadcast(StartingMatchPacket.Instance);
            networking.Send(StartingMatchPacket.Instance, PlayerEndPoints);
            StartGame();
        }

        private void OnExitPressed(MatchConfigurationUI sender)
        {
            if (IsHost) SendQuitMessageToClients();
            else SendQuitMessageToHost();

            Manager.Pop();
        }

        private void OnSlotOccupationChanged(MatchConfigurationUI sender,
            int slotNumber, Player newValue)
        {
            byte[] setSlotMessage = new byte[3];
            setSlotMessage[0] = (byte)SetupMessageType.SetSlot;
            setSlotMessage[1] = (byte)slotNumber;
            if (newValue is RemotePlayer) setSlotMessage[2] = (byte)PlayerSlotType.Open;
            else if (newValue is AIPlayer) setSlotMessage[2] = (byte)PlayerSlotType.AI;
            else throw new InvalidOperationException("Unknown slot type selected");

            //transporter.SendTo(setSlotMessage, ui.PlayerAddresses);
        }

        private void OnPlayerKicked(MatchConfigurationUI sender, IPv4EndPoint endPoint)
        {
            networking.Send(KickedPacket.Instance, endPoint);
        }
        #endregion

        #region Client
        private void HandlePacketAsClient(GamePacketEventArgs args)
        {
            if (args.SenderEndPoint != hostEndPoint.Value)
            {
                Debug.Fail("PacketReceived a packet from an unexpected peer.");
                return;
            }

            if (args.Packet is StartingMatchPacket)
            {
                StartGame();
            }
            else if (args.Packet is UpdateMatchSettingsPacket)
            {
                var packet = (UpdateMatchSettingsPacket)args.Packet;
                UpdateSettings(packet);
            }
            else if (args.Packet is KickedPacket)
            {
                ForceExit();
            }
            else if (args.Packet is UpdatePlayersPacket)
            {
                var packet = (UpdatePlayersPacket)args.Packet;
                SetPeer(packet);
            }
        }

        private void SendQuitMessageToHost()
        {
            networking.Send(QuittingPacket.Instance, hostEndPoint.Value);
        }

        private void SetPeer(byte[] bytes)
        {
            uint address = BitConverter.ToUInt32(bytes, 2);
            ushort port = BitConverter.ToUInt16(bytes, 2 + sizeof(uint));
            IPv4EndPoint peer = new IPv4EndPoint(new IPv4Address(address), port);
            ui.UsePlayerForSlot(bytes[1], peer);
        }

        private void UpdateSettings(UpdateMatchSettingsPacket packet)
        {
#warning HACK: Copying MatchSettings through serialization >.<
            var stream = new MemoryStream();

            var writer = new BinaryWriter(stream);
            packet.Settings.Serialize(writer);
            writer.Flush();

            stream.Position = 0;
            var reader = new BinaryReader(stream, Encoding.UTF8);
            matchSettings.Deserialize(reader);
        }

        private void ForceExit()
        {
            Instant.DisplayAlert(RootView, "You've been disconnected", () => Manager.Pop());
        }
        #endregion

        #region Host
        private void HandlePacketAsHost(GamePacketEventArgs args)
        {
            if (args.Packet is ExploreMatchesPacket)
            {
                Advertize(args.SenderEndPoint);
            }
            else if (args.Packet is JoinRequestPacket)
            {
                TryJoin(args.SenderEndPoint);
            }
            else if (args.Packet is MatchSettingsRequestPacket)
            {
                SendSetup(args.SenderEndPoint);
            }
            else if (args.Packet is QuittingPacket)
            {
                TryLeave(args.SenderEndPoint);
            }
        }

        private void OnSettingsChanged(MatchSettings settings)
        {
            var packet = new UpdateMatchSettingsPacket(settings);
            networking.Send(packet, PlayerEndPoints);
        }

        private byte[] CreateSettingsPacket()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8))
                {
                    writer.Write((byte)SetupMessageType.ChangeOptions);
                    matchSettings.Serialize(writer);
                }

                return stream.ToArray();
            }
        }

        private void SendQuitMessageToClients()
        {
            networking.Send(QuittingPacket.Instance, PlayerEndPoints);
            networking.Broadcast(QuittingPacket.Instance);
        }

        private void Advertize(IPv4EndPoint host)
        {
            int openSlotCount = playerSettings.Players.Count() - maximumPlayerCount;
            var packet = new AdvertizeMatchPacket("Foo", openSlotCount);
            networking.Send(packet, host);
        }

        private void TryJoin(IPv4EndPoint host)
        {
            if (playerSettings.Players.Count() >= maximumPlayerCount)
            {
                networking.Send(JoinResponsePacket.Refused, host);
                return;
            }

            // Send a message indicating we accept the request
            networking.Send(JoinResponsePacket.Accepted, host);

            // Tell the others that a new guy has joined
            byte[] addPeerMessage = new byte[8];
            addPeerMessage[0] = (byte)SetupMessageType.SetPeer;
            //addPeerMessage[1] = (byte)slotIndex;
            BitConverter.GetBytes(host.Address.Value).CopyTo(addPeerMessage, 2);
            BitConverter.GetBytes(host.Port).CopyTo(addPeerMessage, 2 + sizeof(uint));
            networking.Send(addPeerMessage, PlayerEndPoints.Except(host));
        }

        public void SendSetup(IPv4EndPoint targetEndPoint)
        {
            byte[] setSlotMessage = new byte[3];
            setSlotMessage[0] = (byte)SetupMessageType.SetSlot;

            byte[] addPeerMessage = new byte[8];
            addPeerMessage[0] = (byte)SetupMessageType.SetPeer;

            int slotNumber = -1;
            foreach (Player slot in playerSettings.Players)
            {
                slotNumber++;
                if (slotNumber == 0) continue;

                if (slot is RemotePlayer)
                {
                    RemotePlayer remotePlayer = (RemotePlayer)slot;
                    if (remotePlayer.EndPoint == targetEndPoint)
                    {
                        setSlotMessage[1] = (byte)slotNumber;
                        setSlotMessage[2] = (byte)PlayerSlotType.Local;
                        networking.Send(setSlotMessage, targetEndPoint);
                        continue;
                    }

                    IPv4EndPoint peer = ((RemotePlayer)slot).EndPoint;
                    addPeerMessage[1] = (byte)slotNumber;
                    BitConverter.GetBytes(peer.Address.Value).CopyTo(addPeerMessage, 2);
                    BitConverter.GetBytes(peer.Port).CopyTo(addPeerMessage, 2 + sizeof(uint));
                    networking.Send(addPeerMessage, targetEndPoint);
                    continue;
                }

                if (slot is AIPlayer)
                {
                    setSlotMessage[1] = (byte)slotNumber;
                    setSlotMessage[2] = (byte)PlayerSlotType.AI;
                    networking.Send(setSlotMessage, targetEndPoint);
                    continue;
                }
            }

            networking.Send(CreateSettingsPacket(), targetEndPoint);
        }

        private void TryLeave(IPv4EndPoint host)
        {
            int slotNumber = playerSettings.Players.IndexOf(delegate(Player slot)
            {
                if (!(slot is RemotePlayer)) return false;
                RemotePlayer remote = (RemotePlayer)slot;
                return remote.EndPoint == host;
            });

            byte[] setSlotMessage = new byte[3];
            setSlotMessage[0] = (byte)SetupMessageType.SetSlot;
            setSlotMessage[1] = (byte)slotNumber;
            setSlotMessage[2] = (byte)PlayerSlotType.Open;
            foreach (IPv4EndPoint peer in PlayerEndPoints)
                networking.Send(setSlotMessage, peer);
        }
        #endregion
        #endregion
    }
}
