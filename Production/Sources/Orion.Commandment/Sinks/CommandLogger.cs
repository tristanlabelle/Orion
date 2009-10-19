using System;

namespace Orion.Commandment
{
	// yet another currently pass-through sink
	public class CommandLogger : CommandSink
	{
		public CommandLogger ()
		{ }
		
		public CommandLogger(ISinkRecipient recipient)
			: base(recipient)
		{ }
	}
}
