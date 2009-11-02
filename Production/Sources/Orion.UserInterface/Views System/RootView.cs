using System;
using System.Collections.Generic;
using OpenTK.Graphics;
using OpenTK.Math;
using Orion.Geometry;
using Color = System.Drawing.Color;

namespace Orion.UserInterface
{
    public class RootView : Responder
    {
        #region Fields
        public static readonly Rectangle ContentsBounds = new Rectangle(1024, 768);

        private Rectangle frame;
        private Stack<UIDisplay> displays;
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
            Bounds = bounds;
            Frame = frame;
            displays = new Stack<UIDisplay>();
            displays.Push(new MainMenuUI());
        }
        #endregion

        #region Properties
        public override Rectangle Frame
        {
            get { return frame; }
            set
            {
                frame = value;
                ResetViewport();
            }
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
            display.OnEnter(this);
        }

        public void PopDisplay(UIDisplay display)
        {
            if (displays.Count < 2) throw new InvalidOperationException("Cannot pop the initial display from the stack");
            if (displays.Peek() != display) throw new InvalidOperationException("Cannot pop a display from the stack unless it's the current one");

            displays.Pop();
            displays.Peek().OnEnter(this);
        }

        public void Update(float delta)
        {
            displays.Peek().PropagateUpdateEvent(new UpdateEventArgs(delta));
        }

        protected internal override bool PropagateMouseEvent(MouseEventType eventType, MouseEventArgs args)
        {
            Vector2 coords = args.Position;
            coords.Scale(Bounds.Width / Frame.Width, Bounds.Height / Frame.Height);

            return displays.Peek().PropagateMouseEvent(eventType, new MouseEventArgs(coords.X, coords.Y, args.ButtonPressed, args.Clicks, args.WheelDelta));
        }

        protected internal override bool PropagateKeyboardEvent(KeyboardEventType type, KeyboardEventArgs args)
        {
            return displays.Peek().PropagateKeyboardEvent(type, args);
        }

        protected internal override void Render()
        {
            GL.ClearColor(Color.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.LoadIdentity();

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
