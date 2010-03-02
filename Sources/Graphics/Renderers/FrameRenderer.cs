﻿using System;
using Orion.Engine.Graphics;
using Orion.Geometry;

namespace Orion.Graphics
{
    public abstract class FrameRenderer : IRenderer
    {
        #region Fields
        public readonly ColorRgba StrokeColor;
        #endregion

        #region Contructors
        public FrameRenderer()
        {
            StrokeColor = Colors.Gray;
        }

        public FrameRenderer(ColorRgba strokeColor)
        {
            StrokeColor = strokeColor;
        }
        #endregion

        #region Methods
        public virtual void Draw(GraphicsContext context, Rectangle bounds)
        {
            context.StrokeColor = StrokeColor;
            context.Stroke(bounds);
        }
        #endregion
    }
}