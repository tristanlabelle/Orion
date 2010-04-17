using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Game.Simulation;

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

        public override void Update(int updateNumber, float timeDeltaInSeconds)
        {
            RemoveSameUnitCommands();
            AggregateTrainCommands();

            foreach (Command command in commands)
                Flush(command);
            commands.Clear();
        }

        private void RemoveSameUnitCommands()
        {
            for (int i = commands.Count - 1; i > 0; --i)
            {
                Command command = commands[i];
                if (command.IsMandatory) continue;

                concernedHandles.Clear();
                concernedHandles.UnionWith(command.ExecutingUnitHandles);

                int optimizeableCount = 0;
                do
                {
                    Command previousCommand = commands[i - 1 - optimizeableCount];
                    if (previousCommand.IsMandatory) break;

                    bool isOptimizeable = command.FactionHandle == previousCommand.FactionHandle
                        && concernedHandles.SetEquals(previousCommand.ExecutingUnitHandles);
                    if (!isOptimizeable) break;

                    ++optimizeableCount;
                } while (i - optimizeableCount > 0);

                if (optimizeableCount > 0)
                {
                    commands.RemoveRange(i - optimizeableCount, optimizeableCount);
                }
            }
        }

        private void AggregateTrainCommands()
        {
            for (int i = 0; i < commands.Count - 1; ++i)
            {
                TrainCommand trainCommand = commands[i] as TrainCommand;
                if (trainCommand == null) continue;

                concernedHandles.Clear();
                concernedHandles.UnionWith(trainCommand.ExecutingUnitHandles);

                int aggregateableCount = 0;
                int aggregatedTraineeCount = trainCommand.TraineeCount;
                do
                {
                    TrainCommand nextTrainCommand = commands[i + 1] as TrainCommand;
                    if (nextTrainCommand == null) break;

                    bool isAggregateable = trainCommand.TraineeTypeHandle == nextTrainCommand.TraineeTypeHandle
                        && trainCommand.FactionHandle == nextTrainCommand.FactionHandle
                        && concernedHandles.SetEquals(nextTrainCommand.ExecutingUnitHandles);
                    if (!isAggregateable) break;

                    ++aggregateableCount;
                    aggregatedTraineeCount += nextTrainCommand.TraineeCount;
                } while (i < commands.Count - 1);

                if (aggregateableCount > 0)
                {
                    commands.RemoveRange(i + 1, aggregateableCount);
                    commands[i] = new TrainCommand(trainCommand.FactionHandle, concernedHandles,
                        trainCommand.TraineeTypeHandle, aggregatedTraineeCount);
                }
            }
        }
        #endregion
    }
}
