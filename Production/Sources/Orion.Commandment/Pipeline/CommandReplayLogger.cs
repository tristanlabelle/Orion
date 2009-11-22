using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Orion.GameLogic;

namespace Orion.Commandment.Pipeline
{
    public sealed class CommandReplayLogger : CommandFilter
    {
        #region Fields
        private readonly Queue<Command> commandQueue = new Queue<Command>();
        private readonly BinaryWriter writer;
        #endregion

        #region Constructors
        public CommandReplayLogger(Stream stream)
        {
            Argument.EnsureNotNull(stream, "stream");
            writer = new BinaryWriter(stream);
        }

        public CommandReplayLogger(string path)
            : this(File.OpenWrite(path)) { }
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
                writer.Write(updateNumber);
                command.Serialize(writer);
                writer.Flush();
                Flush(command);
            }
        }
        #endregion
    }
}
