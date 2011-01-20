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
        private Control viewedControl;
        private Stretch stretch = Stretch.Uniform;
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the <see cref="Control"/> that is displayed through this <see cref="ControlViewport"/>.
        /// </summary>
        public Control ViewedControl
        {
            get { return viewedControl; }
            set { viewedControl = value; }
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
        #endregion

        #region Methods
        protected override Size MeasureSize(Size availableSize)
        {
            return Size.Zero;
        }

        protected override void ArrangeChildren() { }

        protected internal override void Draw()
        {
            if (viewedControl == null || viewedControl.Manager != Manager) return;

            Region rectangle = Rectangle;
            Region viewedControlRectangle = viewedControl.Rectangle;
            if (viewedControlRectangle.Area == 0) return;

            Vector2 translation = (Vector2)rectangle.Min - (Vector2)viewedControlRectangle.Min
                + Vector2.One * 5; // HACK: Translate the viewed control a tad to account for the lack of padding.

            using (Renderer.PushClippingRectangle(rectangle))
            using (Renderer.PushTransform(new Transform(translation, 0, Vector2.One)))
                viewedControl.Draw();
        }
        #endregion
    }
}
