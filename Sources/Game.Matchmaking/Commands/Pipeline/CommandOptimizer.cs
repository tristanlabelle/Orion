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
        private readonly Stack<Command> commands = new Stack<Command>();
        private readonly List<Handle> concernedHandles = new List<Handle>();
        #endregion

        #region Methods
        public override void Handle(Command command)
        {
            commands.Push(command);
        }

        public override void Update(int updateNumber, float timeDeltaInSeconds)
        {
            concernedHandles.Clear();
            while (commands.Count > 0)
            {
                Command command = commands.Pop();
                IEnumerable<Handle> handles = command.ExecutingUnitHandles;
                IEnumerable<Handle> availableUnits = handles.Except(concernedHandles);

                if (handles.Count() > 0 && availableUnits.Count() > 0)
                {
                    IMultipleExecutingEntitiesCommand manyEntitiesCommand = command as IMultipleExecutingEntitiesCommand;
                    if (manyEntitiesCommand != null)
                    {
                        Command optimizedCommand = (Command)manyEntitiesCommand.CopyWithEntities(availableUnits);
                        FlushOptimized(optimizedCommand);
                        continue;
                    }
                }
                Flush(command);
            }
        }

        private void FlushOptimized(Command command)
        {
            concernedHandles.AddRange(command.ExecutingUnitHandles);
            Flush(command);
        }
        #endregion
    }
}
