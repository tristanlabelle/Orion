using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.GameLogic
{
    /// <summary>
    /// Describes the visible state of a tile with respect to the fog of war.
    /// </summary>
    [Serializable]
    public enum TileVisibility
    {
        /// <summary>
        /// Specifies that a tile has never been seen by units of the faction.
        /// </summary>
        Undiscovered,

        /// <summary>
        /// Specifies that a tile has been seen by units of the faction,
        /// but is not currently within the line of sight of any of them
        /// </summary>
        Discovered,

        /// <summary>
        /// Specifies that a tile is within the line of sight of at least one unit in the faction.
        /// </summary>
        Visible
    }
}
