using System;
using System.Windows.Forms;
using Orion.Engine.Graphics;
using Orion.Geometry;
using MouseButtons = System.Windows.Forms.MouseButtons;
using SysPoint = System.Drawing.Point;
using SysMouseEventArgs = System.Windows.Forms.MouseEventArgs;

namespace Orion.UserInterface
{
    /// <summary>
    /// The base game window class. 
    /// </summary>
    public partial class Window : Form
    {
        #region Fields
        internal readonly RootView rootView;
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
            this.rootView = new RootView(windowBounds, RootView.ContentsBounds);

            this.graphicsContext = new GraphicsContext();

            this.lastGameMousePosition = GetGameMousePosition();
            this.lastMouseButtons = Control.MouseButtons;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Refreshes this game window, causing a render.
        /// </summary>
        public override void Refresh()
        {
            if (Form.ActiveForm == this && !glControl.IsDisposed && !IsDisposed)
                CheckMouseEvents();

            glControl.Refresh();
        }

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
                    MouseButtons.None, 0, 0);
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
                TriggerMouseEvent(pressed ? MouseEventType.MouseDown : MouseEventType.MouseUp,
                    lastGameMousePosition.X, lastGameMousePosition.Y,
                    mouseButton, 1, 0);
            }
        }

        private void glControl_MouseWheel(object sender, SysMouseEventArgs args)
        {
            TriggerMouseEvent(MouseEventType.MouseWheel,
                args.X, args.Y, args.Button, args.Clicks, args.Delta);
        }

        private void glControl_MouseDoubleClick(object sender, SysMouseEventArgs args)
        {
            TriggerMouseEvent(MouseEventType.DoubleClick,
                args.X, args.Y, args.Button, args.Clicks, args.Delta);
        }

        private void TriggerMouseEvent(MouseEventType type, float x, float y, MouseButtons argsButton, int clicks, int delta)
        {
            MouseButton pressedButton = MouseButton.None;
            switch (argsButton)
            {
                case MouseButtons.Left: pressedButton = MouseButton.Left; break;
                case MouseButtons.Middle: pressedButton = MouseButton.Middle; break;
                case MouseButtons.Right: pressedButton = MouseButton.Right; break;
            }

            rootView.PropagateMouseEvent(type,
                new Orion.MouseEventArgs(x, (glControl.Height - 1) - y, pressedButton, clicks, delta));
        }
        #endregion

        private void glControl_Paint(object sender, PaintEventArgs e)
        {
            rootView.Render(graphicsContext);
            glControl.SwapBuffers();
        }

        private void glControl_KeyDown(object sender, KeyEventArgs args)
        {
            if (args.Alt) args.Handled = true;
            if (args.KeyCode == Keys.V && args.Modifiers == Keys.Control && Clipboard.ContainsText())
            {
                string pastedText = Clipboard.GetText();
                foreach (char pastedCharacter in pastedText)
                    OnKeyPress(pastedCharacter);
                return;
            }

            TriggerKeyboardEvent(KeyboardEventType.KeyDown, args.KeyCode, args.Alt, args.Control, args.Shift);
        }

        private void glControl_KeyUp(object sender, KeyEventArgs args)
        {
            TriggerKeyboardEvent(KeyboardEventType.KeyUp, args.KeyCode, args.Alt, args.Control, args.Shift);
        }

        private void glControl_KeyPress(object sender, KeyPressEventArgs args)
        {
            OnKeyPress(args.KeyChar);
        }

        private void OnKeyPress(char keyChar)
        {
            rootView.PropagateKeyPressEvent(keyChar);
        }

        private void TriggerKeyboardEvent(KeyboardEventType type, Keys key, bool alt, bool control, bool shift)
        {
            KeyboardEventArgs args = new KeyboardEventArgs(key, alt, control, shift);
            rootView.PropagateKeyboardEvent(type, args);
        }

        /// <summary>
        /// Fires the Resized event to all listener, and resizes the glControl.
        /// </summary>
        /// <param name="e">Unused arguments</param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (rootView != null)
            {
                rootView.Frame = rootView.Frame.ResizedTo(glControl.Width, glControl.Height);
                glControl.Refresh();
            }
        }
        #endregion
    }
}