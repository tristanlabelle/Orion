
using Orion.GameLogic;
using Orion.Geometry;
using Color = System.Drawing.Color;

namespace Orion.Graphics
{
    public sealed class FogOfWarRenderer
    {
        private readonly FogOfWar fogOfWar;
        private readonly World world;
        private Texture texture;
        public static readonly Color VisibleTileColor = Color.FromArgb(0, 0, 0, 0);
        public static readonly Color DiscoveredTileColor = Color.FromArgb(153, 0, 0, 0);
        public static readonly Color UndiscoveredTileColor = Color.FromArgb(255, 0, 0, 0);
        private readonly byte[] pixels;
        private bool hasFogOfWarChanged = true;

        public FogOfWarRenderer(World world, FogOfWar fogOfWar)
        {
            Argument.EnsureNotNull(fogOfWar, "fogOfWar");
            this.fogOfWar = fogOfWar;
            this.world = world;
            pixels = new byte[world.Width * world.Height * 4];
            fogOfWar.Changed += OnChanged;
            UpdateTexture();
        }

        private void OnChanged(FogOfWar fogOfWar)
        {
            hasFogOfWarChanged = true;
        }

        private void UpdateTexture()
        {
            for (int y = 0; y < world.Height; ++y)
            {
                for (int x = 0; x < world.Width; ++x)
                {
                    int pixelIndex = y * world.Width + x;
                    TileVisibility visibility = fogOfWar.GetTileVisibility(x, y);
                    if (visibility == TileVisibility.Undiscovered)
                    {
                        pixels[pixelIndex * 4 + 0] = UndiscoveredTileColor.R;
                        pixels[pixelIndex * 4 + 1] = UndiscoveredTileColor.G;
                        pixels[pixelIndex * 4 + 2] = UndiscoveredTileColor.B;
                        pixels[pixelIndex * 4 + 3] = UndiscoveredTileColor.A;
                    }
                    else if (visibility == TileVisibility.Discovered)
                    {
                        pixels[pixelIndex * 4 + 0] = DiscoveredTileColor.R;
                        pixels[pixelIndex * 4 + 1] = DiscoveredTileColor.G;
                        pixels[pixelIndex * 4 + 2] = DiscoveredTileColor.B;
                        pixels[pixelIndex * 4 + 3] = DiscoveredTileColor.A;
                    }
                    else if (visibility == TileVisibility.Visible)
                    {
                        pixels[pixelIndex * 4 + 0] = VisibleTileColor.R;
                        pixels[pixelIndex * 4 + 1] = VisibleTileColor.G;
                        pixels[pixelIndex * 4 + 2] = VisibleTileColor.B;
                        pixels[pixelIndex * 4 + 3] = VisibleTileColor.A;
                    }
                }
            }

            if (texture == null)
            {
                var textureBuilder = new TextureBuilder
                {
                    Width = world.Width,
                    Height = world.Height,
                    Format = TextureFormat.Rgba,
                    PixelData = pixels,
                    UseSmoothing = true
                };
                texture = textureBuilder.Build();
            }
            else
                texture.SetPixels(pixels);
        }

        public void Draw(GraphicsContext graphics)
        {
            if (hasFogOfWarChanged)
            { 
                UpdateTexture();
                hasFogOfWarChanged = false;
            }
            Rectangle terrainBounds = new Rectangle(0, 0, world.Width, world.Height);
            graphics.FillTextured(terrainBounds, texture);
        }
    }
}
