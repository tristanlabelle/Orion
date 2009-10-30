
namespace Orion.Commandment
{
    // just be a regular sink for now
    public class CommandOptimizer : CommandSink
    {
        public CommandOptimizer()
        { }

        public CommandOptimizer(ICommandSink recipient)
            : base(recipient)
        { }
    }
}
