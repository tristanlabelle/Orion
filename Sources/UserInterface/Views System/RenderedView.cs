using System;
using Orion.Engine.Graphics;
using Orion.Geometry;
using Orion.Graphics;

namespace Orion.UserInterface
{
    public class RenderedView : View
    {
        #region Constructors
        public RenderedView(Rectangle frame, IRenderer renderer)
            : base(frame)
        {
            Renderer = renderer;
        }
        #endregion

        #region Properties
        public IRenderer Renderer { get; set; }
        #endregion

        #region Methods
        protected internal sealed override void Draw(GraphicsContext context)
        {
            if (Renderer != null) Renderer.Draw(context);
        }
        #endregion
    }
}
