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

        public override void Update(SimulationStep step)
        {
            while (commandQueue.Count > 0)
            {
                Command command = commandQueue.Dequeue();
                writer.WriteCommand(step.Number, command);
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
        public static ReplayRecorder TryCreate(MatchSettings settings, PlayerSettings playerSettings)
        {
            Argument.EnsureNotNull(settings, "settings");
            Argument.EnsureNotNull(playerSettings, "playerSettings");

            ReplayWriter replayWriter = ReplayWriter.TryCreate(settings, playerSettings);
            if (replayWriter == null) return null;

            return new ReplayRecorder(replayWriter);
        }
        #endregion
        #endregion
    }
}
