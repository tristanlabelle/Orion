using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Orion.Commandment;
using Orion.Commandment.Pipeline;
using Orion.GameLogic;
using System.Windows.Forms;

namespace Orion.Networking
{
    public sealed class CommandSynchronizer : CommandFilter, IDisposable
    {
        #region Fields
        #region Static
        private const int frameModulo = 6;
        #endregion

        private readonly SafeTransporter transporter;
        private readonly GenericEventHandler<SafeTransporter, NetworkEventArgs> transporterReceived;
        private readonly GenericEventHandler<SafeTransporter, IPv4EndPoint> transporterTimeout;

        private readonly List<IPv4EndPoint> peerEndPoints;
        private readonly Dictionary<IPv4EndPoint, PeerState> peerStates = new Dictionary<IPv4EndPoint, PeerState>();

        private readonly List<Entity> deadEntities = new List<Entity>();

        private readonly List<Command> synchronizedCommands = new List<Command>();

        private readonly CommandFactory serializer;

        private readonly GenericEventHandler<EntityRegistry, Entity> entityDied;

        private int commandFrameNumber;
        private List<NetworkEventArgs> futureCommands = new List<NetworkEventArgs>();
        #endregion

        #region Constructors
        public CommandSynchronizer(World world, SafeTransporter transporter, IEnumerable<IPv4EndPoint> peerEndPoints)
        {
            Argument.EnsureNotNull(world, "world");
            Argument.EnsureNotNull(transporter, "transporter");
            Argument.EnsureNotNull(peerEndPoints, "peerEndPoints");

            this.transporter = transporter;

            this.serializer = new CommandFactory(world);

            this.peerEndPoints = peerEndPoints.ToList();
            if (this.peerEndPoints.Count == 0)
                throw new ArgumentException("Cannot create a CommandSynchronizer without peers.", "peers");

            foreach (IPv4EndPoint peerEndPoint in this.peerEndPoints)
                peerStates.Add(peerEndPoint, PeerState.ReceivedCommands | PeerState.ReceivedDone);

            entityDied = EntityDied;
            world.Entities.Died += entityDied;

            transporterReceived = TransporterReceived;
            transporter.Received += transporterReceived;
            transporterTimeout = TransporterTimedOut;
            transporter.TimedOut += transporterTimeout;
        }
        #endregion

        #region Properties
        private bool ReceivedFromAllPeers
        {
            get { return peerStates.Values.All(state => (state & PeerState.ReceivedCommands) != 0); }
        }

        private bool AllPeersDone
        {
            get { return peerStates.Values.All(state => (state & PeerState.ReceivedDone) != 0); }
        }
        #endregion

        #region Methods
        #region Public
        public void Update(int frameNumber)
        {
            if (frameNumber % frameModulo == 0)
            {
                ++commandFrameNumber;
                WaitForPeerCommands();
                ResetPeerStates();
                Flush();
            }
        }

        public override void EndFeed() { }

        public void Dispose()
        {
            transporter.Received -= transporterReceived;
            transporter.TimedOut -= transporterTimeout;
        }

        private void FeedSynchronizedCommandsToRecipient()
        {
            // FIXME: This sorting might not be flawless. And stable-sorting might not be garanteed by List.
            synchronizedCommands.Sort((a, b) => a.SourceFaction.Name.CompareTo(b.SourceFaction.Name));
            foreach (Command command in synchronizedCommands)
                Recipient.Feed(command);

            synchronizedCommands.Clear();
        }

        private void DeserializeNeededFuturePackets()
        {
            // FIXME: This sorting might not be flawless. And stable-sorting migth not be garanteed by List.
            for (int i = (futureCommands.Count - 1); i >= 0;--i)
            {
                NetworkEventArgs packet = futureCommands.ElementAt(i);
                int packetCommandFrameNumber = BitConverter.ToInt32(packet.Data, 1);
                if (packetCommandFrameNumber == commandFrameNumber)
                {
                    DeserializeGameMessage(packet);
                    futureCommands.RemoveAt(i);
                }
            }
            synchronizedCommands.Sort((a, b) => a.SourceFaction.Name.CompareTo(b.SourceFaction.Name));
            foreach (Command command in synchronizedCommands)
                Recipient.Feed(command);
            synchronizedCommands.Clear();
        }

        private void SynchronizeLocalCommands()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write((byte)GameMessageType.Commands);
                    writer.Write(commandFrameNumber);
                    foreach (Command accumulatedCommand in accumulatedCommands)
                    {
                        if (accumulatedCommand.EntitiesInvolved.Intersect(deadEntities).Any()) continue;
                        serializer.Serialize(accumulatedCommand, writer);
                    }
                }
                deadEntities.Clear();
                transporter.SendTo(stream.ToArray(), peerStates.Keys);
            }

            synchronizedCommands.AddRange(accumulatedCommands);
            accumulatedCommands.Clear();
        }
        #endregion

        #region Private
        private void WaitForPeerCommands()
        {
            if (!ReceivedFromAllPeers || !AllPeersDone)
            {
                transporter.Poll();
                while (!ReceivedFromAllPeers || !AllPeersDone)
                {
                    Thread.Sleep(0);
                    transporter.Poll();
                }
            }
        }

        private void ResetPeerStates()
        {
            foreach (IPv4EndPoint peerEndPoint in peerEndPoints)
                peerStates[peerEndPoint] = PeerState.None;
        }

        private void TransporterReceived(SafeTransporter source, NetworkEventArgs args)
        {
            int packetFrameNumber = BitConverter.ToInt32(args.Data, 1);
            
            if (packetFrameNumber > commandFrameNumber)
            {
                futureCommands.Add(args);
            }
            else if (packetFrameNumber == commandFrameNumber)
            {
                DeserializeGameMessage(args);
            }
        }

        private void DeserializeGameMessage(NetworkEventArgs args)
        {
            byte messageType = args.Data[0];
            PeerState oldPeerState = peerStates[args.Host];
            if (messageType == (byte)GameMessageType.Commands)
            {
                Deserialize(args.Data.Skip(1 + sizeof(Int32)).ToArray());

                //if ((oldPeerState & PeerState.ReceivedCommands) != 0)
                //    throw new InvalidDataException("Received multiple commands from the same peer in a frame.");
                peerStates[args.Host] = oldPeerState | PeerState.ReceivedCommands;

                if (ReceivedFromAllPeers)
                {
                    byte[] doneMessage = new byte[5];
                    doneMessage[0] = (byte) GameMessageType.Done;
                    BitConverter.GetBytes(commandFrameNumber).CopyTo(doneMessage,1);
                    
                    transporter.SendTo(doneMessage, peerEndPoints);
                }
            }
            else if (messageType == (byte)GameMessageType.Done)
            {
                //if ((oldPeerState & PeerState.ReceivedDone) != 0)
                //    throw new InvalidDataException("Received multiple done from the same peer in a frame.");
                peerStates[args.Host] = oldPeerState | PeerState.ReceivedDone;
            }
        }

        private void TransporterTimedOut(SafeTransporter source, IPv4EndPoint endPoint)
        {
            MessageBox.Show("Lost connection to {0}!".FormatInvariant(endPoint));
            peerStates.Remove(endPoint);
        }

        private void Deserialize(byte[] array)
        {
            using (MemoryStream stream = new MemoryStream(array))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    while (stream.Position != stream.Length)
                    {
                        synchronizedCommands.Add(serializer.Deserialize(reader));
                    }
                }
            }
        }

        private void EntityDied(EntityRegistry registry, Entity deadEntity)
        {
            Unit deadUnit = deadEntity as Unit;
            if (deadUnit != null) deadEntities.Add(deadUnit);
        }
        #endregion
        #endregion
    }
}