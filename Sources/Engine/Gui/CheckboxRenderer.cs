using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Math;
using Orion.Engine.Geometry;
using Orion.Engine.Graphics;

namespace Orion.Engine.Gui
{
    public class CheckboxRenderer : IViewRenderer
    {
        #region Fields
        private IViewRenderer backgroundRenderer;
        private Checkbox checkbox;
        #endregion

        #region Constructors
        public CheckboxRenderer(Checkbox checkbox, IViewRenderer backgroundRenderer)
        {
            this.checkbox = checkbox;
            this.backgroundRenderer = backgroundRenderer;
        }
        #endregion

        #region Properties
        public IViewRenderer BackgroundRenderer
        {
            get { return backgroundRenderer; }
        }
        #endregion

        #region Methods
        public void Draw(GraphicsContext context, Rectangle bounds)
        {
            backgroundRenderer.Draw(context, bounds);
            if (checkbox.IsChecked == true)
            {
                Vector2 topleft = new Vector2(bounds.MinX, bounds.MaxY);
                Vector2 bottomleft = bounds.Min;
                Vector2 topright = bounds.Max;
                Vector2 bottomright = new Vector2(bounds.MaxX, bounds.MinY);
                context.Stroke(new LineSegment(topleft, bottomright), Colors.Black);
                context.Stroke(new LineSegment(topright, bottomleft), Colors.Black);
            }
        }
        #endregion
    }
}
