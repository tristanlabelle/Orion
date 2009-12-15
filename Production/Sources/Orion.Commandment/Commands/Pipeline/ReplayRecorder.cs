using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Orion.GameLogic;

namespace Orion.Commandment.Commands.Pipeline
{
    /// <summary>
    /// A command filter which records commands passing trough it in a replay.
    /// </summary>
    public sealed class ReplayRecorder : CommandFilter
    {
        #region Fields
        private readonly Queue<Command> commandQueue = new Queue<Command>();
        private readonly ReplayWriter writer;
        #endregion

        #region Constructors
        public ReplayRecorder(ReplayWriter writer)
        {
            Argument.EnsureNotNull(writer, "writer");
            this.writer = writer;
        }
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
                writer.WriteCommand(updateNumber, command);
#if DEBUG
                Flush(command);
#endif
            }
        }
        #endregion
    }
}
