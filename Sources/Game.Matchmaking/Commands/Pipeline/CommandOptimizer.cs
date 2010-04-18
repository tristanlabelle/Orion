using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Game.Simulation;
using Orion.Engine;

namespace Orion.Game.Matchmaking.Commands.Pipeline
{
    /// <summary>
    /// Removes duplicate commands on the same units to minimize bandwidth usage
    /// when Mathieu clicks like a madman to move his units.
    /// </summary>
    public sealed class CommandOptimizer : CommandFilter
    {
        #region Fields
        private readonly List<Command> commands = new List<Command>();
        private readonly HashSet<Handle> concernedHandles = new HashSet<Handle>();
        #endregion

        #region Methods
        public override void Handle(Command command)
        {
            commands.Add(command);
        }

        public override void Update(SimulationStep step)
        {
            if (commands.Count == 0) return;

            RemoveSameUnitCommands();
            AggregateTrainCommands();

            foreach (Command command in commands)
                Flush(command);
            commands.Clear();
        }

        /// <summary>
        /// Removes commands concerning units which are assigned another command right after.
        /// </summary>
        private void RemoveSameUnitCommands()
        {
            for (int i = 0; i < commands.Count - 1;)
            {
                Command command = commands[i];
                if (command.IsMandatory)
                {
                    ++i;
                    continue;
                }

                concernedHandles.Clear();
                concernedHandles.UnionWith(command.ExecutingUnitHandles);

                Command nextCommand = commands[i + 1];
                if (nextCommand.IsMandatory)
                {
                    i += 2;
                    continue;
                }

                bool isOptimizeable = command.FactionHandle == nextCommand.FactionHandle
                    && concernedHandles.SetEquals(nextCommand.ExecutingUnitHandles);
                if (!isOptimizeable)
                {
                    ++i;
                    continue;
                }

                // Remove the current command as the next one concerns the same units.
                commands.RemoveAt(i);
            }
        }

        /// <summary>
        /// Merges successive train commands involving the same trainers and the
        /// same trainee type to single commands with a greater trainee count.
        /// </summary>
        private void AggregateTrainCommands()
        {
            for (int i = 0; i < commands.Count - 1; ++i)
            {
                TrainCommand trainCommand = commands[i] as TrainCommand;
                if (trainCommand == null) continue;

                concernedHandles.Clear();
                concernedHandles.UnionWith(trainCommand.ExecutingUnitHandles);

                int aggregatedTraineeCount = trainCommand.TraineeCount;
                do
                {
                    TrainCommand nextTrainCommand = commands[i + 1] as TrainCommand;
                    if (nextTrainCommand == null) break;

                    bool isAggregateable = trainCommand.TraineeTypeHandle == nextTrainCommand.TraineeTypeHandle
                        && trainCommand.FactionHandle == nextTrainCommand.FactionHandle
                        && concernedHandles.SetEquals(nextTrainCommand.ExecutingUnitHandles);
                    if (!isAggregateable) break;

                    aggregatedTraineeCount += nextTrainCommand.TraineeCount;
                    commands.RemoveAt(i + 1);
                } while (i < commands.Count - 1);

                if (aggregatedTraineeCount > trainCommand.TraineeCount)
                {
                    commands[i] = new TrainCommand(trainCommand.FactionHandle, concernedHandles,
                        trainCommand.TraineeTypeHandle, aggregatedTraineeCount);
                }
            }
        }
        #endregion
    }
}
