using System;

namespace Orion.Commandment
{
    // this really is nothing more than a simple command sink
    public sealed class CommandAggregator : CommandSink
    {
        public CommandAggregator()
        { }

        public CommandAggregator(ICommandSink recipient)
            : base(recipient)
        { }
    }
}
