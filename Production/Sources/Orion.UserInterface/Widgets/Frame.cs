using Orion.Geometry;
using Orion.Graphics;
using Color = System.Drawing.Color;

namespace Orion.UserInterface.Widgets
{
    public class Frame : RenderedView
    {
        public Frame(Rectangle frame)
            : base(frame, new FilledFrameRenderer())
        { }
        public Frame(Rectangle frame, Color fillColor)
            : base(frame, new FilledFrameRenderer(fillColor))
        { }
        public Frame(Rectangle frame, FrameRenderer renderer)
            : base(frame, renderer)
        { }

        protected override bool OnMouseDown(MouseEventArgs args)
        {
            base.OnMouseDown(args);
            return false;
        }
    }
}
