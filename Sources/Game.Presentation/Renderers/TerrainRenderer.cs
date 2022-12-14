using System;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Engine.Graphics;
using Orion.Game.Simulation;
using GraphicsContext = Orion.Engine.Graphics.GraphicsContext;
using PixelFormat = Orion.Engine.Graphics.PixelFormat;

namespace Orion.Game.Presentation.Renderers
{
    /// <summary>
    /// Provides methods to draw a <see cref="Terrain"/> on-screen.
    /// </summary>
    public sealed class TerrainRenderer : IDisposable
    {
        #region Fields
        private static readonly float grassTextureSizeInTiles = 4;
        private static readonly float sandTextureSizeInTiles = 12;
        private static readonly float obstacleTextureSizeInTiles = 8;
        private static readonly ColorRgb grassColor = ColorRgb.FromBytes(57, 74, 29);
        private static readonly ColorRgb sandColor = ColorRgb.FromBytes(183, 128, 73);
        private static readonly ColorRgb lavaColor = ColorRgb.FromBytes(98, 11, 1);

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

        private Rectangle TerrainBounds
        {
            get { return new Rectangle(terrain.Width, terrain.Height); } 
        }

        private Rectangle GrassTextureRectangle
        {
            get
            {
                return new Rectangle(
                    terrain.Width / grassTextureSizeInTiles,
                    terrain.Height / grassTextureSizeInTiles);
            }
        }

        private Rectangle SandTextureRectangle
        {
            get
            {
                return new Rectangle(
                    terrain.Width / sandTextureSizeInTiles,
                    terrain.Height / sandTextureSizeInTiles);
            }
        }

        private Rectangle ObstacleTextureRectangle
        {
            get
            {
                return new Rectangle(
                    terrain.Width / obstacleTextureSizeInTiles,
                    terrain.Height / obstacleTextureSizeInTiles);
            }
        }
        #endregion

        #region Methods
        public void Draw(GraphicsContext graphics)
        {
            graphics.Fill(TerrainBounds, grassTileTexture, GrassTextureRectangle);
            graphics.FillMasked(TerrainBounds, sandTileTexture, SandTextureRectangle, splattingMaskTexture, UnitTextureRectangle);
            graphics.FillMasked(TerrainBounds, obstacleTileTexture, ObstacleTextureRectangle, obstacleMaskTexture, UnitTextureRectangle);
        }

        public void DrawMiniature(GraphicsContext graphics)
        {
            graphics.Fill(TerrainBounds, grassColor);
            graphics.Fill(TerrainBounds, splattingMaskTexture, UnitTextureRectangle, sandColor);
            graphics.Fill(TerrainBounds, obstacleMaskTexture, UnitTextureRectangle, lavaColor);
        }

        public void Dispose()
        {
            obstacleMaskTexture.Dispose();
            splattingMaskTexture.Dispose();
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
                    pixels[pixelIndex] = terrain[x, y] == TileType.Walkable ? (byte)0 : (byte)255;
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
