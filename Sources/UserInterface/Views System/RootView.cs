using System;
using System.Collections.Generic;
using OpenTK.Graphics;
using OpenTK.Math;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Engine.Graphics;
using GraphicsContext = Orion.Engine.Graphics.GraphicsContext;

namespace Orion.UserInterface
{
    public sealed class RootView : Responder
    {
        #region Fields
        public static readonly Rectangle ContentsBounds = new Rectangle(1024, 768);

        private Rectangle frame;
        private Stack<UIDisplay> displays;
        private Responder focusedView;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a RootView object with a frame and bounds.
        /// </summary>
        /// <param name="frame">The frame of the view</param>
        /// <param name="bounds">The bounds of the view</param>
        public RootView(Rectangle frame, Rectangle bounds)
        {
            this.Frame = frame;
            this.Bounds = bounds;
            this.displays = new Stack<UIDisplay>();
            this.displays.Push(new NullUI());
        }
        #endregion

        #region Properties
        public UIDisplay TopmostDisplay
        {
            get { return displays.Peek(); }
        }

        public override Rectangle Frame
        {
            get { return frame; }
            set { frame = value; }
        }

        public Responder FocusedView
        {
            get { return focusedView; }
            set { focusedView = value; }
        }
        #endregion

        #region Methods
        public void PushDisplay(UIDisplay display)
        {
            UIDisplay topDisplay = null;
            while (topDisplay != displays.Peek())
            {
                topDisplay = displays.Peek();
                topDisplay.OnShadowed();
            }

            displays.Push(display);
            Children.Add(display);
            display.OnEntered();
        }

        public void PopDisplay(UIDisplay display)
        {
            if (displays.Count < 2) throw new InvalidOperationException("Cannot pop the initial display from the stack");
            if (TopmostDisplay != display) throw new InvalidOperationException("Cannot pop a display from the stack unless it's the current one");

            displays.Pop();
            display.Dispose();
            TopmostDisplay.OnEntered();
        }

        public new void Update(float timeDeltaInSeconds)
        {
            TopmostDisplay.PropagateUpdateEvent(timeDeltaInSeconds);
        }

        protected internal override bool PropagateMouseEvent(MouseEventType eventType, MouseEventArgs args)
        {
            Vector2 coords = args.Position;
            coords.Scale(Bounds.Width / Frame.Width, Bounds.Height / Frame.Height);

            bool canSink = TopmostDisplay.PropagateMouseEvent(eventType,
                new MouseEventArgs(coords.X, coords.Y, args.Button, args.ClickCount, args.WheelDelta));

            return canSink ? DispatchMouseEvent(eventType, args) : false;
        }

        protected internal override bool PropagateKeyboardEvent(KeyboardEventType type, KeyboardEventArgs args)
        {
            if (focusedView == null) return TopmostDisplay.PropagateKeyboardEvent(type, args);
            return focusedView.PropagateKeyboardEvent(type, args);
        }

        protected internal override void Render(GraphicsContext graphicsContext)
        {
            graphicsContext.Clear(Colors.Black);
            graphicsContext.ProjectionBounds = Bounds;

            displays.Peek().Render(graphicsContext);
        }
        #endregion
    }
}
