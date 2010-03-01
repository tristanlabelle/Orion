using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.GameLogic
{
    /// <summary>
    /// Describes the type of a terrain tile.
    /// </summary>
    public enum TileType
    {
        /// <summary>
        /// Specifies a tile on which ground units can walk.
        /// </summary>
        Walkable,

        /// <summary>
        /// Specifies a tile on which ground units cannot walk.
        /// </summary>
        NonWalkable,

        /// <summary>
        /// Specifies a tile on which ground units can walk but on which buildings cannot be built.
        /// </summary>
        NonBuildable
    }
}
