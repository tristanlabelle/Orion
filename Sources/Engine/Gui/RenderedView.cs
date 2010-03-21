using System;
using Orion.Engine.Graphics;
using Orion.Engine.Geometry;

namespace Orion.Engine.Gui
{
    public class RenderedView : View
    {
        #region Fields
        private IViewRenderer renderer;
        #endregion

        #region Constructors
        public RenderedView(Rectangle frame, IViewRenderer renderer)
            : base(frame)
        {
            this.renderer = renderer;
        }
        #endregion

        #region Properties
        public IViewRenderer Renderer
        {
            get { return renderer; }
            set { renderer = value; }
        }
        #endregion

        #region Methods
        protected internal sealed override void Draw(GraphicsContext graphicsContext)
        {
            if (renderer != null) renderer.Draw(graphicsContext, Bounds);
        }
        #endregion
    }
}
