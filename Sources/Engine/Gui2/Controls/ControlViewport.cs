using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Orion.Engine.Gui2
{
    /// <summary>
    /// Provides a read-only view of another <see cref="Control"/>.
    /// </summary>
    public class ControlViewport : Control
    {
        #region Fields
        private Borders padding;
        private Control viewedControl;
        private Stretch stretch = Stretch.Uniform;
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the padding between the bounds of this <see cref="ControlViewport"/>
        /// and the outer rectangle of its <see cref="ViewedControl"/>.
        /// </summary>
        public Borders Padding
        {
            get { return padding; }
            set
            {
                if (value == padding) return;
                padding = value;
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Accesses the <see cref="Control"/> that is displayed through this <see cref="ControlViewport"/>.
        /// </summary>
        public Control ViewedControl
        {
            get { return viewedControl; }
            set
            {
                if (value == viewedControl) return;
                viewedControl = value;
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Accesses the way the viewed control gets stretched over the surface of this <see cref="ControlViewport"/>.
        /// </summary>
        public Stretch Stretch
        {
            get { return stretch; }
            set
            {
                Argument.EnsureDefined(stretch, "stretch");
                this.stretch = value;
            }
        }

        /// <summary>
        /// Gets the rectangle within the padding of this <see cref="ControlViewport"/>.
        /// </summary>
        public Region InnerRectangle
        {
            get
            {
                Region rectangle = Rectangle;
                return new Region(
                    rectangle.MinX + padding.MinX, rectangle.MinY + padding.MinY,
                    Math.Max(0, rectangle.Width - padding.TotalX),
                    Math.Max(0, rectangle.Height - padding.TotalY));
            }
        }
        #endregion

        #region Methods
        protected override Size MeasureSize(Size availableSize)
        {
            Size viewedControlSize = viewedControl != null && viewedControl.IsMeasured ? viewedControl.DesiredOuterSize : Size.Zero;
            return viewedControlSize + padding;
        }

        protected override void ArrangeChildren() { }

        protected internal override void Draw()
        {
            if (viewedControl == null || viewedControl.Manager != Manager) return;

            Region innerRectangle = InnerRectangle;
            Region viewedControlRectangle = viewedControl.Rectangle;
            if (viewedControlRectangle.Area == 0) return;

            Vector2 translation = (Vector2)innerRectangle.Min + (Vector2)innerRectangle.Size * 0.5f - (Vector2)viewedControlRectangle.Size * 0.5f
                - (Vector2)viewedControlRectangle.Min;

            using (Renderer.PushClippingRectangle(innerRectangle))
            using (Renderer.PushTransform(new Transform(translation, 0, Vector2.One)))
                viewedControl.Draw();
        }
        #endregion
    }
}
