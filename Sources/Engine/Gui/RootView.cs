using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenTK.Math;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Engine.Graphics;
using Orion.Engine.Gui;
using Orion.Engine.Input;

namespace Orion.Engine.Gui
{
    public sealed class RootView : Responder
    {
        #region Fields
        [Obsolete("Game Specific, to be moved out of the engine.")]
        public static readonly Rectangle ContentsBounds = new Rectangle(1024, 768);

        private Rectangle frame;
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
        }
        #endregion

        #region Properties
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
        public void SendInputEvent(InputEvent inputEvent)
        {
            if (inputEvent.Type == InputEventType.Keyboard)
            {
                KeyboardEventType type;
                KeyboardEventArgs args;
                inputEvent.GetKeyboard(out type, out args);
                SendKeyboardEvent(type, args);
            }
            else if (inputEvent.Type == InputEventType.Mouse)
            {
                MouseEventType type;
                MouseEventArgs args;
                inputEvent.GetMouse(out type, out args);
                SendMouseEvent(type, args);
            }
            else if (inputEvent.Type == InputEventType.Character)
            {
                char character;
                inputEvent.GetCharacter(out character);
                SendCharacterTypedEvent(character);
            }
            else
            {
                Debug.Fail("Unknown input event type: {0}.".FormatInvariant(inputEvent.Type));
            }
        }
        
        public void SendMouseEvent(MouseEventType type, MouseEventArgs args)
        {
            PropagateMouseEvent(type, args);
        }

        public void SendKeyboardEvent(KeyboardEventType type, KeyboardEventArgs args)
        {
            PropagateKeyboardEvent(type, args);
        }

        public void SendCharacterTypedEvent(char character)
        {
            PropagateCharacterTypedEvent(character);
        }

        public new void Update(float timeDeltaInSeconds)
        {
#warning Kind of an hack... can't we merge Responder and ViewContainer?
            foreach (var child in Children.OfType<Responder>())
                child.Update(timeDeltaInSeconds);
        }

        public new void Draw(GraphicsContext graphicsContext)
        {
            Argument.EnsureNotNull(graphicsContext, "graphicsContext");
            Render(graphicsContext);
        }

        protected internal override void Render(GraphicsContext graphicsContext)
        {
            graphicsContext.ProjectionBounds = Bounds;
            base.Render(graphicsContext);
        }
        #endregion
    }
}
