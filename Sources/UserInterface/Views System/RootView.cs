using System;
using System.Collections.Generic;
using OpenTK.Graphics;
using OpenTK.Math;
using Orion.Engine.Graphics;
using Orion.Geometry;
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
        private GraphicsContext graphicsContext;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a RootView object with a frame and bounds.
        /// </summary>
        /// <param name="frame">The frame of the view</param>
        /// <param name="bounds">The bounds of the view</param>
        public RootView(Rectangle frame, Rectangle bounds)
            : base()
        {
            this.Bounds = bounds;
            this.Frame = frame;
            this.displays = new Stack<UIDisplay>();
            this.displays.Push(new NullUI());
            this.graphicsContext = new GraphicsContext(bounds);
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
            set
            {
                frame = value;
                ResetViewport();
            }
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
                topDisplay.OnShadow(this);
            }

            displays.Push(display);
            Children.Add(display);
            display.OnEnter(this);
        }

        public void PopDisplay(UIDisplay display)
        {
            if (displays.Count < 2) throw new InvalidOperationException("Cannot pop the initial display from the stack");
            if (TopmostDisplay != display) throw new InvalidOperationException("Cannot pop a display from the stack unless it's the current one");

            displays.Pop();
            display.Dispose();
            TopmostDisplay.OnEnter(this);
        }

        public void Update(float delta)
        {
            TopmostDisplay.PropagateUpdateEvent(new UpdateEventArgs(delta));
        }

        protected internal override bool PropagateMouseEvent(MouseEventType eventType, MouseEventArgs args)
        {
            Vector2 coords = args.Position;
            coords.Scale(Bounds.Width / Frame.Width, Bounds.Height / Frame.Height);

            bool canSink = TopmostDisplay.PropagateMouseEvent(eventType,
                new MouseEventArgs(coords.X, coords.Y, args.ButtonPressed, args.Clicks, args.WheelDelta));

            if (canSink) return DispatchMouseEvent(eventType, args);
            return false;
        }

        protected internal override bool PropagateKeyboardEvent(KeyboardEventType type, KeyboardEventArgs args)
        {
            if (focusedView == null)
                return TopmostDisplay.PropagateKeyboardEvent(type, args);
            return focusedView.PropagateKeyboardEvent(type, args);
        }

        protected internal override void Render()
        {
            graphicsContext.Clear(Colors.Black);
            displays.Peek().Render();
        }

        private void ResetViewport()
        {
            Rectangle bounds = Bounds;
            GL.Viewport(0, 0, (int)Frame.Size.X, (int)Frame.Size.Y);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(bounds.MinX, bounds.Width, bounds.MinY, bounds.Height, -1, 1);
            GL.MatrixMode(MatrixMode.Modelview);
        }
        #endregion
    }
}
