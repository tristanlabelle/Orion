using System;
using System.IO;
using System.Net;
using System.Collections.Generic;

using Orion.Commandment;
using Orion.Commandment.Commands;

namespace Orion.Networking
{
	public class CommandSynchronizer : CommandSink, IDisposable
	{
		private ISinkRecipient recipient;
		private Queue<Command> accumulatedCommands = new Queue<Command>();
		
		private GenericEventHandler<Transporter, NetworkEventArgs> receptionDelegate;
		private Transporter transporter;
		private List<IPEndPoint> peers;
		
		public CommandSynchronizer(Transporter transporter, List<IPEndPoint> peers)
		{
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
			// receive commands ... put them into accumulatedCommands...
			// receive done ...
			
			// release commands to the next sink
			if(recipient == null) throw new NullReferenceException("The recipient of a CommandSynchronizer must not be null");
			
			recipient.BeginFeed();
			while(accumulatedCommands.Count > 0)
			{
				recipient.Feed(accumulatedCommands.Dequeue());
			}
			recipient.EndFeed();
		}
		
		public override void BeginFeed ()
		{
			accumulatedCommands.Clear();
			base.BeginFeed();
		}
		
		public override void Flush()
		{
			if(recipient == null) throw new NullReferenceException("Sink's recipient must not be null when Flush() is called");
			
			using(MemoryStream stream = new MemoryStream())
			{
				using(BinaryWriter writer = new BinaryWriter(stream))
				{
					while(commands.Count > 0)
					{
						Command command = commands.Dequeue();
						CommandSerializer serializer = command.GetType().GetNestedType("Serializer").GetField("Instance").GetValue(null) as CommandSerializer;
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
