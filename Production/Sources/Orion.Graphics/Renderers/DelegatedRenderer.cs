using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Graphics.Renderers
{
    public delegate void RenderingDelegate(GraphicsContext context);

    public class DelegatedRenderer : IRenderer
    {
        public readonly RenderingDelegate RenderingDelegate;

        public DelegatedRenderer(RenderingDelegate renderingDelegate)
        {
            RenderingDelegate = renderingDelegate;
        }

        public void RenderInto(GraphicsContext context)
        {
            RenderingDelegate(context);
        }
    }
}
