using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK.Math;

using Orion.GameLogic;
using Orion.Geometry;
using Orion.Graphics.Widgets;

using Color = System.Drawing.Color;

namespace Orion.Graphics
{
    class TerrainView : ClippedView
    {
        private World world;
        public TerrainView(Rectangle frame)
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
                unit.Position = new Vector2(count * 10, count * 10);
                count++;
            }

            Frame panel = new Frame(new Rectangle(50, 50, 100, 100));
            Children.Add(panel);
        }

        protected override void Draw(GraphicsContext context)
        {
            context.StrokeColor = Color.Red;
            context.StrokeRect(600, 600, 150, 300);
            context.FillColor = Color.Chocolate;
            context.FillEllipse(500, 500, 200, 100);
            context.StrokeTriangle(200, 200, 300, 400, 400, 200);

            foreach (Unit unit in world.Units)
            {
                /*Vector2 a = new Vector2(unit.Origin + (Vector2.UnitX * 10));
                Vector2 b = new Vector2(unit.Origin + (Vector2.UnitY * 10));
                Vector2 c = new Vector2(unit.Origin - (Vector2.UnitX * 10));
				
                context.FillTriangle(a, b, c);*/

                //this.AddSubview();
            }
        }
    }
}
