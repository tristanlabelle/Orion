using System;
using System.Collections.Generic;

namespace Orion.Commandment
{
    public abstract class CommandSink : ISinkRecipient
    {
		#region Fields
        private ISinkRecipient recipient;

        protected Queue<Command> commands = new Queue<Command>();
		#endregion

		#region Constructors
        public CommandSink()
        { }

        public CommandSink(ISinkRecipient recipient)
        {
            this.recipient = recipient;
        }
		#endregion

		#region Properties
        public ISinkRecipient Recipient
        {
            get { return recipient; }
            set { recipient = value; }
        }
		#endregion

		#region Methods
        public virtual void BeginFeed()
        { }

        public virtual void Feed(Command command)
        {
            Argument.EnsureNotNull(command, "command");
            commands.Enqueue(command);
        }

        public virtual void EndFeed()
        {
            Flush();
        }

        public virtual void Flush()
        {
            if (recipient == null) throw new NullReferenceException("Sink's recipient must not be null when Flush() is called");

            recipient.BeginFeed();
            while (commands.Count > 0)
            {
                recipient.Feed(commands.Dequeue());
            }
            recipient.EndFeed();
        }
		#endregion
    }
}
