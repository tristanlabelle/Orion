using System;
using Orion.Engine.Graphics;
using Orion.GameLogic;
using Orion.Geometry;
using GraphicsContext = Orion.Engine.Graphics.GraphicsContext;
using PixelFormat = Orion.Engine.Graphics.PixelFormat;

namespace Orion.Graphics
{
    /// <summary>
    /// Provides methods to draw a <see cref="Terrain"/> on-screen.
    /// </summary>
    public sealed class TerrainRenderer : IDisposable
    {
        #region Fields
        private static readonly float GrassTextureSizeInTiles = 4;
        private static readonly float SandTextureSizeInTiles = 12;

        private readonly Terrain terrain;
        private readonly Texture obstacleMaskTexture;
        private readonly Texture splattingMaskTexture;
        private readonly Texture grassTileTexture;
        private readonly Texture sandTileTexture;
        private readonly Texture obstacleTileTexture;
        #endregion

        #region Constructors
        public TerrainRenderer(Terrain terrain, GameGraphics gameGraphics)
        {
            Argument.EnsureNotNull(terrain, "terrain");
            Argument.EnsureNotNull(gameGraphics, "gameGraphics");

            this.terrain = terrain;
            this.obstacleMaskTexture = CreateObstacleMaskTexture(terrain);
            this.splattingMaskTexture = CreateSplattingMaskTexture(terrain.Size);
            this.grassTileTexture = gameGraphics.GetMiscTexture("Grass");
            this.grassTileTexture.SetSmooth(true);
            this.grassTileTexture.SetRepeat(true);
            this.sandTileTexture = gameGraphics.GetMiscTexture("Sand");
            this.sandTileTexture.SetSmooth(true);
            this.sandTileTexture.SetRepeat(true);
            this.obstacleTileTexture = gameGraphics.GetMiscTexture("Obstacle");
            this.obstacleTileTexture.SetRepeat(true);
        }
        #endregion

        #region Properties
        private Rectangle UnitTextureRectangle
        {
            get
            {
                return new Rectangle(0, 0,
                    terrain.Width / (float)obstacleMaskTexture.Width,
                    terrain.Height / (float)obstacleMaskTexture.Height);
            }
        }
        #endregion

        #region Methods
        public void Draw(GraphicsContext graphics)
        {
            Rectangle terrainBounds = new Rectangle(terrain.Width, terrain.Height);
            Rectangle grassTextureRectangle = new Rectangle(terrain.Width / GrassTextureSizeInTiles, terrain.Height / GrassTextureSizeInTiles);
            Rectangle sandTextureRectangle = new Rectangle(terrain.Width / SandTextureSizeInTiles, terrain.Height / SandTextureSizeInTiles);

            graphics.Fill(terrainBounds, grassTileTexture, grassTextureRectangle);
            graphics.FillMasked(terrainBounds, sandTileTexture, sandTextureRectangle, splattingMaskTexture, UnitTextureRectangle);
            graphics.FillMasked(terrainBounds, obstacleTileTexture, grassTextureRectangle, obstacleMaskTexture, UnitTextureRectangle);
        }

        public void DrawMiniature(GraphicsContext graphics)
        {
            Rectangle terrainBounds = new Rectangle(0, 0, terrain.Width, terrain.Height);

            graphics.FillColor = ColorRgb.FromBytes(232, 207, 144);
            graphics.Fill(terrainBounds);
            graphics.Fill(terrainBounds, obstacleMaskTexture, UnitTextureRectangle, ColorRgb.FromBytes(100, 78, 60));
        }

        public void Dispose()
        {
            obstacleMaskTexture.Dispose();
        }

        private static Texture CreateObstacleMaskTexture(Terrain terrain)
        {
            int textureWidth = Math.Max(PowerOfTwo.Ceiling(terrain.Size.Width), PowerOfTwo.Ceiling(terrain.Size.Height));
            Size textureSize = new Size(textureWidth, textureWidth);

            byte[] pixels = new byte[textureSize.Area];
            for (int y = 0; y < terrain.Height; ++y)
            {
                for (int x = 0; x < terrain.Width; ++x)
                {
                    int pixelIndex = y * textureSize.Width + x;
                    Point point = new Point(x, y);
                    pixels[pixelIndex] = terrain.IsWalkable(point) ? (byte)0 : (byte)255;
                }
            }

            return Texture.FromBuffer(textureSize, PixelFormat.Alpha, pixels, true, false);
        }

        private static Texture CreateSplattingMaskTexture(Size size)
        {
            int textureWidth = Math.Max(PowerOfTwo.Ceiling(size.Width), PowerOfTwo.Ceiling(size.Height));
            Size textureSize = new Size(textureWidth, textureWidth);

            PerlinNoise noise = new PerlinNoise();
            noise.Density = 3;
            noise.Frequency = 0.01f;
            byte[] pixels = new byte[textureSize.Area];
            for (int y = 0; y < textureSize.Height; ++y)
            {
                for (int x = 0; x < textureSize.Width; ++x)
                {
                    int pixelIndex = y * textureSize.Width + x;

                    double value = noise[x, y];
                    if (value < 0) value = 0;
                    else if (value > 1) value = 1;

                    pixels[pixelIndex] = (byte)(value * 255);
                }
            }

            return Texture.FromBuffer(textureSize, PixelFormat.Alpha, pixels, true, false);
        }
        #endregion
    }
}
