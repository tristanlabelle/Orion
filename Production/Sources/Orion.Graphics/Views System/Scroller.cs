using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Math;
using OpenTK.Graphics;

using Orion.Geometry;
using Color = System.Drawing.Color;
using Key = System.Windows.Forms.Keys;

namespace Orion.Graphics
{
    /// <summary>
    /// Objects of this view subclass scroll another view in a given direction (they translate the bounds) when the user puts the mouse over it.
    /// </summary>
    public class Scroller : View
    {
        #region Fields
        private ClippedView scrolledView;
        private Vector2 direction;
        #endregion
        private bool keyIsDown = false;
        private bool mouseIsIn = false;
        private Key triggerKey;

        #region Constructors
        /// <summary>
        /// Construct a Scroller object that will stand in a given frame, scrolling the given view in a given direction.
        /// </summary>
        /// <param name="frame">The frame of the scroller view</param>
        /// <param name="view">The view to scroll</param>
        /// <param name="direction">The direction in which to translate the bounds</param>
        /// <param name="maxBounds">The bounds in which it's possible to scroll: the scroller can't translate the view past these</param>
        /// <param name="triggerKey">The key that can be pressed to trigger this scroller</param>
        public Scroller(Rectangle frame, ClippedView view, Vector2 direction, Key triggerKey)
            : base(frame)
        {
            this.triggerKey = triggerKey;
            this.scrolledView = view;
            this.direction = direction;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Updates the scroller to move its target view's bounds origin.
        /// </summary>
        /// <param name="args">The <see cref="UpdateEventArgs"/></param>
        protected override void OnUpdate(UpdateEventArgs args)
        {
            if (mouseIsIn || keyIsDown)
            {
                scrolledView.ScrollBy(direction * args.Delta * 40);
            }
        }

        /// <summary>
        /// Handles keyboard events to tell if the scroller must scroll its attached view. 
        /// </summary>
        /// <param name="args">
        /// The triggering <see cref="KeyboardEventArgs"/>
        /// </param>
        /// <returns>
        /// true, to allow the event to sink
        /// </returns>
        protected override bool OnKeyDown(KeyboardEventArgs args)
        {
            if (args.Key == triggerKey)
            {
                keyIsDown = true;
            }
            return base.OnKeyDown(args);
        }

        /// <summary>
        /// Handles keyboard events to tell if the scroller must scroll its attached view. 
        /// </summary>
        /// <param name="args">
        /// The <see cref="KeyboardEventArgs"/>
        /// </param>
        /// <returns>
        /// true, to allow the event to sink
        /// </returns>
        protected override bool OnKeyUp(KeyboardEventArgs args)
        {
            if (args.Key == triggerKey)
            {
                keyIsDown = false;
            }
            return base.OnKeyUp(args);
        }

        /// <summary>
        /// Indicates the user moved the mouse on to the scroller.
        /// </summary>
        /// <param name="args">The <see cref="MouseEventArgs"/></param>
        /// <returns>true</returns>
        protected override bool OnMouseEnter(MouseEventArgs args)
        {
            mouseIsIn = true;
            return base.OnMouseEnter(args);
        }

        /// <summary>
        /// Indicates the user moved the mouse out of the scroller.
        /// </summary>
        /// <param name="args">The <see cref="MouseEventArgs"/></param>
        /// <returns>true</returns>
        protected override bool OnMouseExit(MouseEventArgs args)
        {
            mouseIsIn = false;

            return base.OnMouseExit(args);
        }

        /// <summary>
        /// Draws nothing.
        /// </summary>
        protected override void Draw()
        { }

        #endregion
    }

}
