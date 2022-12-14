using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using Orion.Engine.Graphics;
using Key = OpenTK.Input.Key;
using Keys = System.Windows.Forms.Keys;
using Input = Orion.Engine.Input;

namespace Orion.Engine.Gui
{
    /// <summary>
    /// The root of the UI hierarchy. Manages the focus and allows the injection of events.
    /// </summary>
    public sealed partial class UIManager : ContentControl
    {
        #region Fields
        private readonly GuiRenderer renderer;
        private readonly PopupCollection popups;
        
        private TimeSpan time;
        private Point mousePosition;
        private MouseButtons mouseButtonStates;
        private ModifierKeys modifierKeys;
        private Control controlUnderMouse;
        private Control keyboardFocusedControl;
        private Control mouseCapturedControl;
        private Texture cursorTexture;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="UIManager"/> from the <see cref="GuiRenderer"/> which draws it.
        /// </summary>
        /// <param name="renderer">The <see cref="GuiRenderer"/> which draws this UI.</param>
        public UIManager(GuiRenderer renderer)
        {
            Argument.EnsureNotNull(renderer, "renderer");

            this.renderer = renderer;
            this.popups = new PopupCollection(this);
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
        public event Action<UIManager, Control> KeyboardFocusChanged;

        /// <summary>
        /// Raised when the <see cref="Control"/> having captured the mouse changes.
        /// </summary>
        public event Action<UIManager, Control> MouseCaptureChanged;
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
        /// Gets the collection of <see cref="Popups"/> currently displayed.
        /// </summary>
        public PopupCollection Popups
        {
        	get { return popups; }
        }

        /// <summary>
        /// Gets the current time for this <see cref="UIManager"/>.
        /// </summary>
        public TimeSpan Time
        {
            get { return time; }
        }

        /// <summary>
        /// Gets the last known position of the mouse.
        /// </summary>
        public Point MousePosition
        {
            get { return mousePosition; }
        }

        /// <summary>
        /// Gets the last known state of the mouse buttons.
        /// </summary>
        public MouseButtons MouseButtonStates
        {
            get { return mouseButtonStates; }
        }

        /// <summary>
        /// Gets the last known state of modifier keys.
        /// </summary>
        public ModifierKeys ModifierKeys
        {
            get { return modifierKeys; }
        }

        /// <summary>
        /// Gets the <see cref="Control"/> containing the mouse cursor.
        /// </summary>
        public Control ControlUnderMouse
        {
            get { return controlUnderMouse; }
            internal set { controlUnderMouse = value; }
        }

        /// <summary>
        /// Accesses the <see cref="Control"/> which currently has the keyboard focus.
        /// This is the <see cref="Control"/> to which key and character events are routed.
        /// </summary>
        [PropertyChangedEvent("KeyboardFocusChanged")]
        public Control KeyboardFocusedControl
        {
            get { return keyboardFocusedControl; }
            set
            {
                if (value == keyboardFocusedControl) return;
                if (value != null && value.Manager != this) throw new InvalidOperationException("Cannot give the keyboard focus to a control from another manager.");

                Control previous = keyboardFocusedControl;
                keyboardFocusedControl = value;
                if (previous != null) previous.HandleKeyboardFocusLoss();
                if (keyboardFocusedControl != null) keyboardFocusedControl.HandleKeyboardFocusAcquisition();

                KeyboardFocusChanged.Raise(this, keyboardFocusedControl);
            }
        }

        /// <summary>
        /// Accesses the <see cref="Control"/> which currently has captured the mouse.
        /// This <see cref="Control"/> has a veto on mouse events, it gets a chance to process them before the normal hierarchy.
        /// </summary>
        [PropertyChangedEvent("MouseCaptureChanged")]
        public Control MouseCapturedControl
        {
            get { return mouseCapturedControl; }
            set
            {
                if (value == mouseCapturedControl) return;
                if (value != null && value.Manager != this) throw new InvalidOperationException("Cannot capture the mouse by a control from another manager.");

                Control previous = mouseCapturedControl;
                mouseCapturedControl = value;
                if (previous != null) previous.HandleMouseCaptureLoss();
                if (mouseCapturedControl != null) mouseCapturedControl.HandleMouseCaptureAcquisition();

                MouseCaptureChanged.Raise(this, mouseCapturedControl);
            }
        }

        /// <summary>
        /// Gets the topmost modal <see cref="Popup"/> currently visible.
        /// </summary>
        public Popup ModalPopup
        {
            get
            {
                for (int i = popups.Count - 1; i >= 0; --i)
                {
                    Popup popup = popups[i];
                    if (popup.IsModal) return popup;
                }
                return null;
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
            
            Measure(new Size(Width.GetValueOrDefault(int.MaxValue), Height.GetValueOrDefault(int.MaxValue)));
            Arrange(new Region(DesiredSize));
            ArrangeChildren();
        }

        protected override Size MeasureSize(Size availableSize)
        {
            foreach (Popup popup in popups) popup.Measure(availableSize);
            return base.MeasureSize(availableSize);
        }
        
		protected override void ArrangeChildren()
		{
			base.ArrangeChildren();
			foreach (Popup popup in popups)
				DefaultArrangeChild(popup, popup.GetDesiredRectangle());
		}
		
		public override Control GetChildAt(Point point)
		{
			for (int i = popups.Count - 1; i >= 0; --i)
			{
				Popup popup = popups[i];
                if (popup.Visibility < Visibility.Visible) continue;
				if (popup.Rectangle.Contains(point)) return popup;
				if (popup.IsModal) return null;
			}
			
			return Content != null
                && Content.Visibility == Visibility.Visible
                && Content.Rectangle.Contains(point) ? Content : null;
		}
        
		protected override IEnumerable<Control> GetChildren()
		{
			if (Content != null) yield return Content;
			foreach (Popup popup in popups) yield return popup;
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

        #region Drawing
        /// <summary>
        /// Draws the UI hierarchy beneath this <see cref="UIManager"/>.
        /// </summary>
        public new void Draw()
        {
            Arrange();

            Renderer.Begin();

            DrawControlAndDescendants(this);
            
            DrawCursor();

            Renderer.End();
        }

        private void DrawControlAndDescendants(Control control)
        {
            if (!control.IsArranged)
            {
                Debug.Fail("Attempted to draw a control that is not arranged.");
                return;
            }

            if (control.VisibilityFlag < Visibility.Visible || control.Rectangle.Area == 0) return;

            using (Renderer.PushClippingRectangle(control.Rectangle))
            {
                control.RaisePreDrawing();

                if (control.Adornment != null) control.Adornment.DrawBackground(renderer, control);

                control.Draw();

                foreach (Control child in control.Children)
                    DrawControlAndDescendants(child);

                if (control.Adornment != null) control.Adornment.DrawForeground(renderer, control);

                control.RaisePostDrawing();
            }
        }

        private void DrawCursor()
        {
            if (cursorTexture != null)
            {
                var cursorSprite = new GuiSprite(cursorTexture)
                {
                    Rectangle = new Region(mousePosition.X, mousePosition.Y, cursorTexture.Width, cursorTexture.Height)
                };

                renderer.DrawSprite(ref cursorSprite);
            }
        }
        #endregion

        #region Input Event Injection
        /// <summary>
        /// Injects a mouse move event to be handled by the UI.
        /// </summary>
        /// <param name="x">The new X coordinate of the mouse.</param>
        /// <param name="y">The new Y coordinate of the mouse.</param>
        /// <returns>A value indicating if the event was handled.</returns>
        public bool InjectMouseMove(int x, int y)
        {
            MouseEvent @event = MouseEvent.CreateMove(new Point(x, y), mouseButtonStates, modifierKeys);
            return InjectMouseEvent(@event);
        }

        /// <summary>
        /// Injects a mouse button event to be handled by the UI.
        /// </summary>
        /// <param name="button">The button that was pressed or released.</param>
        /// <param name="pressed"><c>True</c> if the button was pressed, <c>false</c> if it was released.</param>
        /// <param name="clickCount">The number of successive click to which this button event belongs.</param>
        /// <returns>A value indicating if the event was handled.</returns>
        public bool InjectMouseButton(MouseButtons button, bool pressed, int clickCount)
        {
            EnsureValid(button);

            MouseEvent @event = MouseEvent.CreateButton(mousePosition, mouseButtonStates, modifierKeys, button, pressed, clickCount);
            return InjectMouseEvent(@event);
        }

        /// <summary>
        /// Injects a mouse wheel event to be handled by the UI.
        /// </summary>
        /// <param name="delta">The movement of the wheel, in notches.</param>
        /// <returns>A value indicating if the event was handled.</returns>
        public bool InjectMouseWheel(float delta)
        {
            Argument.EnsureFinite(delta, "amount");

            MouseEvent @event = MouseEvent.CreateWheel(mousePosition, mouseButtonStates, modifierKeys, delta);
            return InjectMouseEvent(@event);
        }

        /// <summary>
        /// Injects a mouse event to be handled by the UI.
        /// </summary>
        /// <param name="event">The event to be injected.</param>
        /// <returns>A value indicating if the event was handled.</returns>
        public bool InjectMouseEvent(MouseEvent @event)
        {
            Arrange();
            
            mousePosition = @event.Position;
            mouseButtonStates = @event.ButtonStates;
            modifierKeys = @event.ModifierKeys;

            // If a control has captured the mouse, it gets a veto on mouse events which are outside of its client area.
            Control targetControl = GetDescendantAt(@event.Position);
            if (mouseCapturedControl != null && (!mouseCapturedControl.HasDescendant(targetControl) || targetControl == null))
            {
                bool handled = mouseCapturedControl.HandleMouseEvent(@event);
                if (handled) return true;
            }
            
            if (controlUnderMouse != targetControl)
            {
                // Generate mouse entered/exited events.
                if (targetControl == null)
                {
                    NotifyMouseExited(controlUnderMouse, null);
                    controlUnderMouse = null;
                }
                else
                {
                    Control commonAncestor = Control.FindCommonAncestor(targetControl, controlUnderMouse);
                    if (controlUnderMouse != null) NotifyMouseExited(controlUnderMouse, commonAncestor);
                    if (targetControl != null) NotifyMouseEntered(targetControl, commonAncestor);
                    controlUnderMouse = targetControl;
                }
            }

            while (targetControl != null)
            {
                bool handled = targetControl.HandleMouseEvent(@event);
                if (handled) return true;
                targetControl = targetControl.Parent;
            }

            return false;
        }

        public bool InjectMouseEvent(Input.MouseEventType type, Input.MouseEventArgs args)
        {
            bool handled = InjectMouseMove((int)args.X, (int)args.Y);
            if (type == Input.MouseEventType.WheelScrolled)
            {
                return InjectMouseWheel(args.WheelDelta);
            }
            else if (type == Input.MouseEventType.ButtonPressed || type == Input.MouseEventType.ButtonReleased)
            {
                MouseButtons buttons = MouseButtons.None;
                if (args.Button == Input.MouseButton.Left) buttons = MouseButtons.Left;
                else if (args.Button == Input.MouseButton.Middle) buttons = MouseButtons.Middle;
                else if (args.Button == Input.MouseButton.Right) buttons = MouseButtons.Right;

                return InjectMouseButton(buttons, type == Input.MouseEventType.ButtonPressed, args.ClickCount);
            }

            return handled;
        }

        private static void EnsureValid(MouseButtons button)
        {
            if (!PowerOfTwo.Is((uint)button)) throw new InvalidEnumArgumentException("button", (int)button, typeof(MouseButtons));
        }

        /// <summary>
        /// Injects a keyboard event into the UI hierarchy.
        /// </summary>
        /// <param name="event">The event to be injected.</param>
        /// <returns>A value indicating if the event was handled.</returns>
        public bool InjectKeyEvent(KeyEvent @event)
        {
            modifierKeys = @event.ModifierKeys;
            if (keyboardFocusedControl == null) return false;

            Arrange();

            Control handler = keyboardFocusedControl;
            
            // Modal popup veto
            Popup modalPopup = ModalPopup;
            if (modalPopup != null && !modalPopup.HasDescendant(handler))
                handler = modalPopup;

            do
            {
                if (handler.HandleKeyEvent(@event)) return true;
                handler = handler.Parent;
            } while (handler != null);

            return false;
        }

        /// <summary>
        /// Injects a keyboard event into the UI hierarchy.
        /// </summary>
        /// <param name="keyAndModifiers">The key to be injected, with any active modifiers.</param>
        /// <param name="type">The type of the key event.</param>
        /// <returns>A value indicating if the event was handled.</returns>
        public bool InjectKeyAndModifiers(Keys keyAndModifiers, KeyEventType type)
        {
            modifierKeys = Input.InputEnums.GetOrionModifierKeys(keyAndModifiers & Keys.Modifiers);
            if (keyboardFocusedControl == null) return false;

            Arrange();

            Key key = Input.InputEnums.GetTKKey(keyAndModifiers & Keys.KeyCode);
            if (key == Key.Unknown) return false;

            KeyEvent @event = new KeyEvent(type, modifierKeys, key);
            return InjectKeyEvent(@event);
        }

        /// <summary>
        /// Injects a character event into the UI hierarchy.
        /// </summary>
        /// <param name="character">The character to be injected.</param>
        /// <returns>A value indicating if the event was handled.</returns>
        public bool InjectCharacter(char character)
        {
            if (keyboardFocusedControl == null) return false;

            Arrange();

            // Modal popup veto
            Popup modalPopup = ModalPopup;
            if (modalPopup != null && !modalPopup.HasDescendant(keyboardFocusedControl))
                return false;

            return keyboardFocusedControl.HandleCharacterTyped(character);
        }

        public bool InjectInputEvent(Input.InputEvent @event)
        {
            if (@event.Type == Input.InputEventType.Mouse)
            {
                Input.MouseEventType type;
                Input.MouseEventArgs args;
                @event.GetMouse(out type, out args);

                return InjectMouseEvent(type, args);
            }
            else if (@event.Type == Input.InputEventType.Keyboard)
            {
                Input.KeyboardEventType type;
                Input.KeyboardEventArgs args;
                @event.GetKeyboard(out type, out args);

                return InjectKeyAndModifiers(args.KeyAndModifiers,
                    type == Input.KeyboardEventType.ButtonPressed ? KeyEventType.Pressed : KeyEventType.Released);
            }
            else if (@event.Type == Input.InputEventType.Character)
            {
                char character;
                @event.GetCharacter(out character);

                return InjectCharacter(character);
            }

            return false;
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
