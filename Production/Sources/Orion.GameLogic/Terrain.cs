using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Orion.GameLogic
{
    /// <summary>
    /// Represents the static terrain grid of solid and walkable tiles.
    /// </summary>
    [Serializable]
    public sealed class Terrain
    {
        #region Fields
        private readonly BitArray grid;
        private readonly int width;
        #endregion

        #region Constructors
        #endregion

        #region Properties
        /// <summary>
        /// Gets the width of this <see cref="Terrain"/>, in tiles.
        /// </summary>
        public int Width
        {
            get { return width; }
        }

        /// <summary>
        /// Gets the height of this <see cref="Terrain"/>, in tiles.
        /// </summary>
        public int Height
        {
            get { return grid.Count / width; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Tests if the tile at a given position on this terrain is walkable.
        /// </summary>
        /// <param name="x">The x coordinate of the tile.</param>
        /// <param name="y">The y coordinate of the tile.</param>
        /// <returns>True if the tile is walkable, false otherwise.</returns>
        public bool IsWalkable(int x, int y)
        {
            return grid[x + y * width];
        }
        #endregion
    }
}
