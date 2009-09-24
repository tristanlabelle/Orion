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

        }
    }
}
