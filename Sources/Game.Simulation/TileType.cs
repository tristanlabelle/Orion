using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Game.Simulation
{
    /// <summary>
    /// Describes the type of a terrain tile.
    /// </summary>
    public enum TileType
    {
        /// <summary>
        /// Indicates that the tile can be walked upon.
        /// </summary>
        Walkable,

        /// <summary>
        /// Indicates that the tile is an obstacle to ground units.
        /// </summary>
        Obstacle
    }
}
