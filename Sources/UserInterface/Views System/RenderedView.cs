using System;
using Orion.Engine.Graphics;
using Orion.Geometry;
using Orion.Graphics;

namespace Orion.UserInterface
{
    public class RenderedView : View
    {
        public RenderedView(Rectangle frame, IRenderer renderer)
            : base(frame)
        {
            Renderer = renderer;
        }

        public IRenderer Renderer { get; set; }

        protected internal override void Render()
        {
            base.Render();
        }

        protected internal sealed override void Draw(GraphicsContext context)
        {
            if(Renderer != null) Renderer.Draw(context);
        }
    }
}
