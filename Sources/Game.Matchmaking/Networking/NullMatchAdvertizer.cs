using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Game.Matchmaking.Networking
{
    public class NullMatchAdvertizer : IMatchAdvertizer
    {
        public void Advertize(string name, int openSlotsCount) { }
    }
}
