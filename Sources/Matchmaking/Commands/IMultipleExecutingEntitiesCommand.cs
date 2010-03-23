using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Orion.Game.Simulation;

namespace Orion.Matchmaking.Commands
{
    public interface IMultipleExecutingEntitiesCommand
    {
        IMultipleExecutingEntitiesCommand CopyWithEntities(IEnumerable<Handle> entityHandles);
    }
}
