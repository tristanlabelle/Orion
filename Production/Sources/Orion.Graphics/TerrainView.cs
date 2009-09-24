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
        }

        protected override void Draw(GraphicsContext context)
        {
            Triangle triangle = new Triangle(new Vector2(10, 10), new Vector2(20, 30), new Vector2(30, 10), Color.Red);
            context.Fill(triangle);
        }
    }
}
