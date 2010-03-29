using System;
using Orion.Engine.Graphics;
using Orion.Engine.Geometry;

namespace Orion.Engine.Gui
{
    /// <summary>
    /// A view renderers which renders a rectangle filled with a color and bordered by another.
    /// </summary>
    public sealed class FilledRenderer : IViewRenderer
    {
        #region Fields
        private ColorRgba fillColor = Colors.DarkGray;
        private ColorRgba borderColor = Colors.Gray;
        #endregion

        #region Constructors
        public FilledRenderer() {}

        public FilledRenderer(ColorRgba fillColor)
        {
            this.fillColor = fillColor;
        }

        public FilledRenderer(ColorRgba fillColor, ColorRgba borderColor)
        {
            this.fillColor = fillColor;
            this.borderColor = borderColor;
        }
        #endregion

        #region Properties
        public ColorRgba FillColor
        {
            get { return fillColor; }
            set { fillColor = value; }
        }

        public ColorRgba BorderColor
        {
            get { return borderColor; }
            set { borderColor = value; }
        }
        #endregion

        #region Methods
        public void Draw(GraphicsContext context, Rectangle bounds)
        {
            context.Fill(bounds, FillColor);
            context.Stroke(bounds, BorderColor);
        }
        #endregion
    }
}
