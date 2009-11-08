using System;
using System.Collections.Generic;

namespace Orion.Commandment
{
    public abstract class CommandSink : ICommandSink
    {
        #region Fields
        private ICommandSink recipient;

        protected List<Command> accumulatedCommands = new List<Command>();
        #endregion

        #region Constructors
        public CommandSink()
        { }

        public CommandSink(ICommandSink recipient)
        {
            this.recipient = recipient;
        }
        #endregion

        #region Properties
        public ICommandSink Recipient
        {
            get { return recipient; }
            set { recipient = value; }
        }
        #endregion

        #region Methods
        public virtual void Feed(Command command)
        {
            Argument.EnsureNotNull(command, "command");
            accumulatedCommands.Add(command);
        }

        public virtual void EndFeed()
        {
            Flush();
        }

        public virtual void Flush()
        {
            if (recipient == null) throw new NullReferenceException("Sink's recipient must not be null when Flush() is called");

            foreach (Command accumulatedCommand in accumulatedCommands)
                recipient.Feed(accumulatedCommand);
            accumulatedCommands.Clear();

            recipient.EndFeed();
        }
        #endregion
    }
}