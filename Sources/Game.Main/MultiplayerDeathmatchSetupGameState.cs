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
using Orion.Game.Matchmaking.Deathmatch;
using Orion.Game.Matchmaking.Networking;
using Orion.Game.Presentation;
using Orion.Game.Presentation.Gui;
using Orion.Game.Simulation;

namespace Orion.Game.Main
{
    /// <summary>
    /// Handles the initialization, updating and cleanup logic of
    /// the multiplayer menu to setup a deatchmatch.
    /// </summary>
    public sealed class MultiplayerDeathmatchSetupGameState : GameState
    {
        #region Fields
        private static readonly byte[] advertiseGameMessage = new byte[2] { (byte)SetupMessageType.Advertise, 12 };
        private static readonly byte[] refuseJoinGameMessage = new byte[] { (byte)SetupMessageType.RefuseJoinRequest };
        
        private readonly GameGraphics graphics;
        private readonly SafeTransporter transporter;
        private readonly IPv4EndPoint? hostEndPoint;
        private readonly MatchSettings matchSettings;
        private readonly MultiplayerMatchConfigurationUI ui;
        private Action<SafeTransporter, NetworkEventArgs> packetReceivedEventHandler;
        private Action<SafeTransporter, IPv4EndPoint> peerTimedOutEventHandler;
        #endregion

        #region Constructors
        public MultiplayerDeathmatchSetupGameState(GameStateManager manager, GameGraphics graphics,
            SafeTransporter transporter, IPv4EndPoint? hostEndPoint)
            : base(manager)
        {
            Argument.EnsureNotNull(graphics, "graphics");
            Argument.EnsureNotNull(transporter, "transporter");

            this.graphics = graphics;
            this.transporter = transporter;
            this.hostEndPoint = hostEndPoint;
            this.matchSettings = new MatchSettings();
            this.ui = new MultiplayerMatchConfigurationUI(matchSettings, transporter, !hostEndPoint.HasValue);
            this.packetReceivedEventHandler = OnPacketReceived;
            this.peerTimedOutEventHandler = OnPeerTimedOut;

            this.ui.InitializeSlots();
            if (hostEndPoint.HasValue) this.ui.UsePlayerForSlot(0, hostEndPoint.Value);

            this.matchSettings.Changed += OnSettingsChanged;
            this.ui.StartGamePressed += OnStartGamePressed;
            this.ui.ExitPressed += OnExitPressed;
            this.ui.SlotOccupationChanged += OnSlotOccupationChanged;
            this.ui.PlayerKicked += OnPlayerKicked;
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
        #endregion

        #region Methods
        #region Overrides
        protected internal override void OnEntered()
        {
            OnUnshadowed();

            if (!IsHost)
            {
                byte[] data = new byte[1];
                data[0] = (byte)SetupMessageType.GetSetup;
                transporter.SendTo(data, hostEndPoint.Value);
            }
        }

        protected internal override void OnShadowed()
        {
            transporter.Received -= packetReceivedEventHandler;
            transporter.TimedOut -= peerTimedOutEventHandler;
            RootView.Children.Remove(ui);
        }

        protected internal override void OnUnshadowed()
        {
            RootView.Children.Add(ui);
            this.transporter.Received += this.packetReceivedEventHandler;
            this.transporter.TimedOut += this.peerTimedOutEventHandler;
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
            foreach (PlayerSlot slot in ui.Players)
            {
                if (!slot.NeedsFaction) continue;

                ColorRgb color = Faction.Colors[colorIndex];
                colorIndex++;

                string factionName = Colors.GetName(color);
                Faction faction = world.CreateFaction(factionName, color);
                faction.AladdiumAmount = matchSettings.InitialAladdiumAmount;
                faction.AlageneAmount = matchSettings.InitialAlageneAmount;
                if (matchSettings.RevealTopology) faction.LocalFogOfWar.Reveal();

                if (slot is LocalPlayerSlot)
                {
                    localCommander = new SlaveCommander(match, faction);
                }
                else if (slot is AIPlayerSlot)
                {
                    Commander commander = new AgressiveAICommander(match, faction);
                    aiCommanders.Add(commander);
                }
                else if (slot is RemotePlayerSlot) // no commanders for remote players
                {
                    RemotePlayerSlot remotePlayerSlot = (RemotePlayerSlot)slot;
                    IPv4EndPoint endPoint = remotePlayerSlot.HostEndPoint.Value;
                    FactionEndPoint peer = new FactionEndPoint(transporter, faction, endPoint);
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
            commandPipeline.PushFilter(new CommandSynchronizer(match, transporter, peers));
            commandPipeline.PushFilter(new CommandOptimizer());

            aiCommanders.ForEach(commander => commandPipeline.AddCommander(commander, aiCommandSink));
            commandPipeline.AddCommander(localCommander);

            GameState targetGameState = new DeathmatchGameState(Manager, graphics,
                match, commandPipeline, localCommander);
            Manager.Push(targetGameState);
        }
        #endregion

        #region Event Handlers
        private void OnPacketReceived(SafeTransporter sender, NetworkEventArgs args)
        {
            if (IsHost) HandlePacketAsHost(args);
            else HandlePacketAsClient(args);
        }

        private void OnPeerTimedOut(SafeTransporter sender, IPv4EndPoint timedOutHostEndPoint)
        {
            if (IsHost)
            {
                if (ui.PlayerAddresses.Contains(timedOutHostEndPoint))
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

            byte[] startGameMessage = new byte[1];
            startGameMessage[0] = (byte)SetupMessageType.RemoveGame;
            transporter.Broadcast(startGameMessage, transporter.Port);
            startGameMessage[0] = (byte)SetupMessageType.StartGame;
            transporter.SendTo(startGameMessage, ui.PlayerAddresses);
            StartGame();
        }

        private void OnExitPressed(MatchConfigurationUI sender)
        {
            if (IsHost) SendQuitMessageToClients();
            else SendQuitMessageToHost();

            Manager.Pop();
        }

        private void OnSlotOccupationChanged(MultiplayerMatchConfigurationUI sender,
            int slotNumber, PlayerSlot newValue)
        {
            byte[] setSlotMessage = new byte[3];
            setSlotMessage[0] = (byte)SetupMessageType.SetSlot;
            setSlotMessage[1] = (byte)slotNumber;
            if (newValue is RemotePlayerSlot) setSlotMessage[2] = (byte)PlayerSlotType.Open;
            else if (newValue is AIPlayerSlot) setSlotMessage[2] = (byte)PlayerSlotType.AI;
            else if (newValue is ClosedPlayerSlot) setSlotMessage[2] = (byte)PlayerSlotType.Closed;
            else throw new InvalidOperationException("Unknown slot type selected");

            transporter.SendTo(setSlotMessage, ui.PlayerAddresses);
        }

        private void OnPlayerKicked(MultiplayerMatchConfigurationUI sender, IPv4EndPoint peer)
        {
            byte[] kickMessage = new byte[1];
            kickMessage[0] = (byte)SetupMessageType.Exit;
            transporter.SendTo(kickMessage, peer);
        }
        #endregion

        #region Client
        private void HandlePacketAsClient(NetworkEventArgs args)
        {
            if (args.Host != hostEndPoint.Value)
            {
                Debug.Fail("Received a packet from an unexpected peer.");
                return;
            }

            byte[] data = args.Data;
            SetupMessageType setupMessageType = (SetupMessageType)data[0];
            switch (setupMessageType)
            {
                case SetupMessageType.SetPeer: SetPeer(data); break;
                case SetupMessageType.SetSlot: SetSlot(data); break;
                case SetupMessageType.StartGame: StartGame(); break;
                case SetupMessageType.ChangeOptions: SetOptions(args.Host, data); break;
                case SetupMessageType.Exit: ForceExit(); break;
                default:
                    Debug.Fail("Unexpected setup message type {0}.".FormatInvariant(setupMessageType));
                    break;
            }
        }

        private void SendQuitMessageToHost()
        {
            byte[] quitMessage = new byte[1];
            quitMessage[0] = (byte)SetupMessageType.LeaveGame;
            transporter.SendTo(quitMessage, hostEndPoint.Value);
        }

        private void SetPeer(byte[] bytes)
        {
            uint address = BitConverter.ToUInt32(bytes, 2);
            ushort port = BitConverter.ToUInt16(bytes, 2 + sizeof(uint));
            IPv4EndPoint peer = new IPv4EndPoint(new IPv4Address(address), port);
            ui.UsePlayerForSlot(bytes[1], peer);
        }

        private void SetSlot(byte[] bytes)
        {
            PlayerSlotType slotType = (PlayerSlotType)bytes[2];
            switch (slotType)
            {
                case PlayerSlotType.Closed: ui.CloseSlot(bytes[1]); break;
                case PlayerSlotType.Open: ui.OpenSlot(bytes[1]); break;
                case PlayerSlotType.AI: ui.UseAIForSlot(bytes[1]); break;
                case PlayerSlotType.Local: ui.SetLocalPlayerForSlot(bytes[1]); break;
                default:
                    Debug.Fail("Unexpected slot type {0}.".FormatInvariant(slotType));
                    break;
            }
        }

        private void SetOptions(IPv4EndPoint host, byte[] bytes)
        {
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    SetupMessageType messageType = (SetupMessageType)reader.ReadByte();
                    Debug.Assert(messageType == SetupMessageType.ChangeOptions);

                    matchSettings.Deserialize(reader);

                    Debug.Assert(reader.PeekChar() == -1, "Warning: The options packet contained more data than we read.");
                }
            }
        }

        private void ForceExit()
        {
            Instant.DisplayAlert(RootView, "You've been disconnected", () => Manager.Pop());
        }
        #endregion

        #region Host
        private void HandlePacketAsHost(NetworkEventArgs args)
        {
            byte[] message = args.Data;
            SetupMessageType setupMessageType = (SetupMessageType)message[0];
            switch (setupMessageType)
            {
                case SetupMessageType.Explore:
                    Advertise(args.Host);
                    break;

                case SetupMessageType.JoinRequest:
                    TryJoin(args.Host);
                    break;

                case SetupMessageType.GetSetup:
                    SendSetup(args.Host);
                    break;

                case SetupMessageType.LeaveGame:
                    TryLeave(args.Host);
                    break;

                default:
                    //Debug.Fail("Unexpected setup message type {0}.".FormatInvariant(setupMessageType));
                    break;
            }
        }

        private void OnSettingsChanged(MatchSettings settings)
        {
            transporter.SendTo(CreateSettingsPacket(), ui.PlayerAddresses);
        }

        private byte[] CreateSettingsPacket()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write((byte)SetupMessageType.ChangeOptions);
                    matchSettings.Serialize(writer);
                }

                return stream.ToArray();
            }
        }

        private void SendQuitMessageToClients()
        {
            byte[] exitMessage = new byte[1];
            exitMessage[0] = (byte)SetupMessageType.Exit;
            transporter.SendTo(exitMessage, ui.PlayerAddresses);
            transporter.Broadcast(exitMessage, transporter.Port);
        }

        private void Advertise(IPv4EndPoint host)
        {
            int leftSlots = ui.Players.OfType<RemotePlayerSlot>().Count(slot => !slot.HostEndPoint.HasValue);
            advertiseGameMessage[1] = (byte)leftSlots;
            transporter.SendTo(advertiseGameMessage, host);
        }

        private void TryJoin(IPv4EndPoint host)
        {
            if (ui.NumberOfPlayers >= ui.MaxNumberOfPlayers)
            {
                transporter.SendTo(refuseJoinGameMessage, host);
                return;
            }

            // Send a message indicating we accept the request
            byte[] accept = new byte[1];
            accept[0] = (byte)SetupMessageType.AcceptJoinRequest;
            transporter.SendTo(accept, host);

            // Set the slot as occupied
            int slotIndex = (byte)ui.NextAvailableSlot;
            ui.UsePlayerForSlot(slotIndex, host);

            // Tell the others that a new guy has joined
            byte[] addPeerMessage = new byte[8];
            addPeerMessage[0] = (byte)SetupMessageType.SetPeer;
            addPeerMessage[1] = (byte)slotIndex;
            BitConverter.GetBytes(host.Address.Value).CopyTo(addPeerMessage, 2);
            BitConverter.GetBytes(host.Port).CopyTo(addPeerMessage, 2 + sizeof(uint));
            transporter.SendTo(addPeerMessage, ui.PlayerAddresses.Except(host));
        }

        public void SendSetup(IPv4EndPoint targetEndPoint)
        {
            byte[] setSlotMessage = new byte[3];
            setSlotMessage[0] = (byte)SetupMessageType.SetSlot;

            byte[] addPeerMessage = new byte[8];
            addPeerMessage[0] = (byte)SetupMessageType.SetPeer;

            int slotNumber = -1;
            foreach (PlayerSlot slot in ui.Players)
            {
                slotNumber++;
                if (slotNumber == 0) continue;

                if (slot is RemotePlayerSlot)
                {
                    RemotePlayerSlot remotePlayer = (RemotePlayerSlot)slot;
                    if (!remotePlayer.HostEndPoint.HasValue)
                    {
                        setSlotMessage[1] = (byte)slotNumber;
                        setSlotMessage[2] = (byte)PlayerSlotType.Open;
                        transporter.SendTo(setSlotMessage, targetEndPoint);
                        continue;
                    }

                    if (remotePlayer.HostEndPoint == targetEndPoint)
                    {
                        setSlotMessage[1] = (byte)slotNumber;
                        setSlotMessage[2] = (byte)PlayerSlotType.Local;
                        transporter.SendTo(setSlotMessage, targetEndPoint);
                        continue;
                    }

                    IPv4EndPoint peer = ((RemotePlayerSlot)slot).HostEndPoint.Value;
                    addPeerMessage[1] = (byte)slotNumber;
                    BitConverter.GetBytes(peer.Address.Value).CopyTo(addPeerMessage, 2);
                    BitConverter.GetBytes(peer.Port).CopyTo(addPeerMessage, 2 + sizeof(uint));
                    transporter.SendTo(addPeerMessage, targetEndPoint);
                    continue;
                }

                if (slot is ClosedPlayerSlot)
                {
                    setSlotMessage[1] = (byte)slotNumber;
                    setSlotMessage[2] = (byte)PlayerSlotType.Closed;
                    transporter.SendTo(setSlotMessage, targetEndPoint);
                    continue;
                }

                if (slot is AIPlayerSlot)
                {
                    setSlotMessage[1] = (byte)slotNumber;
                    setSlotMessage[2] = (byte)PlayerSlotType.AI;
                    transporter.SendTo(setSlotMessage, targetEndPoint);
                    continue;
                }
            }

            transporter.SendTo(CreateSettingsPacket(), targetEndPoint);
        }

        private void TryLeave(IPv4EndPoint host)
        {
            if (!ui.PlayerAddresses.Contains(host)) return;

            int slotNumber = ui.Players.IndexOf(delegate(PlayerSlot slot)
            {
                if (!(slot is RemotePlayerSlot)) return false;
                RemotePlayerSlot remote = (RemotePlayerSlot)slot;
                return remote.HostEndPoint.HasValue && remote.HostEndPoint.Value == host;
            });

            byte[] setSlotMessage = new byte[3];
            setSlotMessage[0] = (byte)SetupMessageType.SetSlot;
            setSlotMessage[1] = (byte)slotNumber;
            setSlotMessage[2] = (byte)PlayerSlotType.Open;
            foreach (IPv4EndPoint peer in ui.PlayerAddresses)
                transporter.SendTo(setSlotMessage, peer);

            ui.OpenSlot(slotNumber);
        }
        #endregion
        #endregion
    }
}
