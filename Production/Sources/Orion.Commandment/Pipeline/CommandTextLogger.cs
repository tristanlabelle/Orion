using System.IO;
using System.Diagnostics;
using System;

namespace Orion.Commandment.Pipeline
{
    /// <summary>
    /// A command filter which logs to a text file the commands which pass through it.
    /// </summary>
    public sealed class CommandTextLogger : CommandFilter
    {
        #region Fields
        private readonly TextWriter writer = GetTextWriter();
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

        private static TextWriter GetTextWriter()
        {
            Stream stream = GetLoggingStream();
            if (stream == null) return Console.Out;
            return new StreamWriter(stream);
        }

        private static Stream GetLoggingStream()
        {
            // Attempt to open the default file. If not possible,
            // assume it's locked by another game process and try
            // filename variations.
            try
            {
                return File.OpenWrite("Command Log.txt");
            }
            catch (IOException)
            {
                for (int i = 2; i < 10; ++i)
                {
                    string fileName = "Command Log {0}.txt".FormatInvariant(i);
                    try { return File.OpenWrite(fileName); }
                    catch (IOException) { }
                }

                return null;
            }
        }
        #endregion
    }
}
