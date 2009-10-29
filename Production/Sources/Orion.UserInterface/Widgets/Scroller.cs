using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Keys = System.Windows.Forms.Keys;

using Orion.Geometry;
using OpenTK.Math;

namespace Orion.UserInterface.Widgets
{
    public class Scroller : Responder
    {
        #region Fields

        private readonly ClippedView scrolledView;
        private readonly Vector2 direction;
        private readonly Keys keyboardTrigger;

        private bool mouseTriggered;
        private bool keyboardTriggered;
        #endregion

        #region Constructors
        public Scroller(ClippedView view, Rectangle frame, Vector2 direction, MouseEventType trigger)
            : this(view, frame, direction, Keys.None)
        { }

        public Scroller(ClippedView view, Rectangle frame, Vector2 direction, Keys keyboardTrigger)
        {
            Frame = frame;
            Bounds = frame;
            scrolledView = view;
            this.direction = direction;
            this.keyboardTrigger = keyboardTrigger;
        }
        #endregion

        #region Methods

        protected override bool OnMouseEnter(MouseEventArgs args)
        {
            mouseTriggered = true;
            return base.OnMouseEnter(args);
        }

        protected override bool OnMouseExit(MouseEventArgs args)
        {
            mouseTriggered = false;
            return base.OnMouseExit(args);
        }

        protected override bool OnKeyDown(KeyboardEventArgs args)
        {
            if (args.Key == keyboardTrigger)
            {
                keyboardTriggered = true;
            }
            return base.OnKeyDown(args);
        }

        protected override bool OnKeyUp(KeyboardEventArgs args)
        {
            if (args.Key == keyboardTrigger)
            {
                keyboardTriggered = false;
            }
            return base.OnKeyUp(args);
        }

        protected override void OnUpdate(UpdateEventArgs args)
        {
            if (mouseTriggered || keyboardTriggered)
            {
                scrolledView.ScrollBy(direction * args.Delta * 40);
            }
        }

        #endregion
    }
}