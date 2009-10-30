using System;


using Orion.GameLogic;
using Orion.Geometry;

namespace Orion.Graphics
{
    /// <summary>
    /// Provides methods to draw a <see cref="Terrain"/> on-screen.
    /// </summary>
    public sealed class TerrainRenderer : IDisposable
    {
        #region Fields
        private readonly Terrain terrain;
        private readonly Texture texture;
        #endregion

        #region Constructors
        public TerrainRenderer(Terrain terrain)
        {
            Argument.EnsureNotNull(terrain, "terrain");

            this.terrain = terrain;

            byte[] pixels = new byte[terrain.Width * terrain.Height];
            for (int y = 0; y < terrain.Height; ++y)
            {
                for (int x = 0; x < terrain.Width; ++x)
                {
                    int pixelIndex = y * terrain.Width + x;
                    byte luminance = terrain.IsWalkable(x, y) ? (byte)0 : (byte)255;
                    pixels[pixelIndex] = luminance;
                }
            }

            this.texture = new Texture(terrain.Width, terrain.Height, TextureFormat.Luminance, pixels);
        }
        #endregion

        #region Methods
        public void Draw(GraphicsContext graphics)
        {
            Rectangle terrainBounds = new Rectangle(0, 0, terrain.Width, terrain.Height);
            graphics.FillTextured(terrainBounds, texture);
        }

        public void Dispose()
        {
            texture.Dispose();
        }
        #endregion
    }
}
