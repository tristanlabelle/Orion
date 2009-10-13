using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Core
{
    /// <summary>
    /// Defines a terrain to be drawn in background, parts of this terrain are walkable, others are not.
    /// </summary>
    public sealed class Terrain
    {

        #region Fields

        private bool[,] terrain;
        
        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="Terrain"/> object. 
        /// </summary>
        /// <param name="terrainWidth"> The width of the terrain to be generated.</param>
        /// <param name="terrainHeight"> The height of the terrain to be generated.</param>
        public Terrain(int TerrainWitdh, int TerrainHeight)
        {
            this.terrain = new bool[TerrainWitdh, TerrainHeight];
        }

        #endregion

        #region Properties

        #endregion

        #region Indexers

        /// <summary>
        /// Gets the boolean value of the terrain field at a given coordinate.
        /// </summary>
        /// <param name="x">The x coordinate in the field.</param>
        /// <param name="y">The y coordinate in the field.</param>
        /// <returns>A boolean value from the terrain field.</returns>
        public bool this[int x, int y]
        {
            get { return terrain[x, y]; }
            set { terrain[x, y] = value; ; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the boolean value of the terrain field at a given coordinate.
        /// </summary>
        /// <param name="x">The x coordinate in the field.</param>
        /// <param name="y">The y coordinate in the field.</param>
        /// <returns>A boolean value from the terrain field.</returns>
        public bool IsWalkable(int x, int y)
        {
            return terrain[x, y];
        }

        #endregion
    }
}
