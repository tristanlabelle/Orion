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

        public virtual void Draw(GraphicsContext context)
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
            FillColor = Color.DarkGray;
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

        public override void Draw(GraphicsContext context)
        {
            context.FillColor = FillColor;
            context.Fill(context.CoordinateSystem);
            base.Draw(context);
        }
    }

    public class TexturedFrameRenderer : FrameRenderer
    {
        public readonly Texture Texture;

        public TexturedFrameRenderer(Texture texture)
        {
            Argument.EnsureNotNull(texture, "texture");
            Texture = texture;
        }

        public TexturedFrameRenderer(Texture texture, Color strokeColor)
            : base(strokeColor)
        {
            Argument.EnsureNotNull(texture, "texture");
            Texture = texture;
        }

        public override void Draw(GraphicsContext context)
        {
            context.Fill(context.CoordinateSystem, Texture);
            base.Draw(context);
        }
    }
}
