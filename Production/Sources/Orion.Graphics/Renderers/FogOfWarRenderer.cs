
using Orion.GameLogic;
using Orion.Geometry;
using Color = System.Drawing.Color;

namespace Orion.Graphics
{
    public sealed class FogOfWarRenderer
    {
        private readonly FogOfWar fogOfWar;
        private readonly World world;

        public FogOfWarRenderer(World world, FogOfWar fogOfWar)
        {
            Argument.EnsureNotNull(fogOfWar, "fogOfWar");
            this.fogOfWar = fogOfWar;
            this.world = world;
        }

        public void Draw(GraphicsContext graphics)
        {
            Argument.EnsureNotNull(graphics, "graphics");
            for (int i = 0; i < world.Width; i++)
                for (int j = 0; j < world.Height; j++)
                {
                    if (!fogOfWar.HasSeenTile(i, j))
                    {
                        graphics.FillColor = Color.DarkBlue;
                        graphics.Fill(new Rectangle(i, j, 1, 1));
                    }
                    else if (!fogOfWar.SeesTileCurrently(i, j))
                    {
                        graphics.FillColor = Color.Blue;
                        graphics.Fill(new Rectangle(i, j, 1, 1));
                    } 
                }
        }
    }
}
