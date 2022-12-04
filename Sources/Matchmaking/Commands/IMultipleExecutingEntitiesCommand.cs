using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Orion.GameLogic;

namespace Orion.Matchmaking.Commands
{
    public interface IMultipleExecutingEntitiesCommand
    {
        IMultipleExecutingEntitiesCommand CopyWithEntities(IEnumerable<Handle> entityHandles);
    }
}
