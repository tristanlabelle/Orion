using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Orion.Commandment;
using Orion.Commandment.Pipeline;
using Orion.GameLogic;

namespace Orion.Networking
{
    public enum GameMessageType : byte
    {
        Commands,
        Done
    }

    public sealed class CommandSynchronizer : CommandFilter, IDisposable
    {
        #region Fields
        #region Static
        private const int frameModulo = 6;
        private static readonly byte[] doneMessage = { (byte)GameMessageType.Done };
        #endregion

        private readonly Transporter transporter;
        private readonly GenericEventHandler<Transporter, NetworkEventArgs> transporterReceived;
        private readonly GenericEventHandler<Transporter, NetworkTimeoutEventArgs> transporterTimeout;

        private readonly List<IPEndPoint> peers;
        private readonly Dictionary<IPEndPoint, bool> receivedFromPeers = new Dictionary<IPEndPoint, bool>();
        private readonly Dictionary<IPEndPoint, bool> peersCompleted = new Dictionary<IPEndPoint, bool>();

        private readonly List<Entity> deadEntities = new List<Entity>();

        private readonly List<Command> synchronizedCommands = new List<Command>();

        private readonly CommandFactory serializer;

        private readonly GenericEventHandler<EntityRegistry, Entity> entityDied;
        #endregion

        #region Constructors

        public CommandSynchronizer(World world, Transporter transporter, IEnumerable<IPEndPoint> peers)
        {
            Argument.EnsureNotNull(world, "world");
            Argument.EnsureNotNull(transporter, "transporter");
            Argument.EnsureNotNull(peers, "peers");

            this.transporter = transporter;
            this.peers = peers.ToList();
            if (this.peers.Count == 0) throw new ArgumentException("Cannot create a CommandSynchronizer without peers.", "peers");

            this.serializer = new CommandFactory(world);

            foreach (IPEndPoint endpoint in peers)
            {
                receivedFromPeers[endpoint] = true;
                peersCompleted[endpoint] = true;
            }

            entityDied = EntityDied;
            world.Entities.Died += entityDied;

            transporterReceived = new GenericEventHandler<Transporter, NetworkEventArgs>(TransporterReceived);
            transporter.Received += transporterReceived;
            transporterTimeout = new GenericEventHandler<Transporter, NetworkTimeoutEventArgs>(TransporterTimedOut);
            transporter.TimedOut += transporterTimeout;
        }

        #endregion

        #region Properties
        private bool ReadyToContinue
        {
            get { return ReceivedFromAllPeers && AllPeersReady; }
        }

        private bool ReceivedFromAllPeers
        {
            get { return receivedFromPeers.Values.All(received => received); }
        }

        private bool AllPeersReady
        {
            get { return peersCompleted.Values.All(completed => completed); }
        }
        #endregion

        #region Methods
        #region Public
        public void Update(int frameNumber)
        {
            if (frameNumber % frameModulo == 0)
            {
                WaitForPeerCommands();
                Flush();
                ResetPeerStatuses();
            }
        }

        public override void EndFeed() { }

        public void Dispose()
        {
            transporter.Received -= transporterReceived;
            transporter.TimedOut -= transporterTimeout;
        }

        public override void Flush()
        {
            if (Recipient == null) throw new InvalidOperationException("Sink's recipient must not be null when Flush() is called");

            // The order here is important because synchronizedCommands is accessed in both methods.
            BeginSynchronizationOfAccumulatedCommands();
            FeedSynchronizedCommandsToRecipient();

            Recipient.EndFeed();
        }

        private void FeedSynchronizedCommandsToRecipient()
        {
            // FIXME: This sorting might not be flawless. And stable-sorting migth not be garanteed by List.
            synchronizedCommands.Sort((a, b) => a.SourceFaction.Name.CompareTo(b.SourceFaction.Name));
            foreach (Command command in synchronizedCommands)
                Recipient.Feed(command);
            synchronizedCommands.Clear();
        }

        private void BeginSynchronizationOfAccumulatedCommands()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write((byte)GameMessageType.Commands);
                    foreach (Command accumulatedCommand in accumulatedCommands)
                    {
                        if (accumulatedCommand.EntitiesInvolved.Intersect(deadEntities).Any()) continue;
                        serializer.Serialize(accumulatedCommand, writer);
                    }
                }
                deadEntities.Clear();
                transporter.SendTo(stream.ToArray(), peers);
            }

            synchronizedCommands.AddRange(accumulatedCommands);
            accumulatedCommands.Clear();
        }
        #endregion

        #region Private
        private void WaitForPeerCommands()
        {
            if (!ReadyToContinue)
            {
                transporter.Poll();
                while (!ReadyToContinue)
                {
                    Thread.Sleep(0);
                    transporter.Poll();
                }
            }
        }

        private void ResetPeerStatuses()
        {
            foreach (IPEndPoint peer in peers)
            {
                receivedFromPeers[peer] = false;
                peersCompleted[peer] = false;
            }
        }

        private void TransporterReceived(Transporter source, NetworkEventArgs args)
        {
            byte messageType = args.Data[0];
            if (messageType == (byte)GameMessageType.Commands)
            {
                Deserialize(args.Data.Skip(1).ToArray());
                receivedFromPeers[args.Host] = true;
                if (ReceivedFromAllPeers)
                {
                    transporter.SendTo(doneMessage, peers);
                }
            }
            else if (messageType == (byte)GameMessageType.Done)
            {
                peersCompleted[args.Host] = true;
            }
        }

        private void TransporterTimedOut(Transporter source, NetworkTimeoutEventArgs args)
        {
            Console.WriteLine("*** Lost connection to {0}", args.Host);
            peers.Remove(args.Host);
            peersCompleted.Remove(args.Host);
            receivedFromPeers.Remove(args.Host);
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