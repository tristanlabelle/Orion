using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Graphics;
using Orion.Engine.Geometry;

namespace Orion.Engine.Gui
{
    /// <summary>
    /// Renders a view as a rounded rectangle.
    /// </summary>
    public sealed class RoundedRectangleRenderer : IViewRenderer
    {
        #region Fields
        private ColorRgba borderColor;
        private ColorRgba fillColor;
        #endregion

        #region Constructors
        public RoundedRectangleRenderer()
        {
            this.borderColor = Colors.Gray;
            this.fillColor = Colors.DarkGray;
        }

        public RoundedRectangleRenderer(ColorRgba borderColor, ColorRgba fillColor)
        {
            this.borderColor = borderColor;
            this.fillColor = fillColor;
        }
        #endregion

        #region Properties
        public ColorRgba BorderColor
        {
            get { return borderColor; }
            set { borderColor = value; }
        }

        public ColorRgba FillColor
        {
            get { return fillColor; }
            set { fillColor = value; }
        }
        #endregion

        #region Methods
        public void Draw(GraphicsContext graphicsContext, Rectangle bounds)
        {
            float cornerRadius = Math.Min(bounds.Width, bounds.Height) / 4;
            graphicsContext.FillRoundedRectangle(bounds, cornerRadius, fillColor);
            graphicsContext.StrokeRoundedRectangle(bounds, cornerRadius, borderColor);
        }
        #endregion
    }
}
