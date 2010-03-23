using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Orion.Game.Simulation;

namespace Orion.Game.Matchmaking.Commands
{
    public interface IMultipleExecutingEntitiesCommand
    {
        IMultipleExecutingEntitiesCommand CopyWithEntities(IEnumerable<Handle> entityHandles);
    }
}
