
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
        public static readonly Color TransparencyColor = Color.FromArgb(0, 0, 0, 0);
        public static readonly Color TranslucencyColor = Color.FromArgb(153, 0, 0, 0);
        public static readonly Color FogColor = Color.FromArgb(255, 0, 0, 0);
        private readonly byte[] pixels;
        private bool hasFogOfWarChanged = true;

        public FogOfWarRenderer(World world, FogOfWar fogOfWar)
        {
            Argument.EnsureNotNull(fogOfWar, "fogOfWar");
            this.fogOfWar = fogOfWar;
            this.world = world;
            pixels = new byte[world.Width * world.Height * 4];
            fogOfWar.Changed += OnFogOfWarChanged;
            ChangeTexture();
        }

        private void OnFogOfWarChanged(FogOfWar fogOfWar)
        {
            Argument.EnsureNotNull(fogOfWar, "fogOfWar");
            hasFogOfWarChanged = true;
        }

        public void ChangeTexture()
        {
            for (int y = 0; y < world.Height; ++y)
            {
                for (int x = 0; x < world.Width; ++x)
                {
                    int pixelIndex = y * world.Width + x;
                    if (fogOfWar.HasSeenTile(x, y))
                    {
                        if (fogOfWar.SeesTileCurrently(x, y))
                        {
                            pixels[pixelIndex * 4 + 0] = TransparencyColor.R;
                            pixels[pixelIndex * 4 + 1] = TransparencyColor.G;
                            pixels[pixelIndex * 4 + 2] = TransparencyColor.B;
                            pixels[pixelIndex * 4 + 3] = TransparencyColor.A;
                        }
                        else
                        {
                            pixels[pixelIndex * 4 + 0] = TranslucencyColor.R;
                            pixels[pixelIndex * 4 + 1] = TranslucencyColor.G;
                            pixels[pixelIndex * 4 + 2] = TranslucencyColor.B;
                            pixels[pixelIndex * 4 + 3] = TranslucencyColor.A;
                        }
                    }
                    else
                    {
                        pixels[pixelIndex * 4 + 0] = FogColor.R;
                        pixels[pixelIndex * 4 + 1] = FogColor.G;
                        pixels[pixelIndex * 4 + 2] = FogColor.B;
                        pixels[pixelIndex * 4 + 3] = FogColor.A;
                    }
                }
            }

            this.texture = new Texture(world.Width, world.Height, TextureFormat.Rgba, pixels);
        }

        public void Draw(GraphicsContext graphics)
        {
            if (hasFogOfWarChanged)
            { 
                ChangeTexture();
                hasFogOfWarChanged = false;
            }
            Rectangle terrainBounds = new Rectangle(0, 0, world.Width, world.Height);
            graphics.FillTextured(terrainBounds, texture);
        }
    }
}
