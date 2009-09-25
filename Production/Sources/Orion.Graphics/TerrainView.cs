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
			context.Color = Color.Red;
			foreach (Unit unit in world.Units)
			{
				Vector2 a = new Vector2(unit.Position + (Vector2.UnitX * 10));
				Vector2 b = new Vector2(unit.Position + (Vector2.UnitY * 10));
				Vector2 c = new Vector2(unit.Position - (Vector2.UnitY * 10));
				
				context.FillTriangle(a, b, c);
            }
        }
    }
}
