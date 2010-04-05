using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Orion.Engine;
using Orion.Game.Simulation;

namespace Orion.Game.Matchmaking.Commands.Pipeline
{
    /// <summary>
    /// A command filter which records commands passing trough it in a replay.
    /// </summary>
    public sealed class ReplayRecorder : CommandFilter
    {
        #region Instance
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
                Flush(command);
            }
        }

        public override void Dispose()
        {
            writer.Dispose();
        }
        #endregion
        #endregion

        #region Static
        #region Methods
        public static ReplayRecorder TryCreate(MatchSettings settings, World world)
        {
            Argument.EnsureNotNull(settings, "settings");
            Argument.EnsureNotNull(world, "world");

            ReplayWriter replayWriter = ReplayWriter.TryCreate();
            if (replayWriter == null) return null;

#if DEBUG
            replayWriter.AutoFlush = true;
#endif

            replayWriter.WriteHeader(settings, world.Factions.Select(faction => faction.Name));

            return new ReplayRecorder(replayWriter);
        }
        #endregion
        #endregion
    }
}
