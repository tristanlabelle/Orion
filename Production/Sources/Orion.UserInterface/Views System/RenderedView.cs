
using Orion.Geometry;
using Orion.Graphics;

namespace Orion.UserInterface
{
    public class RenderedView : View
    {
        #region Fields
        public readonly IRenderer Renderer;
        #endregion

        public RenderedView(Rectangle frame, IRenderer renderer)
            : base(frame)
        {
            Renderer = renderer;
        }

        protected internal sealed override void Draw(GraphicsContext context)
        {
            Renderer.RenderInto(context);
        }
    }
}
