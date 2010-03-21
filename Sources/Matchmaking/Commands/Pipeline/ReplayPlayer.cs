﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using Orion.Engine;

namespace Orion.Matchmaking.Commands.Pipeline
{
    /// <summary>
    /// A command filter which injects commands read from a replay file.
    /// </summary>
    public sealed class ReplayPlayer : CommandFilter
    {
        #region Fields
        private readonly ReplayReader reader;
        private readonly Queue<Command> accumulatedCommands = new Queue<Command>();
        private ReplayEvent? nextReplayCommand;
        #endregion

        #region Constructors
        public ReplayPlayer(ReplayReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");
            this.reader = reader;

            if (!reader.IsEndOfStreamReached)
                nextReplayCommand = reader.ReadCommand();
        }
        #endregion

        #region Methods
        public override void Handle(Command command)
        {
            Argument.EnsureNotNull(command, "command");
            accumulatedCommands.Enqueue(command);
        }

        public override void Update(int updateNumber, float timeDeltaInSeconds)
        {
            while (accumulatedCommands.Count > 0)
            {
                Command command = accumulatedCommands.Dequeue();
                Flush(command);
            }

            while (nextReplayCommand != null && nextReplayCommand.Value.UpdateNumber <= updateNumber)
            {
                Debug.Assert(nextReplayCommand.Value.UpdateNumber == updateNumber);
                Flush(nextReplayCommand.Value.Command);
                if (reader.IsEndOfStreamReached)
                {
                    nextReplayCommand = null;
                    break;
                }

                nextReplayCommand = reader.ReadCommand();
            }
        }
        #endregion
    }
}
