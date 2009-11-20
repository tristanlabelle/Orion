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
using System.Diagnostics;

namespace Orion.Networking
{
    /// <summary>
    /// A command filter which synchronizes commands with other players
    /// before passing them on.
    /// </summary>
    public sealed class CommandSynchronizer : CommandFilter
    {
        #region Fields
        #region Static
        private const int frameTimesQueueLength = 50;
        private const int initialFramesCount = 6;
        #endregion
        private readonly SafeTransporter transporter;
        private readonly GenericEventHandler<SafeTransporter, NetworkEventArgs> transporterReceived;
        private readonly GenericEventHandler<SafeTransporter, IPv4EndPoint> transporterTimeout;

        private readonly List<IPv4EndPoint> peerEndPoints;
        private readonly Dictionary<IPv4EndPoint, PeerState> peerStates = new Dictionary<IPv4EndPoint, PeerState>();

        private readonly List<Entity> deadEntities = new List<Entity>();

        /// <summary>
        /// The commands that have been passed to this CommandSynchroniser by the pipeline.
        /// </summary>
        private readonly List<Command> localCommands = new List<Command>();

        /// <summary>
        /// An accumulator for commands that will be executed at the end of the frame.
        /// Used in RunCommandFrame.
        /// </summary>
        private readonly List<Command> commandsToBeFlushed = new List<Command>();

        /// <summary>
        /// The commands that were generated locally during the last frame that will be
        /// executed one frame later.
        /// </summary>
        private readonly List<Command> lastFrameLocalCommands = new List<Command>();

        /// <summary>
        /// Packets which describes events related to future frames and that should not be
        /// deserialized until then.
        /// </summary>
        private readonly List<NetworkEventArgs> futureFramePackets = new List<NetworkEventArgs>();

        private readonly CommandFactory serializer;

        private readonly GenericEventHandler<EntityRegistry, Entity> entityDied;

        private int commandFrameNumber = -1;
        private int frameNumber = -1;
        private int lastCommandFrame = -1;
        private Queue<int> frameTimes = new Queue<int>();
        private List<byte> receivedFrameTimes = new List<byte>();
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

            frameTimes.Enqueue(initialFramesCount);
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
        public override void Handle(Command command)
        {
            Argument.EnsureNotNull(command, "command");
            localCommands.Add(command);
        }

        public override void Update(int updateNumber, float timeDeltaInSeconds)
        {
            int frameModulo = (int)(frameTimes.Average() + 0.5);
            frameNumber = updateNumber;
            if (updateNumber % frameModulo == 0)
            {
                RunCommandFrame();
                lastCommandFrame = updateNumber;
            }
        }

        public override void Dispose()
        {
            transporter.Received -= transporterReceived;
            transporter.TimedOut -= transporterTimeout;
        }

        public void RunCommandFrame()
        {
            ++commandFrameNumber;
            SendLocalCommands();

            if (commandFrameNumber == 0)
            {
                // Don't execute commands in the first command frame as no commands are ready yet.
                SendDone();
            }
            else
            {
                ResetPeerStates();
                DeserializeNeededFuturePackets();
                WaitForPeerCommands();
                commandsToBeFlushed.AddRange(lastFrameLocalCommands);
                lastFrameLocalCommands.Clear();
            }

            frameTimes.Enqueue(receivedFrameTimes.Max());
            if (frameTimes.Count > 50)
                frameTimes.Dequeue();

            ExecuteCurrentFrameCommands();
            commandsToBeFlushed.Clear();

            lastFrameLocalCommands.AddRange(localCommands);
            localCommands.Clear();
        }

        private void ExecuteCurrentFrameCommands()
        {
            // FIXME: This sorting might not be flawless. And stable-sorting might not be garanteed by List.
            commandsToBeFlushed.Sort((a, b) => a.SourceFaction.Name.CompareTo(b.SourceFaction.Name));
            foreach (Command command in commandsToBeFlushed)
                Flush(command);
            commandsToBeFlushed.Clear();
        }

        private void DeserializeNeededFuturePackets()
        {
            for (int i = (futureFramePackets.Count - 1); i >= 0; --i)
            {
                NetworkEventArgs packet = futureFramePackets[i];
                int packetCommandFrameNumber = BitConverter.ToInt32(packet.Data, 1);
                if (packetCommandFrameNumber < commandFrameNumber)
                {
                    Debug.Assert(packetCommandFrameNumber == commandFrameNumber - 1);
                    DeserializeGameMessage(packet);
                    futureFramePackets.RemoveAt(i);
                }
            }
        }

        private void SendLocalCommands()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write((byte)GameMessageType.Commands);
                    writer.Write(commandFrameNumber);
                    foreach (Command command in localCommands)
                    {
                        if (command.EntitiesInvolved.Intersect(deadEntities).Any()) continue;
                        serializer.Serialize(command, writer);
                    }
                }
                deadEntities.Clear();
                transporter.SendTo(stream.ToArray(), peerStates.Keys);
            }
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
                lastCommandFrame--;
            }
        }

        private void ResetPeerStates()
        {
            foreach (IPv4EndPoint peerEndPoint in peerEndPoints)
                peerStates[peerEndPoint] = PeerState.None;
        }

        private void TransporterReceived(SafeTransporter source, NetworkEventArgs args)
        {
            int packetCommandFrameNumber = BitConverter.ToInt32(args.Data, 1);
            if (packetCommandFrameNumber < commandFrameNumber)
            {
                Debug.Assert(packetCommandFrameNumber == commandFrameNumber - 1);
                DeserializeGameMessage(args);
            }
            else
            {
                futureFramePackets.Add(args);
            }
        }

        private void DeserializeGameMessage(NetworkEventArgs args)
        {
            byte messageType = args.Data[0];
            PeerState oldPeerState = peerStates[args.Host];
            if (messageType == (byte)GameMessageType.Commands)
            {
                Deserialize(args.Data, 1 + sizeof(int));
                peerStates[args.Host] = oldPeerState | PeerState.ReceivedCommands;

                if (ReceivedFromAllPeers) SendDone();
            }
            else if (messageType == (byte)GameMessageType.Done)
            {
                peerStates[args.Host] = oldPeerState | PeerState.ReceivedDone;
                receivedFrameTimes.Add(args.Data[5]);
            }
        }

        private void SendDone()
        {
            byte framesNeeded = (byte)(frameNumber - lastCommandFrame);
            lastCommandFrame = frameNumber;
            receivedFrameTimes.Add(framesNeeded);

            byte[] doneMessage = new byte[6];
            doneMessage[0] = (byte)GameMessageType.Done;
            BitConverter.GetBytes(commandFrameNumber).CopyTo(doneMessage, 1);
            doneMessage[5] = framesNeeded;

            transporter.SendTo(doneMessage, peerEndPoints);
        }

        private void TransporterTimedOut(SafeTransporter source, IPv4EndPoint endPoint)
        {
            Debug.Fail("Lost connection to {0}!".FormatInvariant(endPoint));
            peerStates.Remove(endPoint);
        }

        private void Deserialize(byte[] array, int startIndex)
        {
            using (MemoryStream stream = new MemoryStream(array, startIndex, array.Length - startIndex))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    while (stream.Position != stream.Length)
                    {
                        Command deserializedCommand = serializer.Deserialize(reader);
                        commandsToBeFlushed.Add(deserializedCommand);
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