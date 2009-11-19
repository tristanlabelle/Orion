using System.IO;
using System.Diagnostics;
using System;
using System.Collections.Generic;

namespace Orion.Commandment.Pipeline
{
    /// <summary>
    /// A command filter which logs to a text file the commands which pass through it.
    /// </summary>
    public sealed class CommandTextLogger : CommandFilter
    {
        #region Fields
        private readonly Queue<Command> commandQueue = new Queue<Command>();
        private readonly TextWriter writer = GetTextWriter();
        private int commandNumber = 0;
        #endregion

        #region Methods
        public override void Handle(Command command)
        {
            Argument.EnsureNotNull(command, "command");
            commandQueue.Enqueue(command);
        }

        public override void Update(int updateNumber, float timeDeltaInSeconds)
        {
            while (commandQueue.Count > 0)
            {
                Command command = commandQueue.Dequeue();
                writer.WriteLine("Command #{0}, Frame #{1}: {2}", commandNumber, updateNumber, command.ToString());
                ++commandNumber;
                writer.Flush();
                Flush(command);
            }
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
                return OpenFileStream("Command Log.txt");
            }
            catch (IOException)
            {
                for (int i = 2; i < 10; ++i)
                {
                    string fileName = "Command Log {0}.txt".FormatInvariant(i);
                    try { return OpenFileStream(fileName); }
                    catch (IOException) { }
                }

                return null;
            }
        }

        private static Stream OpenFileStream(string path)
        {
            return new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
        }
        #endregion
    }
}
