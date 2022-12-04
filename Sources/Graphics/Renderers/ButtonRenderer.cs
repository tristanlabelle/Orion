using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Color = System.Drawing.Color;

namespace Orion.Graphics
{
    public abstract class ButtonRenderer : IRenderer
    {
        public Color StrokeColor;

        public ButtonRenderer()
        {
            StrokeColor = Color.Gray;
        }

        public ButtonRenderer(Color strokeColor)
        {
            StrokeColor = strokeColor;
        }

        public void RenderInto(GraphicsContext context)
        {
            context.StrokeColor = StrokeColor;
            context.Stroke(context.CoordinateSystem);
        }
    }

    public class PlainButtonRenderer : ButtonRenderer
    {
        public Color FillColor;

        public PlainButtonRenderer()
        {
            FillColor = Color.DarkBlue;
        }

        public PlainButtonRenderer(Color fillColor)
        {
            FillColor = fillColor;
        }

        public PlainButtonRenderer(Color fillColor, Color strokeColor)
            : base(strokeColor)
        {
            FillColor = fillColor;
        }

        public void RenderInto(GraphicsContext context)
        {
            context.FillColor = FillColor;
            context.Fill(context.CoordinateSystem);
            base.RenderInto(context);
        }
    }

    public class TextureButtonRenderer : ButtonRenderer
    {
        public readonly int TextureId;

        public TextureButtonRenderer(int textureId)
        {
            TextureId = textureId;
        }

        public TextureButtonRenderer(int textureId, Color strokeColor)
            : base(strokeColor)
        {
            TextureId = textureId;
        }

        public void RenderInto(GraphicsContext context)
        {
            context.FillTextured(context.CoordinateSystem, TextureId);
            base.RenderInto(context);
        }
    }
}
