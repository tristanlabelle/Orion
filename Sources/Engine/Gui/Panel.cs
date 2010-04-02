using System;
using OpenTK.Math;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Engine.Gui;
using Orion.Engine.Input;

namespace Orion.Engine.Gui
{
    public class Panel : RenderedView
    {
        #region Fields
        private bool captureMouseEvents = true;
        #endregion

        #region Constructors
        public Panel(Rectangle frame)
            : base(frame, new FilledRenderer())
        { }

        public Panel(Rectangle frame, ColorRgba fillColor)
            : base(frame, new FilledRenderer(fillColor))
        { }

        public Panel(Rectangle frame, IViewRenderer renderer)
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
        #endregion
    }
}
