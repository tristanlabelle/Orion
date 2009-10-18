﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion
{
    /// <summary>
    /// Defines a terrain to be drawn in background, parts of this terrain are walkable, others are not.
    /// </summary>
    public sealed class Terrain
    {
        #region Instance
        #region Fields
        private bool[,] tiles;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="Terrain"/> object. 
        /// </summary>
        /// <param name="width">The width of the terrain to be generated.</param>
        /// <param name="height">The height of the terrain to be generated.</param>
        public Terrain(int width, int height)
        {
            this.tiles = new bool[width, height];
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the width of this terrain, in tiles.
        /// </summary>
        public int Width
        {
            get { return tiles.GetLength(0); }
        }

        /// <summary>
        /// Gets the height of this terrain, in tiles.
        /// </summary>
        public int Height
        {
            get { return tiles.GetLength(1); }
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
            return !tiles[x, y];
        }
        #endregion
        #endregion

        #region Static
        #region Methods
        /// <summary>
        /// Returns a <see cref="Terrain"/> generated by a <see cref="PerlinNoise"/>. 
        /// </summary>
        /// <param name="terrainWidth">The width of the terrain to be generated.</param>
        /// <param name="terrainHeight">The height of the terrain to be generated.</param>
        /// <param name="random">The <see cref="Random"/> to be used to generate the terrain.</param>
        /// <returns>A newly generated <see cref="Terrain"/>.</returns>
        public static Terrain Generate(int terrainWidth, int terrainHeight, Random random)
        {
            PerlinNoise noise = new PerlinNoise(random);

            Terrain terrain = new Terrain(terrainWidth, terrainHeight);
            double[] rawTerrain = new double[terrainWidth * terrainHeight];
            for (int i = 0; i < terrainHeight; i++)
            {
                for (int j = 0; j < terrainWidth; j++)
                {
                    rawTerrain[i * terrainWidth + j] = noise[j, i];
                }
            }

            double max = rawTerrain.Max();
            int k = 0;
            foreach (double noiseValue in rawTerrain.Select(d => d / max))
            {
                terrain.tiles[k % terrainHeight, k / terrainHeight] = noiseValue >= 0.5;
                k++;
            }

            return terrain;
        }
        #endregion
        #endregion
    }
}
