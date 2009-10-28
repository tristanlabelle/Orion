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
        private static readonly Dictionary<MouseEventType, MouseEventType> oppositeMap;

        private readonly MouseEventType mouseTrigger;
        private readonly ClippedView scrolledView;
        private readonly Vector2 direction;
        private readonly Keys keyboardTrigger;

        private bool mouseTriggered;
        private bool keyboardTriggered;
        #endregion

        #region Constructors
        public Scroller(ClippedView view, Rectangle frame, Vector2 direction, MouseEventType trigger)
            : this(view, frame, direction, trigger, Keys.None)
        { }

        public Scroller(ClippedView view, Rectangle frame, Vector2 direction, Keys trigger)
            : this(view, frame, direction, MouseEventType.None, trigger)
        { }

        public Scroller(ClippedView view, Rectangle frame, Vector2 direction, MouseEventType mouseTrigger, Keys keyboardTrigger)
        {
            if (!oppositeMap.ContainsKey(mouseTrigger))
                throw new ArgumentException("Cannot use the mouse event type {0} as a trigger because it has no opposite event".FormatInvariant(mouseTrigger));
            
            Frame = frame;
            Bounds = frame;
            scrolledView = view;
            this.direction = direction;
            this.mouseTrigger = mouseTrigger;
            this.keyboardTrigger = keyboardTrigger;
        }

        static Scroller()
        {
            oppositeMap = new Dictionary<MouseEventType, MouseEventType>();
            oppositeMap[MouseEventType.MouseUp] = MouseEventType.MouseDown;
            oppositeMap[MouseEventType.MouseDown] = MouseEventType.MouseUp;
            oppositeMap[MouseEventType.MouseExited] = MouseEventType.MouseEntered;
            oppositeMap[MouseEventType.MouseEntered] = MouseEventType.MouseExited;
        }
        #endregion

        #region Methods

        protected internal override bool PropagateMouseEvent(MouseEventType type, MouseEventArgs args)
        {
            if (type == mouseTrigger)
            {
                mouseTriggered = true;
            }
            else if (type == oppositeMap[mouseTrigger])
            {
                mouseTriggered = false;
            }
            return true;
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