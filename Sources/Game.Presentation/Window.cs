using System;
using System.Diagnostics;
using System.Windows.Forms;
using OpenTK.Math;
using Orion.Engine;
using Orion.Engine.Graphics;
using Orion.Engine.Geometry;
using Orion.Engine.Gui;
using MouseButtons = System.Windows.Forms.MouseButtons;
using SysPoint = System.Drawing.Point;
using SysMouseEventArgs = System.Windows.Forms.MouseEventArgs;

namespace Orion.Game.Presentation
{
    /// <summary>
    /// The base game window class. 
    /// </summary>
    public partial class Window : Form
    {
        #region Fields
        private readonly GraphicsContext graphicsContext;
        private SysPoint lastGameMousePosition;
        private MouseButtons lastMouseButtons;
        #endregion

        #region Constructors
        /// <summary>
        /// Instantiates a new game window. 
        /// </summary>
        public Window()
        {
            InitializeComponent();

            Rectangle windowBounds = new Rectangle(glControl.Width, glControl.Height);

            this.lastGameMousePosition = GetGameMousePosition();
            this.lastMouseButtons = Control.MouseButtons;

            this.CreateControl();
            this.graphicsContext = new GraphicsContext(glControl.SwapBuffers);
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when the window has received some kind of input.
        /// </summary>
        public event Action<Window, InputEvent> InputReceived;
        #endregion

        #region Properties
        public GraphicsContext GraphicsContext
        {
            get { return graphicsContext; }
        }

        public Size ViewportSize
        {
            get { return new Size(glControl.ClientSize.Width, glControl.ClientSize.Height); }
        }
        #endregion

        #region Methods
        #region Mouse Stuff
        private SysPoint GetGameMousePosition()
        {
            SysPoint mousePosition = glControl.PointToClient(Control.MousePosition);

            if (mousePosition.X < 0) mousePosition.X = 0;
            if (mousePosition.X >= glControl.Width) mousePosition.X = glControl.Width - 1;

            if (mousePosition.Y < 0) mousePosition.Y = 0;
            if (mousePosition.Y >= glControl.Height) mousePosition.Y = glControl.Height - 1;

            return mousePosition;
        }

        private void CheckMouseEvents()
        {
            SysPoint newGameMousePosition = GetGameMousePosition();
            MouseButtons newMouseButtons = Control.MouseButtons;

            if (newGameMousePosition != lastGameMousePosition)
            {
                TriggerMouseEvent(MouseEventType.MouseMoved,
                    newGameMousePosition.X, newGameMousePosition.Y,
                    MouseButton.None, 0, 0);
                lastGameMousePosition = newGameMousePosition;
            }

            if (newMouseButtons != lastMouseButtons)
            {
                CheckMouseButtonStateChange(lastMouseButtons, newMouseButtons, MouseButtons.Left);
                CheckMouseButtonStateChange(lastMouseButtons, newMouseButtons, MouseButtons.Middle);
                CheckMouseButtonStateChange(lastMouseButtons, newMouseButtons, MouseButtons.Right);
                lastMouseButtons = newMouseButtons;
            }
        }

        private void CheckMouseButtonStateChange(
            MouseButtons lastMouseButtons, MouseButtons newMouseButtons, MouseButtons mouseButton)
        {
            if ((lastMouseButtons & mouseButton) != (newMouseButtons & mouseButton))
            {
                bool pressed = (newMouseButtons & mouseButton) == mouseButton;
                TriggerMouseEvent(pressed ? MouseEventType.MouseButtonPressed : MouseEventType.MouseButtonReleased,
                    lastGameMousePosition.X, lastGameMousePosition.Y,
                    mouseButton, 1, 0);
            }
        }

        private void glControl_MouseWheel(object sender, SysMouseEventArgs args)
        {
            TriggerMouseEvent(MouseEventType.MouseWheelScrolled,
                args.X, args.Y, MouseButton.None, 0, args.Delta / 600f);
        }

        private void glControl_MouseDoubleClick(object sender, SysMouseEventArgs args)
        {
            TriggerMouseEvent(MouseEventType.MouseButtonPressed,
                args.X, args.Y, args.Button, args.Clicks, args.Delta);
        }

        private void TriggerMouseEvent(MouseEventType type, float x, float y,
            MouseButtons button, int clickCount, float wheelDelta)
        {
            MouseButton pressedButton = MouseButton.None;
            switch (button)
            {
                case MouseButtons.None: pressedButton = MouseButton.None; break;
                case MouseButtons.Left: pressedButton = MouseButton.Left; break;
                case MouseButtons.Middle: pressedButton = MouseButton.Middle; break;
                case MouseButtons.Right: pressedButton = MouseButton.Right; break;
                default:
                    Debug.Fail("Unexpected mouse button: {0}.".FormatInvariant(button));
                    break;
            }

            TriggerMouseEvent(type, x, y, pressedButton, clickCount, wheelDelta);
        }

        private void TriggerMouseEvent(MouseEventType type, float x, float y,
            MouseButton button, int clickCount, float wheelDelta)
        {
            Vector2 position = new Vector2(x, (glControl.Height - 1) - y);
            var eventArgs = new Orion.Engine.Gui.MouseEventArgs(position, button, clickCount, wheelDelta);
            InputEvent inputEvent = InputEvent.CreateMouse(type, eventArgs);
            InputReceived.Raise(this, inputEvent);
        }
        #endregion

        #region Keyboard Events
        protected override void WndProc(ref Message m)
        {
            const int WM_PASTE = 0x0302;
            if (m.Msg == WM_PASTE)
            {
                string pastedText = Clipboard.GetText();
                if (!string.IsNullOrEmpty(pastedText))
                {
                    foreach (char pastedCharacter in pastedText)
                        TriggerCharacterEvent(pastedCharacter);

                    return;
                }
            }

            base.WndProc(ref m);
        }

        private void glControl_KeyDown(object sender, KeyEventArgs args)
        {
            if (args.Alt) args.Handled = true;

            TriggerKeyboardEvent(KeyboardEventType.ButtonPressed, args.KeyData);
        }

        private void glControl_KeyUp(object sender, KeyEventArgs args)
        {
            TriggerKeyboardEvent(KeyboardEventType.ButtonReleased, args.KeyData);
        }

        private void glControl_KeyPress(object sender, KeyPressEventArgs args)
        {
            TriggerCharacterEvent(args.KeyChar);
        }

        private void TriggerKeyboardEvent(KeyboardEventType type, Keys keyAndModifiers)
        {
            KeyboardEventArgs args = new KeyboardEventArgs(keyAndModifiers);
            InputEvent inputEvent = InputEvent.CreateKeyboard(type, args);
            InputReceived.Raise(this, inputEvent);
        }

        private void TriggerCharacterEvent(char character)
        {
            InputEvent inputEvent = InputEvent.CreateCharacter(character);
            InputReceived.Raise(this, inputEvent);
        }
        #endregion
        #endregion
    }
}