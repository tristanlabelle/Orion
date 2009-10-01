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
        private ViewContainer scrolledView;
        private Vector2 direction;
        private Rectangle maxBounds;
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
        public Scroller(Rectangle frame, ViewContainer view, Vector2 direction, Rectangle maxBounds, Key triggerKey)
            : base(frame)
        {
            this.triggerKey = triggerKey;
            this.scrolledView = view;
            this.direction = direction;
            this.maxBounds = maxBounds;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Checks if the scroller will overflow the maximum bounds.
        /// </summary>
        /// <returns>True if we can safely translate the target view's bounds; false otherwise</returns>
        private bool ValidateBoundsOverflow()
        {
            if(maxBounds.ContainsPoint(scrolledView.Bounds.Origin + direction) &&
                maxBounds.ContainsPoint(scrolledView.Bounds.Max + direction))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Updates the scroller to move its target view's bounds origin.
        /// </summary>
        /// <param name="args">The <see cref="UpdateEventArgs"/></param>
        protected override void OnUpdate(UpdateEventArgs args)
        {
            if (mouseIsIn)
            {
                if (ValidateBoundsOverflow())
                {
                    scrolledView.Bounds = scrolledView.Bounds.Translate(direction);
                }
            }

            if (keyIsDown)
            {
                if (ValidateBoundsOverflow())
                {
                    scrolledView.Bounds = scrolledView.Bounds.Translate(direction);
                }
            }
        }
        protected override bool OnKeyDown(KeyboardEventArgs args)
        {
            if (args.Key == triggerKey)
            {
                keyIsDown = true;
            }
            
            return true; 
        }
        protected override bool OnKeyUp(KeyboardEventArgs args)
        {
            if (args.Key == triggerKey)
            {
                keyIsDown = false;
            }
            return true;
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
