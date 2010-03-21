using System;
using Orion.Engine;
using Orion.Engine.Graphics;
using Orion.Engine.Geometry;

namespace Orion.Graphics.Renderers
{
    public delegate void RenderingDelegate(GraphicsContext context, Rectangle rectangle);

    public class DelegatedRenderer : IRenderer
    {
        public readonly RenderingDelegate RenderingDelegate;

        public DelegatedRenderer(RenderingDelegate renderingDelegate)
        {
            Argument.EnsureNotNull(renderingDelegate, "renderingDelegate");
            RenderingDelegate = renderingDelegate;
        }

        public void Draw(GraphicsContext context, Rectangle rectangle)
        {
            RenderingDelegate(context, rectangle);
        }
    }
}
