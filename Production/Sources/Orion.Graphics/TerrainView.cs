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
    /// <summary>
    /// A <see cref="View"/> which displays the game <see cref="Terrain"/>.
    /// </summary>
    public sealed class TerrainView : ClippedView
    {
        #region Fields
        private World world;
        #endregion

        #region Constructor
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
        #endregion

        #region Methods
        protected override void Draw(GraphicsContext context)
        {
            context.StrokeColor = Color.Red;
            context.Stroke(new Rectangle(600, 600, 150, 300));
            context.FillColor = Color.Chocolate;
            context.Fill(new Ellipse(500, 500, 200, 100));
            context.Stroke(new Triangle(200, 200, 300, 400, 400, 200));

            foreach (Unit unit in world.Units)
            {
                /*Vector2 a = new Vector2(unit.Origin + (Vector2.UnitX * 10));
                Vector2 b = new Vector2(unit.Origin + (Vector2.UnitY * 10));
                Vector2 c = new Vector2(unit.Origin - (Vector2.UnitX * 10));
				
                context.FillTriangle(a, b, c);*/

                //this.AddSubview();
            }
        }
        #endregion
    }
}
