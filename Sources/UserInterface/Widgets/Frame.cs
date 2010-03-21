using System;
using OpenTK.Math;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Graphics;
using Orion.Graphics.Renderers;

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
        protected override bool OnMouseButtonPressed(MouseEventArgs args)
        {
            if (captureMouseEvents)
            {
                base.OnMouseButtonPressed(args);
                return false;
            }
            else
            {
                return base.OnMouseButtonPressed(args);
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
