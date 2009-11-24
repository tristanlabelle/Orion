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
        private const int frameTimesQueueLength = 0x20;
        private const int initialFramesCount = 6;
        #endregion

        private readonly World world;
        private readonly SafeTransporter transporter;
        private readonly GenericEventHandler<SafeTransporter, NetworkEventArgs> transporterReceived;
        private readonly GenericEventHandler<SafeTransporter, IPv4EndPoint> transporterTimeout;

        private readonly List<IPv4EndPoint> peerEndPoints;
        private readonly Dictionary<IPv4EndPoint, PeerState> peerStates = new Dictionary<IPv4EndPoint, PeerState>();

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

        private int commandFrameNumber = -1;
        private int frameNumber = -1;
        private int lastCommandFrame = -1;
        private Queue<int> frameTimes = new Queue<int>();
        private List<byte> receivedFrameTimes = new List<byte>();
        #endregion

        #region Constructors
        public CommandSynchronizer(Match match, SafeTransporter transporter, IEnumerable<IPv4EndPoint> peerEndPoints)
        {
            Argument.EnsureNotNull(match, "match");
            Argument.EnsureNotNull(transporter, "transporter");
            Argument.EnsureNotNull(peerEndPoints, "peerEndPoints");

            world = match.World;
            this.transporter = transporter;
            match.Quitting += SafeQuit;

            this.peerEndPoints = peerEndPoints.ToList();
            if (this.peerEndPoints.Count == 0)
                throw new ArgumentException("Cannot create a CommandSynchronizer without peers.", "peers");

            foreach (IPv4EndPoint peerEndPoint in this.peerEndPoints)
                peerStates.Add(peerEndPoint, PeerState.ReceivedCommands | PeerState.ReceivedDone);

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

            AdaptFrameModulo();

            ExecuteCurrentFrameCommands();
            commandsToBeFlushed.Clear();

            lastFrameLocalCommands.AddRange(localCommands);
            localCommands.Clear();
        }
        #endregion

        #region Private
        private void AdaptFrameModulo()
        {
            int oldFrameModulo = (int)(frameTimes.Average() + 0.5);
            frameTimes.Enqueue(receivedFrameTimes.Max());
            if (frameTimes.Count > frameTimesQueueLength)
                frameTimes.Dequeue();
            int newFrameModulo = (int)(frameTimes.Average() + 0.5);
            if (newFrameModulo != oldFrameModulo)
                Debug.WriteLine("Command frame modulo changed from {0} to {1}".FormatInvariant(oldFrameModulo, newFrameModulo));
        }

        private void SafeQuit(Match match)
        {
            byte[] quitPacket = new byte[1] { (byte)GameMessageType.Quit };
            transporter.SendTo(quitPacket, peerStates.Keys);
            Dispose();
        }

        private void ExecuteCurrentFrameCommands()
        {
            // OrderBy garantees stable-sorting: http://msdn.microsoft.com/en-us/library/bb534966.aspx
            foreach (Command command in commandsToBeFlushed.OrderBy(c => c.FactionHandle.Value))
                Flush(command);
            commandsToBeFlushed.Clear();
        }

        private int CommandSortingComparer(Command a, Command b)
        {
            Faction factionA = world.FindFactionFromHandle(a.FactionHandle);
            Faction factionB = world.FindFactionFromHandle(b.FactionHandle);
            return factionA.Name.CompareTo(factionB.Name);
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
                        command.Serialize(writer);
                }
                transporter.SendTo(stream.ToArray(), peerStates.Keys);
            }
        }

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

        private void RemoveFromPeers(IPv4EndPoint host)
        {
            peerEndPoints.Remove(host);
            peerStates.Remove(host);
        }

        private void TransporterReceived(SafeTransporter source, NetworkEventArgs args)
        {
            if (args.Data[0] == (byte)GameMessageType.Commands ||
                args.Data[0] == (byte)GameMessageType.Done)
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
            else if (args.Data[0] == (byte)GameMessageType.Quit)
            {
                RemoveFromPeers(args.Host);
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
                        Command deserializedCommand = Command.Deserialize(reader);
                        commandsToBeFlushed.Add(deserializedCommand);
                    }
                }
            }
        }
        #endregion
        #endregion
    }
}