using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using OpenTK.Math;

using Orion.Graphics.Drawing;
using Orion.GameLogic;

namespace Orion.Graphics
{
    class TerrainView : ClippedView
    {
        private World world;
        public TerrainView(Rect frame)
            : base(frame)
        {
            world = new World();

            for (uint i = 0; i < 3; i++)
            {
                world.Units.Add(new Unit(i, new UnitType("mcwarrior"), world));
            }
            float count = 0;
            foreach (Unit unit in world.Units) 
            {
                unit.Position = new Vector2(count, count);
                count++;
            }
            
        }

        protected override void Draw(GraphicsContext context)
        {
            var rect = new Orion.Graphics.Drawing.Rectangle(Bounds, Color.White);
            context.Fill(rect);
            rect = new Orion.Graphics.Drawing.Rectangle(Bounds, Color.Black);
            context.Stroke(rect);

            foreach (Unit unit in world.Units)
            {
                Triangle triangle = new Triangle(unit.Position + (Vector2.UnitX * 10), unit.Position + (Vector2.UnitY * 10), unit.Position - (Vector2.UnitY * 10), Color.Red);
            }
        }
    }
}
