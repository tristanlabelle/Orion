using Orion.Geometry;
using Orion.Graphics;
using OpenTK.Math;

namespace Orion.UserInterface.Widgets
{
    public class Frame : RenderedView
    {
        #region Fields
        private bool captureMouseEvents = true;
        #endregion

        #region Constructors
        public Frame(Rectangle frame)
            : base(frame, new FilledFrameRenderer())
        { }

        public Frame(Rectangle frame, ColorRgba fillColor)
            : base(frame, new FilledFrameRenderer(fillColor))
        { }

        public Frame(Rectangle frame, IRenderer renderer)
            : base(frame, renderer)
        { }
        #endregion

        #region Properties
        public bool CaptureMouseEvents
        {
            get { return captureMouseEvents; }
            set { captureMouseEvents = value; }
        }
        #endregion

        #region Methods
        protected override bool OnMouseDown(MouseEventArgs args)
        {
            if (captureMouseEvents)
            {
                base.OnMouseDown(args);
                return false;
            }
            else
            {
                return base.OnMouseDown(args);
            }
        }

        protected override bool OnDoubleClick(MouseEventArgs args)
        {
            if (captureMouseEvents)
            {
                base.OnDoubleClick(args);
                return false;
            }
            else
            {
                return base.OnDoubleClick(args);
            }
        }
        #endregion
    }
}
