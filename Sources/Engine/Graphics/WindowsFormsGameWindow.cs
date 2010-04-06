using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using OpenTK.Graphics;
using OpenTK.Math;
using Orion.Engine.Collections;
using Orion.Engine.Gui;
using Orion.Engine.Input;
using WinForms = System.Windows.Forms;

namespace Orion.Engine.Graphics
{
    /// <summary>
    /// A game window implemented upon windows forms. 
    /// </summary>
    public sealed class WindowsFormsGameWindow : IGameWindow
    {
        #region Instance
        #region Fields
        private readonly GameWindowForm form;
        private readonly GraphicsContext graphicsContext;
        private WindowMode mode = WindowMode.Windowed;
        private bool wasClosed;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new game window from the desired title text and client size. 
        /// </summary>
        /// <param name="title">The title text of the window.</param>
        /// <param name="mode">The initial mode of the window. This is a hint.</param>
        /// <param name="clientAreaSize">The size of the client area of the window.</param>
        public WindowsFormsGameWindow(string title, WindowMode mode, Size clientAreaSize)
        {
            Argument.EnsureNotNull(title, "title");
            
            this.mode = mode;
            if (this.mode == WindowMode.Fullscreen && !TryChangePrimaryScreenResolution(clientAreaSize))
            {
                Debug.Fail("Failed to switch to a {0} resolution, falling back to windowed mode.".FormatInvariant(clientAreaSize));
                this.mode = WindowMode.Windowed;
            }

            form = new GameWindowForm();

            form.Text = title;
            form.ClientSize = new System.Drawing.Size(clientAreaSize.Width, clientAreaSize.Height);
            if (this.mode == WindowMode.Fullscreen) SetFullscreenStyle();

            form.Resize += OnResized;
            form.FormClosing += OnClosing;
            form.FormClosed += OnClosed;
            form.TextPasted += OnTextPasted;

            form.GLControl.MouseMove += OnMouseMoved;
            form.GLControl.MouseDown += OnMouseButtonPressed;
            form.GLControl.MouseUp += OnMouseButtonReleased;
            form.GLControl.MouseDoubleClick += OnMouseButtonDoubleClicked;
            form.GLControl.MouseWheel += OnMouseWheelScrolled;
            form.GLControl.KeyUp += OnKeyboardKeyReleased;
            form.GLControl.KeyPress += OnCharacter;
            form.GLControl.KeyDown += OnKeyboardKeyPressed;

            graphicsContext = new GraphicsContext(form.GLControl.SwapBuffers);
        }
        #endregion

        #region Events
        public event Action<IGameWindow, InputEvent> InputReceived;
        public event Action<IGameWindow> Resized;
        public event Action<IGameWindow> Closing;
        #endregion

        #region Properties
        public string Title
        {
            get { return form.Text; }
            set { form.Text = value; }
        }

        public WindowMode Mode
        {
            get { return mode; }
        }

        public Size ClientAreaSize
        {
            get { return new Size(form.GLControl.ClientSize.Width, form.GLControl.ClientSize.Height); }
        }

        public GraphicsContext GraphicsContext
        {
            get { return graphicsContext; }
        }

        public bool WasClosed
        {
            get { return wasClosed; }
        }
        #endregion

        #region Methods
        #region IGameWindow Interface
        public void SetWindowed(Size clientAreaSize)
        {
            if (mode == WindowMode.Fullscreen)
            {
                try { DisplayDevice.Default.RestoreResolution(); }
                catch (GraphicsModeException e)
                {
                    Debug.Fail("Failed to restore resolution: {0}.".FormatInvariant(e));
                }

                mode = WindowMode.Windowed;
                form.TopMost = false;
                form.FormBorderStyle = FormBorderStyle.Sizable;
                form.Location = System.Drawing.Point.Empty;
            }

            form.ClientSize = new System.Drawing.Size(clientAreaSize.Width, clientAreaSize.Height);
        }

        public void SetFullscreen(Size resolution)
        {
            if (!TryChangePrimaryScreenResolution(resolution))
            {
                throw new NotSupportedException("Fullscreen resolution not supported: {0}.".FormatInvariant(resolution));
            }

            SetFullscreenStyle();
            mode = WindowMode.Fullscreen;
        }

        private void SetFullscreenStyle()
        {
            form.TopMost = true;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Location = System.Drawing.Point.Empty;
        }

        public void Update()
        {
            Application.DoEvents();
        }

        public void Dispose()
        {
            form.Dispose();
        }
        #endregion

        #region Event Handlers
        #region Mouse Stuff
        private void OnMouseMoved(object sender, WinForms.MouseEventArgs args)
        {
            RaiseMouseEvent(MouseEventType.Moved,
                args.X, args.Y, MouseButton.None, 0, 0);
        }

        private void OnMouseButtonPressed(object sender, WinForms.MouseEventArgs args)
        {
            RaiseMouseEvent(MouseEventType.ButtonPressed,
                args.X, args.Y, args.Button, 0, 0);
        }

        private void OnMouseButtonReleased(object sender, WinForms.MouseEventArgs args)
        {
            RaiseMouseEvent(MouseEventType.ButtonReleased,
                args.X, args.Y, args.Button, 0, 0);
        }

        private void OnMouseWheelScrolled(object sender, WinForms.MouseEventArgs args)
        {
            RaiseMouseEvent(MouseEventType.WheelScrolled,
                args.X, args.Y, MouseButton.None, 0, args.Delta / 600f);
        }

        private void OnMouseButtonDoubleClicked(object sender, WinForms.MouseEventArgs args)
        {
            RaiseMouseEvent(MouseEventType.ButtonPressed,
                args.X, args.Y, args.Button, args.Clicks, args.Delta);
        }

        private void RaiseMouseEvent(MouseEventType type, float x, float y,
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

            RaiseMouseEvent(type, x, y, pressedButton, clickCount, wheelDelta);
        }

        private void RaiseMouseEvent(MouseEventType type, float x, float y,
            MouseButton button, int clickCount, float wheelDelta)
        {
            Vector2 position = new Vector2(x, (form.GLControl.Height - 1) - y);
            var eventArgs = new Orion.Engine.Input.MouseEventArgs(position, button, clickCount, wheelDelta);
            InputEvent inputEvent = InputEvent.CreateMouse(type, eventArgs);
            InputReceived.Raise(this, inputEvent);
        }
        #endregion

        #region Keyboard Events
        private void OnTextPasted(GameWindowForm sender, string text)
        {
            foreach (char character in text) RaiseCharacterEvent(character);
        }

        private void OnKeyboardKeyPressed(object sender, KeyEventArgs args)
        {
            if (args.KeyCode == Keys.Menu) args.Handled = true;

            RaiseKeyboardEvent(KeyboardEventType.ButtonPressed, args.KeyData);
        }

        private void OnKeyboardKeyReleased(object sender, KeyEventArgs args)
        {
            if (args.KeyCode == Keys.Menu) args.Handled = true;

            RaiseKeyboardEvent(KeyboardEventType.ButtonReleased, args.KeyData);
        }

        private void OnCharacter(object sender, KeyPressEventArgs args)
        {
            RaiseCharacterEvent(args.KeyChar);
        }

        private void RaiseKeyboardEvent(KeyboardEventType type, Keys keyAndModifiers)
        {
            KeyboardEventArgs args = new KeyboardEventArgs(keyAndModifiers);
            InputEvent inputEvent = InputEvent.CreateKeyboard(type, args);
            InputReceived.Raise(this, inputEvent);
        }

        private void RaiseCharacterEvent(char character)
        {
            InputEvent inputEvent = InputEvent.CreateCharacter(character);
            InputReceived.Raise(this, inputEvent);
        }
        #endregion

        private void OnResized(object sender, EventArgs args)
        {
            Resized.Raise(this);
        }

        private void OnClosing(object sender, FormClosingEventArgs args)
        {
            args.Cancel = true;
            form.Hide();
            wasClosed = true;
            Closing.Raise(this);
        }

        private void OnClosed(object sender, FormClosedEventArgs args)
        {
            if (mode == WindowMode.Fullscreen)
            {
                try { DisplayDevice.Default.RestoreResolution(); }
                catch (GraphicsModeException e)
                {
                    Debug.Fail("Failed to restore the display resolution: {0}".FormatInvariant(e));
                }
            }
        }
        #endregion
        #endregion
        #endregion

        #region Static
        #region Methods
        private static bool TryChangePrimaryScreenResolution(Size size)
        {
            DisplayResolution resolution = DisplayDevice.Default.AvailableResolutions
                .Where(res => res.Width == size.Width
                    && res.Height == size.Height
                    && res.BitsPerPixel == 32)
                .WithMaxOrDefault(res => res.RefreshRate);

            if (resolution == null) return false;

            try
            {
                DisplayDevice.Default.ChangeResolution(resolution);
                return true;
            }
            catch (GraphicsModeException)
            {
                return false;
            }
        }
        #endregion
        #endregion
    }
}