using System;
using Orion.Engine.Graphics;
using Orion.Engine.Geometry;

namespace Orion.Engine.Gui
{
    public delegate void RenderingDelegate(GraphicsContext context, Rectangle rectangle);

    public class DelegatedRenderer : IViewRenderer
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
