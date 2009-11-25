using System;
using Orion.GameLogic;
using Orion.Geometry;
using Color = System.Drawing.Color;

namespace Orion.Graphics
{
    public sealed class FogOfWarRenderer
    {
        #region Fields
        private const float FogTransparency = 0.5f;

        private readonly FogOfWar fogOfWar;
        private readonly Texture texture;
        private readonly byte[] pixelBuffer;
        private Region? dirtyRegion;
        #endregion

        #region Constructors
        public FogOfWarRenderer(FogOfWar fogOfWar)
        {
            Argument.EnsureNotNull(fogOfWar, "fogOfWar");

            this.fogOfWar = fogOfWar;
            this.fogOfWar.Changed += OnChanged;

            pixelBuffer = new byte[fogOfWar.Size.Area];
            UpdatePixelBuffer();

            texture = Texture.FromBuffer(fogOfWar.Size, PixelFormat.Alpha, pixelBuffer, true, false);
        }
        #endregion

        #region Properties
        public bool IsDirty
        {
            get { return dirtyRegion.HasValue; }
        }
        #endregion

        #region Methods
        public void Draw(GraphicsContext graphics)
        {
            if (IsDirty)
            {
                UpdateTexture(dirtyRegion.Value);
                dirtyRegion = null;
            }

            if (fogOfWar.IsEnabled)
            {
                Rectangle terrainBounds = new Rectangle(0, 0, fogOfWar.Size.Width, fogOfWar.Size.Height);
                graphics.Fill(terrainBounds, texture, Color.Black);
            }
        }

        private void OnChanged(FogOfWar fogOfWar, Region region)
        {
            if (dirtyRegion.HasValue)
                dirtyRegion = Region.Union(dirtyRegion.Value, region);
            else
                dirtyRegion = region;
        }

        private void UpdatePixelBuffer(Region region)
        {
            byte fogAlpha = (byte)(FogTransparency * 255.99f);

            for (int y = region.Min.Y; y < region.ExclusiveMax.Y; ++y)
            {
                for (int x = region.Min.X; x < region.ExclusiveMax.X; ++x)
                {
                    int pixelIndex = (y - region.Min.Y) * region.Size.Width + (x - region.Min.X);
                    TileVisibility visibility = fogOfWar.GetTileVisibility(new Point(x, y));
                    if (visibility == TileVisibility.Undiscovered)
                        pixelBuffer[pixelIndex] = 255;
                    else if (visibility == TileVisibility.Discovered)
                        pixelBuffer[pixelIndex] = fogAlpha;
                    else if (visibility == TileVisibility.Visible)
                        pixelBuffer[pixelIndex] = 0;
                }
            }
        }

        private void UpdatePixelBuffer()
        {
            Region region = (Region)fogOfWar.Size;
            UpdatePixelBuffer(region);
        }

        private void UpdateTexture(Region area)
        {
            UpdatePixelBuffer(area);
            texture.Blit(area, pixelBuffer);
        }
        #endregion
    }
}
