using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Localization;

namespace Orion.Game.Simulation
{
    /// <summary>
    /// A terrain generator based on perlin noise.
    /// </summary>
    public sealed class PerlinNoiseTerrainGenerator : TerrainGenerator
    {
        public override string GetName(Localizer localizer)
        {
            return localizer.GetNoun("PerlinNoise");
        }

        public override void Generate(Terrain terrain, Random random)
        {
            PerlinNoise noise = new PerlinNoise(random);

            double[] rawTerrain = new double[terrain.Size.Area];
            for (int y = 0; y < terrain.Height; y++)
                for (int x = 0; x < terrain.Width; x++)
                    rawTerrain[y * terrain.Width + x] = noise[x, y];

            double max = rawTerrain.Max();
            int k = 0;
            foreach (double noiseValue in rawTerrain.Select(d => d / max))
            {
                terrain[k % terrain.Width, k / terrain.Width]
                    = noiseValue < 0.6 ? TileType.Walkable : TileType.Obstacle;
                k++;
            }
        }
    }
}
