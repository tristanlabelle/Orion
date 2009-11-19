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
        private readonly CommandFactory commandFactory;
        #endregion

        #region Constructors
        public CommandReplayLogger(Stream stream, World world)
        {
            Argument.EnsureNotNull(stream, "stream");
            Argument.EnsureNotNull(world, "world");
            writer = new BinaryWriter(stream);
            commandFactory = new CommandFactory(world);
        }

        public CommandReplayLogger(string path, World world)
            : this(File.OpenWrite(path), world) { }
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
                commandFactory.Serialize(command, writer);
                writer.Flush();
                Flush(command);
            }
        }
        #endregion
    }
}
