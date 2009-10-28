﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Color = System.Drawing.Color;

namespace Orion.Graphics
{
    public abstract class FrameRenderer : IRenderer
    {
        public Color StrokeColor;

        public FrameRenderer()
        {
            StrokeColor = Color.Gray;
        }

        public FrameRenderer(Color strokeColor)
        {
            StrokeColor = strokeColor;
        }

        public virtual void RenderInto(GraphicsContext context)
        {
            context.StrokeColor = StrokeColor;
            context.Stroke(context.CoordinateSystem);
        }
    }

    public class FilledFrameRenderer : FrameRenderer
    {
        public Color FillColor;

        public FilledFrameRenderer()
        {
            FillColor = Color.DarkBlue;
        }

        public FilledFrameRenderer(Color fillColor)
        {
            FillColor = fillColor;
        }

        public FilledFrameRenderer(Color fillColor, Color strokeColor)
            : base(strokeColor)
        {
            FillColor = fillColor;
        }

        public override void RenderInto(GraphicsContext context)
        {
            context.FillColor = FillColor;
            context.Fill(context.CoordinateSystem);
            base.RenderInto(context);
        }
    }

    public class TexturedFrameRenderer : FrameRenderer
    {
        public readonly int TextureId;

        public TexturedFrameRenderer(int textureId)
        {
            TextureId = textureId;
        }

        public TexturedFrameRenderer(int textureId, Color strokeColor)
            : base(strokeColor)
        {
            TextureId = textureId;
        }

        public override void RenderInto(GraphicsContext context)
        {
            context.FillTextured(context.CoordinateSystem, TextureId);
            base.RenderInto(context);
        }
    }
}
