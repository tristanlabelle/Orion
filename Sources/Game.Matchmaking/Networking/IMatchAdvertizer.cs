using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Game.Matchmaking.Networking
{
    public interface IMatchAdvertizer
    {
        void Advertize(string name, int openSlotsCount);
        void Delist(string name);
    }
}
