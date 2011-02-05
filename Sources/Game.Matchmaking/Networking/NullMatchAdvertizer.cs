using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Game.Matchmaking.Networking
{
    /// <summary>
    /// A null object match advertizer which ignores all advertising requests.
    /// </summary>
    public sealed class NullMatchAdvertizer : IMatchAdvertizer
    {
        /// <summary>
        /// A globally accessible instance of this advertizer.
        /// </summary>
        public static readonly NullMatchAdvertizer Instance = new NullMatchAdvertizer();

        public void Advertize(string name, int openSlotsCount) { }
        public void Delist(string name) { }
    }
}
