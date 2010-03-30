using System;
using System.Collections.Generic;
using OpenTK.Graphics;
using OpenTK.Math;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Engine.Graphics;
using Orion.Engine.Gui;
using GraphicsContext = Orion.Engine.Graphics.GraphicsContext;
using System.Collections;
using System.Diagnostics;

namespace Orion.Engine.Gui
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
            PopDisplay(display, true);
        }

        public void PopDisplayWithoutDisposing(UIDisplay display)
        {
            PopDisplay(display, false);
        }

        private void PopDisplay(UIDisplay display, bool dispose)
        {
            if (displays.Count < 2) throw new InvalidOperationException("Cannot pop the initial display from the stack");
            if (TopmostDisplay != display) throw new InvalidOperationException("Cannot pop a display from the stack unless it's the current one");

            displays.Pop();
            if (dispose) display.Dispose();
            TopmostDisplay.OnEntered();
        }

        #region Public event generation
        public void SendInputEvent(InputEvent inputEvent)
        {
            switch (inputEvent.Type)
            {
                case InputEventType.Keyboard:
                    KeyboardEventType type;
                    KeyboardEventArgs args;
                    inputEvent.GetKeyboard(out type, out args);
                    SendKeyboardEvent(type, args);
                    break;

                case InputEventType.Mouse:
                    break;

                case InputEventType.Character:
                    break;

                default:
                    Debug.Fail("Unknown input event type: {0}.".FormatInvariant(inputEvent.Type));
                    break;
            }
        }
        
        private void SendMouseEvent(MouseEventType type, MouseEventArgs args)
        {
            PropagateMouseEvent(type, args);
        }

        private void SendKeyboardEvent(KeyboardEventType type, KeyboardEventArgs args)
        {
            PropagateKeyboardEvent(type, args);
        }

        private void SendCharacterTypedEvent(char character)
        {
            PropagateCharacterTypedEvent(character);
        }

        public new void Update(float timeDeltaInSeconds)
        {
            TopmostDisplay.PropagateUpdateEvent(timeDeltaInSeconds);
        }

        public new void Draw(GraphicsContext graphicsContext)
        {
            Argument.EnsureNotNull(graphicsContext, "graphicsContext");
            Render(graphicsContext);
        }
        #endregion

        #region Protected Overrides
        protected internal override bool PropagateMouseEvent(MouseEventType eventType, MouseEventArgs args)
        {
            Vector2 propagatedMousePosition = args.Position;
            propagatedMousePosition.Scale(Bounds.Width / Frame.Width, Bounds.Height / Frame.Height);

            var propagatedArgs = args.CloneWithNewPosition(propagatedMousePosition);
            bool canSink = TopmostDisplay.PropagateMouseEvent(eventType, propagatedArgs);

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
        #endregion
    }
}
