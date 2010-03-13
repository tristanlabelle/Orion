﻿using Orion.Engine.Graphics;
using Orion.Geometry;

namespace Orion.Graphics.Renderers
{
    public interface IRenderer
    {
        void Draw(GraphicsContext graphicsContext, Rectangle bounds);
    }
}
