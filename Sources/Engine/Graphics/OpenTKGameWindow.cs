using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using OpenTK.Math;
using OpenTK.Platform;
using Orion.Engine.Gui;
using Orion.Engine.Input;

namespace Orion.Engine.Graphics
{
    /// <summary>
    /// Wraps the GameWindow provided by OpenTK behinds an <see cref="IGameWindow"/> interface.
    /// </summary>
    public sealed class OpenTKGameWindow : IGameWindow
    {
        #region Fields
        private readonly GameWindow window;
        private GraphicsContext graphicsContext;
        private bool wasClosed;
        #endregion

        #region Constructors
        public OpenTKGameWindow(string title, WindowMode mode, Size clientAreaSize)
        {
            Argument.EnsureNotNull(title, "title");
            Argument.EnsureDefined(mode, "mode");

            ColorFormat colorFormat = new ColorFormat(8, 8, 8, 8);
            GraphicsMode graphicsMode = new GraphicsMode(colorFormat);
            GameWindowFlags gameWindowFlags = mode == WindowMode.Fullscreen ? GameWindowFlags.Fullscreen : 0;
#if DEBUG
            GraphicsContextFlags flags = GraphicsContextFlags.Debug;
#else
            GraphicsContextFlags flags = GraphicsContextFlags.Default;
#endif
            window = new GameWindow(clientAreaSize.Width, clientAreaSize.Height,
                graphicsMode, title, gameWindowFlags, DisplayDevice.Default, 2, 0, flags);
            window.VSync = VSyncMode.On;

            // Give the window a chance to create its context.
            window.ProcessEvents();

            Debug.Assert(window.Exists && window.Context != null,
                "No OpenGL context is available for the OpenTK GameWindow, this might be bad.");

            window.Resize += OnWindowResized;
            window.Closing += OnWindowClosing;

            window.Keyboard.KeyDown += OnKeyboardKeyDown;
            window.Keyboard.KeyUp += OnKeyboardKeyUp;

            window.Mouse.Move += OnMouseMoved;
            window.Mouse.ButtonDown += OnMouseButtonPressed;
            window.Mouse.ButtonUp += OnMouseButtonReleased;
            window.Mouse.WheelChanged += OnMouseWheelChanged;

            graphicsContext = new GraphicsContext(window.Context.SwapBuffers);
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
            get { return window.Title; }
            set { window.Title = value; }
        }

        public Size ClientAreaSize
        {
            get { return new Size(window.Width, window.Height); }
        }

        public WindowMode Mode
        {
            get { return window.WindowState == WindowState.Fullscreen ? WindowMode.Fullscreen : WindowMode.Windowed; }
        }

        public GraphicsContext GraphicsContext
        {
            get { return graphicsContext; }
        }

        public bool WasClosed
        {
            get { return wasClosed || window.IsExiting || !window.Exists; }
        }
        #endregion

        #region Methods
        #region IGameWindow Interface
        public void SetWindowed(Size clientAreaSize)
        {
            if (window.WindowState == WindowState.Fullscreen)
                window.WindowState = WindowState.Normal;

            window.Width = clientAreaSize.Width;
            window.Height = clientAreaSize.Height;
        }

        public void SetFullscreen(Size resolution)
        {
            if (window.WindowState == WindowState.Fullscreen
                && window.Width == resolution.Width
                && window.Height == resolution.Height)
                return;

            window.Width = resolution.Width;
            window.Height = resolution.Height;

            if (window.WindowState != WindowState.Fullscreen)
                window.WindowState = WindowState.Fullscreen;
        }

        public void Update()
        {
            window.ProcessEvents();
        }

        public void Dispose()
        {
            window.Dispose();
        }
        #endregion

        #region Event Handling
        #region Keyboard
        private void OnKeyboardKeyDown(KeyboardDevice sender, Key key)
        {
            RaiseKeyboardEvent(KeyboardEventType.ButtonPressed, key);
        }

        private void OnKeyboardKeyUp(KeyboardDevice sender, Key key)
        {
            RaiseKeyboardEvent(KeyboardEventType.ButtonReleased, key);
        }

        private void RaiseKeyboardEvent(KeyboardEventType type, Key key)
        {
            KeyboardEventArgs args = new KeyboardEventArgs((Keys)(int)key);
            InputEvent inputEvent = InputEvent.CreateKeyboard(type, args);
            InputReceived.Raise(this, inputEvent);
        }
        #endregion

        #region Mouse
        private void OnMouseButtonPressed(object sender, MouseButtonEventArgs e)
        {
            RaiseMouseButtonEvent(MouseEventType.ButtonPressed,
                e.Position, e.Button, 1);
        }

        private void OnMouseButtonReleased(object sender, MouseButtonEventArgs e)
        {
            RaiseMouseButtonEvent(MouseEventType.ButtonReleased,
                e.Position, e.Button, 1);
        }

        private void OnMouseMoved(object sender, MouseMoveEventArgs e)
        {
            RaiseMouseEvent(MouseEventType.Moved, e.Position,
                Orion.Engine.Input.MouseButton.None, 0, 0);
        }

        private void OnMouseWheelChanged(object sender, MouseWheelEventArgs e)
        {
            RaiseMouseEvent(MouseEventType.WheelScrolled, e.Position,
                Orion.Engine.Input.MouseButton.None, 0, e.Delta);
        }

        private static Orion.Engine.Input.MouseButton GetMouseButton(OpenTK.Input.MouseButton button)
        {
            if (button == OpenTK.Input.MouseButton.Left) return Orion.Engine.Input.MouseButton.Left;
            if (button == OpenTK.Input.MouseButton.Right) return Orion.Engine.Input.MouseButton.Right;
            if (button == OpenTK.Input.MouseButton.Middle) return Orion.Engine.Input.MouseButton.Middle;
            return Orion.Engine.Input.MouseButton.None;
        }

        private void RaiseMouseButtonEvent(MouseEventType type, System.Drawing.Point screenPoint,
            OpenTK.Input.MouseButton button, int clickCount)
        {
            Orion.Engine.Input.MouseButton orionButton = GetMouseButton(button);
            if (orionButton == Orion.Engine.Input.MouseButton.None) return;

            RaiseMouseEvent(type, screenPoint, orionButton, clickCount, 0);
        }

        private void RaiseMouseEvent(MouseEventType type, System.Drawing.Point screenPoint,
            Orion.Engine.Input.MouseButton button, int clickCount, int wheelDelta)
        {
            System.Drawing.Point clientPoint = window.PointToClient(screenPoint);
            Orion.Engine.Input.MouseEventArgs args = new Orion.Engine.Input.MouseEventArgs(
                new Vector2(clientPoint.X, clientPoint.Y), button, clickCount, wheelDelta / 600.0f);
            InputEvent inputEvent = InputEvent.CreateMouse(type, args);
            InputReceived.Raise(this, inputEvent);
        }
        #endregion

        private void OnWindowResized(object sender, ResizeEventArgs e)
        {
            Resized.Raise(this);
        }

        private void OnWindowClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            wasClosed = true;
            Closing.Raise(this);
        }
        #endregion
        #endregion
    }
}
