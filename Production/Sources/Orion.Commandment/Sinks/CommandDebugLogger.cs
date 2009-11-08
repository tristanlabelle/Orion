using System.IO;
using System.Diagnostics;

namespace Orion.Commandment
{
    public sealed class CommandDegugLogger : CommandSink
    {
        #region Fields
        private readonly TextWriter writer = new StreamWriter("command log.txt");
        private int commandIndex = 0;
        private int frameIndex = 0;
        #endregion

        #region Constructors
        public CommandDegugLogger() { }

        public CommandDegugLogger(ICommandSink recipient)
            : base(recipient)
        { }
        #endregion

        #region Methods
        public override void Feed(Command command)
        {
            writer.WriteLine("Command #{0}, Frame #{1}: {2}",
                commandIndex, frameIndex, command.ToString());
            ++commandIndex;
            writer.Flush();

            base.Feed(command);
        }

        public override void Flush()
        {
            ++frameIndex;
            base.Flush();
        }
        #endregion
    }
}
