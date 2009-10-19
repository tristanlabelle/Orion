using System;
using System.IO;
using System.Net;
using System.Collections.Generic;

using Orion.GameLogic;
using Orion.Commandment;
using Orion.Commandment.Commands;

namespace Orion.Networking
{
    public enum GameMessageType : byte
    {
        Commands,
        Done
    }

    public class CommandSynchronizer : CommandSink, IDisposable
    {
        private static readonly TimeSpan sendDelay = new TimeSpan(0, 0, 0, 0, 200);

        private Queue<Command> accumulatedCommands = new Queue<Command>();
        private CommandFactory serializer;

        private GenericEventHandler<Transporter, NetworkEventArgs> receptionDelegate;
        private Transporter transporter;
        private List<IPEndPoint> peers;
        private DateTime nextSend = DateTime.UtcNow;

        public CommandSynchronizer(World world, Transporter transporter, List<IPEndPoint> peers)
        {
            serializer = new CommandFactory(world);
            receptionDelegate = new GenericEventHandler<Transporter, NetworkEventArgs>(TransporterReceived);
            this.transporter = transporter;
            transporter.Received += receptionDelegate;
            this.peers = peers;
        }

        public CommandSynchronizer(ISinkRecipient recipient, Transporter transporter)
            : base(recipient)
        {
            receptionDelegate = new GenericEventHandler<Transporter, NetworkEventArgs>(TransporterReceived);
            this.transporter = transporter;
            transporter.Received += receptionDelegate;
        }

        private void TransporterReceived(Transporter source, NetworkEventArgs args)
        {
            if (args.Data[0] == (byte)GameMessageType.Commands)
            {
                using (MemoryStream stream = new MemoryStream(args.Data))
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        while (stream.Position != stream.Length)
                        {
                            accumulatedCommands.Enqueue(serializer.Deserialize(reader));
                        }
                    }
                }
            }
            else if (args.Data[0] == (byte)GameMessageType.Done)
            {

            }
            else
            {
                throw new NotImplementedException("The CommandSynchronizer does not expects a message of type {0}".FormatInvariant(args.Data[0]));
            }

            // release commands to the next sink
            if (Recipient == null) throw new NullReferenceException("The recipient of a CommandSynchronizer must not be null");

            Recipient.BeginFeed();
            while (accumulatedCommands.Count > 0)
            {
                Recipient.Feed(accumulatedCommands.Dequeue());
            }
            Recipient.EndFeed();
        }

        public override void BeginFeed()
        {
            accumulatedCommands.Clear();
            base.BeginFeed();
        }

        public override void EndFeed()
        {
            transporter.Poll();
            if (DateTime.UtcNow >= nextSend)
            {
                nextSend += sendDelay;
                Flush();
            }
        }

        public override void Flush()
        {
            if (Recipient == null) throw new NullReferenceException("Sink's recipient must not be null when Flush() is called");

            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    while (commands.Count > 0)
                    {
                        Command command = commands.Dequeue();

                        serializer.Serialize(command, writer);
                        accumulatedCommands.Enqueue(command);
                    }
                    transporter.SendTo(stream.ToArray(), peers);
                }
            }
        }

        public void Dispose()
        {
            transporter.Received -= receptionDelegate;
        }
    }
}