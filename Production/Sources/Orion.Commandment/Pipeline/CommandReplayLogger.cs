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
        private readonly BinaryWriter writer;
        private readonly CommandFactory commandFactory;
        private int commandFrameNumber = -1;

        public CommandReplayLogger(Stream stream, World world)
        {
            Argument.EnsureNotNull(stream, "stream");
            Argument.EnsureNotNull(world, "world");
            writer = new BinaryWriter(stream);
            commandFactory = new CommandFactory(world);
        }

        public CommandReplayLogger(string path, World world)
            : this(File.OpenWrite(path), world) { }

        public override void Flush()
        {
            ++commandFrameNumber;
            foreach (Command command in accumulatedCommands)
            {  
                writer.Write(commandFrameNumber);
                commandFactory.Serialize(command, writer);
                writer.Flush();
            }

            base.Flush();
        }
    }
}
