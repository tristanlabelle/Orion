using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Threading;
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
		#region Fields
		#region Static
        private static int frameModulo = 6;
		private static readonly byte[] doneMessage = {(byte)GameMessageType.Done};
		#endregion
		
		private uint frameCounter;
		
		private GenericEventHandler<Transporter, NetworkEventArgs> transporterReceived;
		private GenericEventHandler<Transporter, NetworkTimeoutEventArgs> transporterTimeout;
        private Transporter transporter;
		
		private List<IPEndPoint> peers;
        private Dictionary<IPEndPoint, bool> receivedFromPeers = new Dictionary<IPEndPoint, bool>();
		private Dictionary<IPEndPoint, bool> peersCompleted = new Dictionary<IPEndPoint, bool>();
		
		private Queue<Command> readyCommands = new Queue<Command>();
		
		private CommandFactory serializer;
		#endregion
		
		#region Constructors
		
        public CommandSynchronizer(World world, Transporter transporter, IEnumerable<IPEndPoint> endpoints)
        {
            this.transporter = transporter;
			peers = new List<IPEndPoint>(endpoints);
			serializer = new CommandFactory(world);

            foreach (IPEndPoint endpoint in peers)
            {
				receivedFromPeers[endpoint] = true;
                peersCompleted[endpoint] = true;
            }
			
			transporterReceived = new GenericEventHandler<Transporter, NetworkEventArgs>(TransporterReceived);
			transporterTimeout = new GenericEventHandler<Transporter, NetworkTimeoutEventArgs>(TransporterTimedOut);
			transporter.Received += transporterReceived;
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
			get { return receivedFromPeers.Select(pair => pair.Value).Where(i => i == false).Count() == 0; }
		}
		
		private bool AllPeersReady
		{
			get { return peersCompleted.Select(pair => pair.Value).Where(i => i == false).Count() == 0; }
		}
		
		#endregion
		
		#region Methods
		
		#region Public
		
		public void Update()
		{
			frameCounter++;
			
			if(frameCounter % frameModulo == 0)
			{
				while(!ReadyToContinue)
				{
					// no choice but to wait
					Thread.Sleep(10);
					transporter.Poll();
				}
				
                Flush();

                foreach (IPEndPoint peer in peers)
                {
                    receivedFromPeers[peer] = false;
                    peersCompleted[peer] = false;
                }
			}
		}

        public override void EndFeed()
        { }

        public void Dispose()
        {
			transporter.Received -= transporterReceived;
        }
		
		public override void Flush()
		{
            if (Recipient == null) throw new NullReferenceException("Sink's recipient must not be null when Flush() is called");
			
			Recipient.BeginFeed();

            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write((byte)GameMessageType.Commands);
                    while (commands.Count > 0)
                    {
                        Command command = commands.Dequeue();
                        Recipient.Feed(command);
                        serializer.Serialize(command, writer);
                    }
                }
                transporter.SendTo(stream.ToArray(), peers);
            }
			
			while(readyCommands.Count > 0)
			{
				Recipient.Feed(readyCommands.Dequeue());
			}
			
			Recipient.EndFeed();
		}
		
		#endregion
		
		#region Private
		
		private void TransporterReceived(Transporter source, NetworkEventArgs args)
		{
			byte messageType = args.Data[0];
			if(messageType == (byte)GameMessageType.Commands)
			{
				Unserialize(args.Data.Skip(1).ToArray());
				receivedFromPeers[args.Host] = true;
				if(ReceivedFromAllPeers)
				{
					transporter.SendTo(doneMessage, peers);
				}
			}
			else if(messageType == (byte)GameMessageType.Done)
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
		
		private void Unserialize(byte[] array)
		{
			using(MemoryStream stream = new MemoryStream(array))
			{
				using(BinaryReader reader = new BinaryReader(stream))
				{
                    while (stream.Position != stream.Length)
                    {
                        readyCommands.Enqueue(serializer.Unserialize(reader));
                    }
				}
			}
		}
		
		#endregion
		#endregion
    }
}