using System;
using System.Diagnostics;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Engine.Graphics;
using Orion.GameLogic;

namespace Orion.Graphics.Renderers
{
    public sealed class FogOfWarRenderer : IDisposable
    {
        #region Fields
        private const float fogTransparency = 0.5f;
        private static readonly ColorRgb fogColor = ColorRgb.CreateGray(0.1f);

        private readonly Faction faction;
        private readonly Texture texture;
        private readonly byte[] pixelBuffer;
        private Region? dirtyRegion;
        #endregion

        #region Constructors
        public FogOfWarRenderer(Faction faction)
        {
            Argument.EnsureNotNull(faction, "faction");

            this.faction = faction;
            this.faction.VisibilityChanged += OnVisibilityChanged;

            int textureWidth = PowerOfTwo.Ceiling(
                Math.Max(faction.World.Size.Width, faction.World.Size.Height));
            Size textureSize = new Size(textureWidth, textureWidth);

            this.pixelBuffer = new byte[textureSize.Area];
            for (int i = 0; i < pixelBuffer.Length; ++i)
                this.pixelBuffer[i] = 255;

            this.texture = Texture.FromBuffer(textureSize, PixelFormat.Alpha, pixelBuffer, true, false);
            UpdateTexture((Region)faction.World.Size);
        }
        #endregion

        #region Properties
        public bool IsDirty
        {
            get { return dirtyRegion.HasValue; }
        }

        private Rectangle TextureRectangle
        {
            get
            {
                return new Rectangle(0, 0,
                    faction.LocalFogOfWar.Size.Width / (float)texture.Width,
                    faction.LocalFogOfWar.Size.Height / (float)texture.Height);
            }
        }
        #endregion

        #region Methods
        public void Draw(GraphicsContext graphics)
        {
            if (!faction.LocalFogOfWar.IsEnabled) return;

            if (IsDirty)
            {
                UpdateTexture(dirtyRegion.Value);
                dirtyRegion = null;
            }

            Rectangle terrainBounds = new Rectangle(0, 0,
                faction.LocalFogOfWar.Size.Width, faction.LocalFogOfWar.Size.Height);
            graphics.Fill(terrainBounds, texture, TextureRectangle, fogColor);
        }

        private void OnVisibilityChanged(Faction faction, Region region)
        {
            if (dirtyRegion.HasValue)
                dirtyRegion = Region.Union(dirtyRegion.Value, region);
            else
                dirtyRegion = region;
        }

        private void UpdatePixelBuffer(Region region)
        {
            byte fogAlpha = (byte)(fogTransparency * 255.99f);

            int exclusiveMaxX = region.ExclusiveMaxX;
            int exclusiveMaxY = region.ExclusiveMaxY;
            for (int x = region.MinX; x < exclusiveMaxX; ++x)
            {
                for (int y = region.MinY; y < exclusiveMaxY; ++y)
                {
                    Point point = new Point(x, y);
                    int pixelIndex = (y - region.MinY) * region.Width + (x - region.MinX);
                    TileVisibility visibility = faction.GetTileVisibility(point);
                    if (visibility == TileVisibility.Undiscovered)
                        pixelBuffer[pixelIndex] = 255;
                    else if (visibility == TileVisibility.Discovered)
                        pixelBuffer[pixelIndex] = fogAlpha;
                    else
                    {
                        Debug.Assert(visibility == TileVisibility.Visible);
                        pixelBuffer[pixelIndex] = 0;
                    }
                }
            }
        }

        private void UpdatePixelBuffer()
        {
            Region region = (Region)faction.World.Size;
            UpdatePixelBuffer(region);
        }

        private void UpdateTexture(Region area)
        {
            UpdatePixelBuffer(area);
            texture.Blit(area, pixelBuffer);
        }

        private void DebugDumpToFile()
        {
            BufferedPixelSurface surface = new BufferedPixelSurface(faction.LocalFogOfWar.Size, PixelFormat.Alpha);
            for (int x = 0; x < surface.Size.Width; ++x)
            {
                for (int y = 0; y < surface.Size.Height; ++y)
                {
                    int pixelIndex = y * surface.Size.Width + x;
                    Point point = new Point(x, y);
                    TileVisibility visibility = faction.LocalFogOfWar.GetTileVisibility(point);
                    if (visibility == TileVisibility.Visible)
                        surface.Data.Array[pixelIndex] = 0;
                    else if (visibility == TileVisibility.Discovered)
                        surface.Data.Array[pixelIndex] = (byte)(fogTransparency * 255);
                    else
                        surface.Data.Array[pixelIndex] = 255;
                }
            }
            PixelSurface.SaveToFile(surface, "FogOfWar.png");
        }

        public void Dispose()
        {
            texture.Dispose();
        }
        #endregion
    }
}
