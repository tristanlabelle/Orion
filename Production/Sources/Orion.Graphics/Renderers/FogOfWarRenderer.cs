
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

            pixelBuffer = new byte[fogOfWar.Width * fogOfWar.Height];
            UpdatePixelBuffer();

            texture = new TextureBuilder
            {
                Width = fogOfWar.Width,
                Height = fogOfWar.Height,
                Format = TextureFormat.Alpha,
                PixelData = pixelBuffer,
                UseSmoothing = true
            }.Build();
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
                Rectangle terrainBounds = new Rectangle(0, 0, fogOfWar.Width, fogOfWar.Height);
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

            for (int y = region.MinY; y < region.ExclusiveMaxY; ++y)
            {
                for (int x = region.MinX; x < region.ExclusiveMaxX; ++x)
                {
                    int pixelIndex = (y - region.MinY) * region.Width + (x - region.MinX);
                    TileVisibility visibility = fogOfWar.GetTileVisibility(x, y);
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
            Region region = new Region(0, 0, fogOfWar.Width, fogOfWar.Height);
            UpdatePixelBuffer(region);
        }

        private void UpdateTexture(Region area)
        {
            UpdatePixelBuffer(area);
            texture.SetPixels(area, pixelBuffer);
        }
        #endregion
    }
}
