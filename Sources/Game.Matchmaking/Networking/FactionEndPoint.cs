using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Networking;
using Orion.Game.Simulation;
using Orion.Game.Matchmaking;
using Orion.Game.Matchmaking.Commands;
using Orion.Engine.Collections;
using Orion.Game.Matchmaking.Networking.Packets;

namespace Orion.Game.Matchmaking.Networking
{
    public sealed class FactionEndPoint : IDisposable
    {
        #region Fields
        private readonly GameNetworking networking;
        private readonly IPv4EndPoint hostEndPoint;
        private readonly Faction faction;

        private readonly Dictionary<int, List<Command>> availableCommands = new Dictionary<int, List<Command>>();
        private readonly Dictionary<int, int> updatesForDone = new Dictionary<int, int>();

        private Action<GameNetworking, GamePacketEventArgs> packetReceivedEventHandler;
        private Action<GameNetworking, IPv4EndPoint> peerTimedOutEventHandler;

        private bool hasDesynced;
        #endregion

        #region Constructors
        public FactionEndPoint(GameNetworking networking, Faction faction, IPv4EndPoint hostEndPoint)
        {
            Argument.EnsureNotNull(networking, "networking");
            Argument.EnsureNotNull(faction, "faction");

            this.networking = networking;
            this.faction = faction;
            this.hostEndPoint = hostEndPoint;

            this.packetReceivedEventHandler = OnPacketReceived;
            this.peerTimedOutEventHandler = OnPeerTimedOut;

            this.networking.PacketReceived += packetReceivedEventHandler;
            this.networking.PeerTimedOut += peerTimedOutEventHandler;

            availableCommands[0] = new List<Command>();
        }
        #endregion

        #region Properties
        public Faction Faction
        {
            get { return faction; }
        }
        #endregion

        #region Methods
        public bool IsDoneForFrame(int commandFrame)
        {
            return updatesForDone.ContainsKey(commandFrame);
        }

        public int GetUpdatesForCommandFrame(int commandFrame)
        {
            return updatesForDone[commandFrame];
        }

        public bool HasCommandsForCommandFrame(int commandFrame)
        {
            return availableCommands.ContainsKey(commandFrame);
        }

        public List<Command> GetCommandsForCommandFrame(int commandFrame)
        {
            if (!HasCommandsForCommandFrame(commandFrame))
                throw new InvalidOperationException("Did not receive commands for frame {0}".FormatInvariant(commandFrame));

            return availableCommands[commandFrame];
        }

        public void SendLeave()
        {
            networking.Send(new RemovePlayerPacket(), hostEndPoint);
        }

        public void SendDone(CommandFrameCompletedPacket packet)
        {
            networking.Send(packet, hostEndPoint);
        }

        private List<Command> DeserializeCommandDatagram(byte[] data, int startIndex)
        {
            Subarray<byte> subarray = new Subarray<byte>(data, startIndex);
            List<Command> commands = Command.Serializer.DeserializeToEnd(subarray);

            Command firstMindControlledCommand = commands
                .FirstOrDefault(c => c.FactionHandle != faction.Handle);
            if (firstMindControlledCommand != null)
            {
                Debug.Fail("Faction {0} is mind controlling faction {1}."
                    .FormatInvariant(faction.Handle, firstMindControlledCommand.FactionHandle));
            }

            return commands;
        }

        public void SendCommands(int commandFrameNumber, IEnumerable<Command> commands)
        {
            var packet = new CommandsPacket(commandFrameNumber, commands);
            networking.Send(packet, hostEndPoint);
        }

        private void OnPacketReceived(GameNetworking networking, GamePacketEventArgs args)
        {
            if (args.SenderEndPoint != hostEndPoint) return;

            if (args.Packet is RemovePlayerPacket)
            {
                faction.MassSuicide();
            }
            else if (args.Packet is CommandsPacket)
            {
                var packet = (CommandsPacket)args.Packet;
                availableCommands[packet.CommandFrameNumber] = packet.Commands.ToList();
            }
            else if (args.Packet is CommandFrameCompletedPacket)
            {
                var packet = (CommandFrameCompletedPacket)args.Packet;
                updatesForDone[packet.CommandFrameNumber] = packet.UpdateFrameCount;

#if DEBUG
                if (!hasDesynced && packet.WorldStateHashCode != 0)
                {
                    int worldStateHashCode = faction.World.GetStateHashCode();
                    if (worldStateHashCode != packet.WorldStateHashCode)
                    {
                        Debug.Fail("Desync detected with faction {0} at ip {1}!"
                            .FormatInvariant(faction.Name, hostEndPoint));
                        hasDesynced = true;
                    }
                }
#endif
            }
        }

        private void OnPeerTimedOut(GameNetworking networking, IPv4EndPoint endPoint)
        {
            if (endPoint != hostEndPoint) return;

            faction.RaiseWarning("Perdu la connexion à {0}.".FormatInvariant(endPoint));
            faction.MarkAsDefeated();
        }

        public void Dispose()
        {
            networking.PacketReceived -= packetReceivedEventHandler;
            networking.PeerTimedOut -= peerTimedOutEventHandler;
        }
        #endregion
    }
}
