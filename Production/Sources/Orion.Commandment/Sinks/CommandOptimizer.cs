
namespace Orion.Commandment
{
    // just be a regular sink for now
    public class CommandOptimizer : CommandFilter
    {
        public CommandOptimizer()
        { }

        public CommandOptimizer(ICommandSink recipient)
            : base(recipient)
        { }
    }
}
