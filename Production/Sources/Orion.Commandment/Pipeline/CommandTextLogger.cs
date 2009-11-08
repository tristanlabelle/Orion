using System.IO;
using System.Diagnostics;

namespace Orion.Commandment.Pipeline
{
    /// <summary>
    /// A command filter which logs to a text file the commands which pass through it.
    /// </summary>
    public sealed class CommandTextLogger : CommandFilter
    {
        #region Fields
        private readonly TextWriter writer = new StreamWriter("Command Log.txt");
        private int commandIndex = 0;
        private int frameIndex = 0;
        #endregion

        #region Constructors
        public CommandTextLogger() { }

        public CommandTextLogger(ICommandSink recipient)
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
