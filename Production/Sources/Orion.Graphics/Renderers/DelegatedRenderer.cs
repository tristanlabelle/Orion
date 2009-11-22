
namespace Orion.Graphics
{
    public delegate void RenderingDelegate(GraphicsContext context);

    public class DelegatedRenderer : IRenderer
    {
        public readonly RenderingDelegate RenderingDelegate;

        public DelegatedRenderer(RenderingDelegate renderingDelegate)
        {
            RenderingDelegate = renderingDelegate;
        }

        public void Draw(GraphicsContext context)
        {
            RenderingDelegate(context);
        }
    }
}
