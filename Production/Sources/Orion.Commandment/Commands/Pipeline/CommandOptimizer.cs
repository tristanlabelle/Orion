﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.GameLogic;

namespace Orion.Commandment.Commands.Pipeline
{
    public class CommandOptimizer : CommandFilter
    {
        private Stack<Command> commands = new Stack<Command>();
        private List<Handle> concernedHandles = new List<Handle>();

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
                IEnumerable<Handle> handles = command.ExecutingEntityHandles;
                IEnumerable<Handle> availableUnits = handles.Except(concernedHandles);

                if (availableUnits.Count() > 0)
                {
                    IMultipleExecutingEntitiesCommand manyEntitiesCommand = command as IMultipleExecutingEntitiesCommand;
                    if (manyEntitiesCommand != null)
                    {
                        Command optimizedCommand = (Command)manyEntitiesCommand.CopyWithEntities(availableUnits);
                        FlushOptimized(optimizedCommand);
                    }
                    else Flush(command);
                }
            }
        }

        private void FlushOptimized(Command command)
        {
            concernedHandles.AddRange(command.ExecutingEntityHandles);
            Flush(command);
        }
    }
}