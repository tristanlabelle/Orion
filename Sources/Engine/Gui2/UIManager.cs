using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using Orion.Engine.Graphics;
using Orion.Engine.Input;
using Keys = System.Windows.Forms.Keys;
using MouseButtons = System.Windows.Forms.MouseButtons;
using System.Diagnostics;

namespace Orion.Engine.Gui2
{
    /// <summary>
    /// The root of the UI hierarchy. Manages the focus and allows the injection of events.
    /// </summary>
    public sealed partial class UIManager : ContentControl
    {
        #region Fields
        private static readonly Func<Control, MouseState, MouseButtons, float, bool> MouseMoveCaller
            = (sender, state, button, amount) => sender.OnMouseMove(state);
        private static readonly Func<Control, MouseState, MouseButtons, float, bool> MouseButtonCaller
            = (sender, state, button, amount) => sender.OnMouseButton(state, button, (int)amount);
        private static readonly Func<Control, MouseState, MouseButtons, float, bool> MouseWheelCaller
            = (sender, state, button, amount) => sender.OnMouseWheel(state, amount);

        private readonly GuiRenderer renderer;
        private TimeSpan time;
        private MouseState mouseState;
        private Control hoveredControl;
        private Control keyboardFocusedControl;
        private Control mouseCapturedControl;
        private Texture cursorTexture;
        #endregion

        #region Constructors
        public UIManager(GuiRenderer renderer)
        {
            Argument.EnsureNotNull(renderer, "renderer");

            this.renderer = renderer;
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when the UI hierarchy gets updated.
        /// The parameter specifies the time elapsed since the last update.
        /// </summary>
        public event Action<UIManager, TimeSpan> Updated;

        /// <summary>
        /// Raised when the <see cref="Control"/> having the keyboard focus changes.
        /// </summary>
        public event Action<UIManager, Control> KeyboardFocusedControlChanged;

        /// <summary>
        /// Raised when the <see cref="Control"/> having captured the mouse changes.
        /// </summary>
        public event Action<UIManager, Control> MouseCapturedControlChanged;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the <see cref="GuiRenderer"/> responsible of drawing this UI hierarchy.
        /// </summary>
        public new GuiRenderer Renderer
        {
            get { return renderer; }
        }

        /// <summary>
        /// Gets the current time for this <see cref="UIManager"/>.
        /// </summary>
        public TimeSpan Time
        {
            get { return time; }
        }

        /// <summary>
        /// Gets the current state of the mouse.
        /// </summary>
        public MouseState MouseState
        {
            get { return mouseState; }
        }

        /// <summary>
        /// Gets the <see cref="Control"/> containing the mouse cursor.
        /// </summary>
        public Control HoveredControl
        {
            get { return hoveredControl; }
        }

        /// <summary>
        /// Accesses the <see cref="Control"/> which currently has the keyboard focus.
        /// This is the <see cref="Control"/> to which key and character events are routed.
        /// </summary>
        public Control KeyboardFocusedControl
        {
            get { return keyboardFocusedControl; }
            set
            {
                if (value == keyboardFocusedControl) return;
                if (value != null && value.Manager != this) throw new InvalidOperationException("Cannot give the keyboard focus to a control from another manager.");

                Control previous = keyboardFocusedControl;
                keyboardFocusedControl = value;
                if (previous != null) previous.OnKeyboardFocusLost();
                if (keyboardFocusedControl != null) keyboardFocusedControl.OnKeyboardFocusAcquired();

                KeyboardFocusedControlChanged.Raise(this, keyboardFocusedControl);
            }
        }

        /// <summary>
        /// Accesses the <see cref="Control"/> which currently has captured the mouse.
        /// This <see cref="Control"/> has a veto on mouse events, it gets a chance to process them before the normal hierarchy.
        /// </summary>
        public Control MouseCapturedControl
        {
            get { return mouseCapturedControl; }
            set
            {
                if (value == mouseCapturedControl) return;
                if (value != null && value.Manager != this) throw new InvalidOperationException("Cannot capture the mouse by a control from another manager.");

                Control previous = mouseCapturedControl;
                mouseCapturedControl = value;
                if (previous != null) previous.OnMouseCaptureLost();
                if (mouseCapturedControl != null) mouseCapturedControl.OnMouseCaptureAcquired();

                MouseCapturedControlChanged.Raise(this, mouseCapturedControl);
            }
        }

        /// <summary>
        /// Accesses the current cursor texture.
        /// </summary>
        public Texture CursorTexture
        {
            get { return cursorTexture; }
            set { cursorTexture = value; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Arranges this UI hierarchy.
        /// </summary>
        public void Arrange()
        {
            if (IsArranged && IsMeasured) return;

            Measure();
            Arrange(new Region(DesiredSize));
            ArrangeChildren();

            return;
        }

        /// <summary>
        /// Updates this UI hierarchy.
        /// </summary>
        /// <param name="elapsedTime">The time elapsed since the last update.</param>
        public void Update(TimeSpan elapsedTime)
        {
            if (elapsedTime < TimeSpan.Zero) throw new InvalidOperationException("The elapsed time should be positive.");

            Arrange();

            time += elapsedTime;
            Updated.Raise(this, elapsedTime);
        }

        /// <summary>
        /// Draws the UI hierarchy beneath this <see cref="UIManager"/>.
        /// </summary>
        public new void Draw()
        {
            Arrange();

            Renderer.Begin();

            Draw(this);
            if (cursorTexture != null)
            {
                var cursorSprite = new GuiSprite(cursorTexture)
                {
                    Rectangle = new Region(mouseState.X, mouseState.Y - cursorTexture.Height, cursorTexture.Width, cursorTexture.Height)
                };

                renderer.DrawSprite(ref cursorSprite);
            }

            Renderer.End();
        }

        private void Draw(Control control)
        {
            if (!control.IsArranged)
            {
                Debug.Fail("Attempted to draw a control that is not arranged.");
                return;
            }

            if (control.Visibility != Visibility.Visible || control.Rectangle.Area == 0)
                return;

            if (control.Adornment != null) control.Adornment.DrawBackground(renderer, control);

            Region? previousClippingRectangle = Renderer.ClippingRectangle;
            Renderer.ClippingRectangle = control.Rectangle;

            control.Draw();

            foreach (Control child in control.Children)
                Draw(child);

            renderer.ClippingRectangle = previousClippingRectangle;

            if (control.Adornment != null) control.Adornment.DrawForeground(renderer, control);
        }

        #region Input Event Injection
        /// <summary>
        /// Injects a mouse move event to be handled by the UI.
        /// </summary>
        /// <param name="x">The new X coordinate of the mouse.</param>
        /// <param name="y">The new Y coordinate of the mouse.</param>
        public void InjectMouseMove(int x, int y)
        {
            if (x == mouseState.X && y == mouseState.Y) return;

            Arrange();

            mouseState = new MouseState(x, y, mouseState.Buttons);

            InjectMouseEvent(MouseMoveCaller, MouseButtons.None, 0);
        }

        /// <summary>
        /// Injects a mouse button event to be handled by the UI.
        /// </summary>
        /// <param name="button">The button that was pressed or released.</param>
        /// <param name="pressCount">
        /// The number of successive presses of the button, or <c>0</c> if the button was released.
        /// </param>
        public void InjectMouseButton(MouseButtons button, int pressCount)
        {
            if (button == MouseButtons.None) return;
            EnsureValid(button);
            Argument.EnsurePositive(pressCount, "pressCount");

            Arrange();

            MouseButtons buttons = mouseState.Buttons & ~button;
            if (pressCount > 0) buttons &= button;

            mouseState = new MouseState(mouseState.Position, buttons);

            InjectMouseEvent(MouseButtonCaller, button, pressCount);
        }

        /// <summary>
        /// Injects a mouse wheel event to be handled by the UI.
        /// </summary>
        /// <param name="amount">The number of notches the wheel was rolled, as a real number.</param>
        public void InjectMouseWheel(float amount)
        {
            Argument.EnsureFinite(amount, "amount");

            Arrange();

            InjectMouseEvent(MouseWheelCaller, MouseButtons.None, amount);
        }

        private static void EnsureValid(MouseButtons button)
        {
            if (!PowerOfTwo.Is((uint)button)) throw new InvalidEnumArgumentException("button", (int)button, typeof(MouseButtons));
        }

        private void InjectMouseEvent(Func<Control, MouseState, MouseButtons, float, bool> caller, MouseButtons button, float amount)
        {
            // If a control has captured the mouse, it gets a veto on mouse events,
            // regardless of if they are in its client area.
            if (mouseCapturedControl != null)
            {
                bool handled = caller(mouseCapturedControl, mouseState, button, amount);
                if (handled) return;
            }

            // Update the mouse position and generate mouse entered/exited events.
            Control target = GetDescendantAt(mouseState.Position);
            if (target != hoveredControl)
            {
                Control commonAncestor = Control.FindCommonAncestor(target, hoveredControl);
                if (hoveredControl != null) NotifyMouseExited(hoveredControl, commonAncestor);
                if (target != null) NotifyMouseEntered(target, commonAncestor);
                hoveredControl = target;
            }

            // Let the target or one of its descendant handle the event.
            if (target != null)
            {
                Control handler = target;
                do
                {
                    bool handled = caller(handler, mouseState, button, amount);
                    if (handled) break;

                    handler = handler.Parent;
                } while (handler != null);
            }
        }

        /// <summary>
        /// Injects a keyboard event into the UI hierarchy.
        /// </summary>
        /// <param name="key">The key to be injected, with any active modifiers.</param>
        /// <param name="pressed">A value indicating if the key was pressed or released.</param>
        public void InjectKey(Keys keyAndModifiers, bool pressed)
        {
            if (keyboardFocusedControl == null) return;

            Arrange();

            Keys key = keyAndModifiers & Keys.KeyCode;
            if (key == Keys.None) return;

            Control handler = keyboardFocusedControl;
            do
            {
                if (handler.OnKey(key, keyAndModifiers & Keys.Modifiers, pressed)) break;
                handler = handler.Parent;
            } while (handler != null);
        }

        /// <summary>
        /// Injects a character event into the UI hierarchy.
        /// </summary>
        /// <param name="character">The character to be injected.</param>
        public void InjectCharacter(char character)
        {
            if (keyboardFocusedControl == null) return;

            Arrange();

            keyboardFocusedControl.OnCharacter(character);
        }

        private void NotifyMouseExited(Control target, Control firstExcludedAncestor)
        {
            while (target != firstExcludedAncestor)
            {
                target.OnMouseExited();
                target = target.Parent;
            }
        }

        private void NotifyMouseEntered(Control target, Control firstExcludedAncestor)
        {
            while (target != firstExcludedAncestor)
            {
                Control ancestor = target;
                while (ancestor.Parent != firstExcludedAncestor)
                    ancestor = ancestor.Parent;
                ancestor.OnMouseEntered();
                firstExcludedAncestor = ancestor;
            }
        }
        #endregion
        #endregion
    }
}
