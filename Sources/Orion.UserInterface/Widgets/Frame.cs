﻿using Orion.Geometry;
using Orion.Graphics;
using OpenTK.Math;

namespace Orion.UserInterface.Widgets
{
    public class Frame : RenderedView
    {
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

        #region Methods
        protected override bool OnMouseDown(MouseEventArgs args)
        {
            base.OnMouseDown(args);
            return false;
        }

        protected override bool OnDoubleClick(MouseEventArgs args)
        {
            base.OnDoubleClick(args);
            return false;
        }
        #endregion
    }
}
