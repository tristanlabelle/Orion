using System;


using Orion.GameLogic;
using Orion.Geometry;
using Color = System.Drawing.Color;

namespace Orion.Graphics
{
    /// <summary>
    /// Provides methods to draw a <see cref="Terrain"/> on-screen.
    /// </summary>
    public sealed class TerrainRenderer : IDisposable
    {
        #region Fields
        public static readonly Color WalkableColor = Color.FromArgb(60, 50, 50);
        public static readonly Color SolidColor = Color.FromArgb(255, 248, 233);

        private readonly Terrain terrain;
        private readonly Texture texture;
        #endregion

        #region Constructors
        public TerrainRenderer(Terrain terrain)
        {
            Argument.EnsureNotNull(terrain, "terrain");

            this.terrain = terrain;

            byte[] pixels = new byte[terrain.Width * terrain.Height * 3];
            for (int y = 0; y < terrain.Height; ++y)
            {
                for (int x = 0; x < terrain.Width; ++x)
                {
                    int pixelIndex = y * terrain.Width + x;
                    if (terrain.IsWalkable(x, y))
                    {
                        pixels[pixelIndex * 3 + 0] = WalkableColor.R;
                        pixels[pixelIndex * 3 + 1] = WalkableColor.G;
                        pixels[pixelIndex * 3 + 2] = WalkableColor.B;
                    }
                    else
                    {
                        pixels[pixelIndex * 3 + 0] = SolidColor.R;
                        pixels[pixelIndex * 3 + 1] = SolidColor.G;
                        pixels[pixelIndex * 3 + 2] = SolidColor.B;
                    }
                }
            }

            texture = new TextureBuilder
            {
                Width = terrain.Width,
                Height = terrain.Height,
                Format = TextureFormat.Rgb,
                PixelData = pixels
            }.Build();
        }
        #endregion

        #region Methods
        public void Draw(GraphicsContext graphics)
        {
            Rectangle terrainBounds = new Rectangle(0, 0, terrain.Width, terrain.Height);
            graphics.Fill(terrainBounds, texture);
        }

        public void Dispose()
        {
            texture.Dispose();
        }
        #endregion
    }
}
