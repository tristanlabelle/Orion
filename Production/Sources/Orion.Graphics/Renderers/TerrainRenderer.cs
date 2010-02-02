using System;


using Orion.GameLogic;
using Orion.Geometry;
using Color = System.Drawing.Color;
using OpenTK.Graphics;

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
        private readonly Texture groundTileTexture;
        private readonly Texture obstacleTileTexture;
        #endregion

        #region Constructors
        public TerrainRenderer(Terrain terrain, TextureManager textureManager)
        {
            Argument.EnsureNotNull(terrain, "terrain");
            Argument.EnsureNotNull(textureManager, "textureManager");

            this.terrain = terrain;
            this.texture = CreateTerrainTexture(terrain);
            this.groundTileTexture = textureManager.Get("Ground");
            this.groundTileTexture.SetRepeat(true);
            this.obstacleTileTexture = textureManager.Get("Obstacle");
            this.obstacleTileTexture.SetRepeat(true);
        }
        #endregion

        #region Methods
        public void Draw(GraphicsContext graphics)
        {
            Rectangle terrainBounds = new Rectangle(0, 0, terrain.Width, terrain.Height);

            GL.ColorMask(false, false, false, true);
            graphics.Fill(terrainBounds, texture);

            Rectangle textureRectangle = new Rectangle(0, 0, terrain.Width / 4, terrain.Height / 4);

            GL.ColorMask(true, true, true, false);
            GL.Color4(1f, 1f, 1f, 1f);
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.OneMinusDstAlpha, BlendingFactorDest.One);
            groundTileTexture.BindWhile(() => graphics.DrawTexturedQuad(terrainBounds, textureRectangle));
            GL.BlendFunc(BlendingFactorSrc.DstAlpha, BlendingFactorDest.One);
            obstacleTileTexture.BindWhile(() => graphics.DrawTexturedQuad(terrainBounds, textureRectangle));
            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.Texture2D);
        }

        public void DrawMiniature(GraphicsContext graphics)
        {
            Rectangle terrainBounds = new Rectangle(0, 0, terrain.Width, terrain.Height);

            float textureRectangleHeight = terrain.Height / (float)texture.Height;
            Rectangle textureRectangle = new Rectangle(0, 1 - textureRectangleHeight,
                terrain.Width / (float)texture.Width,
                textureRectangleHeight);

            graphics.FillColor = Color.FromArgb(232, 207, 144);
            graphics.Fill(terrainBounds);
            graphics.Fill(terrainBounds, texture, Color.FromArgb(100, 78, 60));
        }

        public void Dispose()
        {
            texture.Dispose();
        }

        private static Texture CreateTerrainTexture(Terrain terrain)
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

            return Texture.FromBuffer(textureSize, PixelFormat.Alpha, pixels, false, false);
        }
        #endregion
    }
}
