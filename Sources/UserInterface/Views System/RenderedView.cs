using System;
using Orion.Engine.Graphics;
using Orion.Geometry;
using Orion.Graphics;
using Orion.Graphics.Renderers;

namespace Orion.UserInterface
{
    public class RenderedView : View
    {
        #region Fields
        private IRenderer renderer;
        #endregion

        #region Constructors
        public RenderedView(Rectangle frame, IRenderer renderer)
            : base(frame)
        {
            this.renderer = renderer;
        }
        #endregion

        #region Properties
        public IRenderer Renderer
        {
            get { return renderer; }
            set { renderer = value; }
        }
        #endregion

        #region Methods
        protected internal sealed override void Draw(GraphicsContext graphicsContext)
        {
            if (Renderer != null) Renderer.Draw(graphicsContext, Bounds);
        }
        #endregion
    }
}
