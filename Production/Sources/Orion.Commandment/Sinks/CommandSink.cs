using System;
using System.Collections.Generic;

namespace Orion.Commandment
{
    public abstract class CommandSink : ISinkRecipient
    {
        private ISinkRecipient recipient;

        protected Queue<Command> commands = new Queue<Command>();

        public CommandSink()
        { }

        public CommandSink(ISinkRecipient recipient)
        {
            this.recipient = recipient;
        }

        public ISinkRecipient Recipient
        {
            get { return recipient; }
            set { recipient = value; }
        }

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
    }
}
