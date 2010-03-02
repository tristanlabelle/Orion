using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Matchmaking.TowerDefense;
using Orion.Engine.Graphics;
using Orion.Collections;
using Orion.Geometry;

namespace Orion.Graphics.Renderers
{
    /// <summary>
    /// Responsible for drawing the path that is followed by the creeps.
    /// </summary>
    public sealed class CreepPathRenderer : IRenderer
    {
        #region Fields
        private static readonly float TextureSizeInTiles = 6;

        private readonly CreepPath path;
        private readonly Texture maskTexture;
        private readonly Texture pathTexture;
        #endregion

        #region Constructors
        public CreepPathRenderer(CreepPath path, TextureManager textureManager)
        {
            Argument.EnsureNotNull(path, "path");
            Argument.EnsureNotNull(textureManager, "textureManager");

            this.path = path;

            BitArray2D bitmap = path.GenerateBitmap();

            int textureWidth = PowerOfTwo.Ceiling(Math.Max(bitmap.Width, bitmap.Height));
            Size textureSize = new Size(textureWidth, textureWidth);

            byte[] pixelData = new byte[textureSize.Area];
            for (int y = 0; y < bitmap.Height; ++y)
            {
                for (int x = 0; x < bitmap.Width; ++x)
                {
                    int pixelIndex = x + y * textureSize.Width;
                    if (bitmap[x, y]) pixelData[pixelIndex] = 255;
                }
            }

            this.maskTexture = Texture.FromBuffer(textureSize, PixelFormat.Alpha, pixelData, true, false);
            this.pathTexture = textureManager.Get("Path");
            this.pathTexture.SetSmooth(true);
            this.pathTexture.SetRepeat(true);
        }
        #endregion

        #region Properties
        private Rectangle UnitTextureRectangle
        {
            get
            {
                return new Rectangle(
                    path.TerrainSize.Width / (float)maskTexture.Width,
                    path.TerrainSize.Height / (float)maskTexture.Height);
            }
        }

        private Rectangle PathTextureRectangle
        {
            get
            {
                return new Rectangle(
                    path.TerrainSize.Width / TextureSizeInTiles,
                    path.TerrainSize.Height / TextureSizeInTiles);
            }
        }
        #endregion

        #region Methods
        public void Draw(GraphicsContext context, Rectangle viewBounds)
        {
            Argument.EnsureNotNull(context, "context");
            
            Rectangle terrainBounds = new Rectangle(path.TerrainSize.Width, path.TerrainSize.Height);
            context.FillMasked(terrainBounds, pathTexture, PathTextureRectangle, maskTexture, UnitTextureRectangle);
        }
        #endregion
    }
}
