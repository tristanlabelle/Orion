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

namespace Orion.Engine.Gui2
{
    /// <summary>
    /// Root of the UI element hierarchy.
    /// </summary>
    public sealed partial class UIManager : UIElement
    {
        #region Fields
        private static readonly Func<UIElement, MouseState, MouseButtons, float, bool> MouseMoveCaller
            = (sender, state, button, amount) => sender.HandleMouseMove(state);
        private static readonly Func<UIElement, MouseState, MouseButtons, float, bool> MouseButtonCaller
            = (sender, state, button, amount) => sender.HandleMouseButton(state, button, (int)amount);
        private static readonly Func<UIElement, MouseState, MouseButtons, float, bool> MouseWheelCaller
            = (sender, state, button, amount) => sender.HandleMouseWheel(state, amount);

        private readonly IGuiRenderer renderer;
        private readonly SingleChildCollection children;
        private UIElement root;
        private Size size = new Size(800, 600);
        private MouseState mouseState;
        private UIElement hoveredElement;
        private UIElement keyboardFocusedElement;
        private UIElement mouseCapturedElement;
        #endregion

        #region Constructors
        public UIManager(IGuiRenderer renderer)
        {
            Argument.EnsureNotNull(renderer, "renderer");

            this.renderer = renderer;
            this.children = new SingleChildCollection(() => Root, value => Root = value);
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when the <see cref="UIElement"/> having the keyboard focus changes.
        /// </summary>
        public event Action<UIManager, UIElement> KeyboardFocusedElementChanged;

        /// <summary>
        /// Raised when the <see cref="UIElement"/> having captured the mouse changes.
        /// </summary>
        public event Action<UIManager, UIElement> MouseCapturedElementChanged;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the <see cref="IGuiRenderer"/> responsible of drawing this UI hierarchy.
        /// </summary>
        public IGuiRenderer Renderer
        {
            get { return renderer; }
        }

        /// <summary>
        /// Gets the size of the client area where this <see cref="UIManager"/> can draw.
        /// </summary>
        public Size Size
        {
            get { return size; }
            set
            {
                if (value == size) return;
                size = value;
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Gets the current state of the mouse.
        /// </summary>
        public MouseState MouseState
        {
            get { return mouseState; }
        }

        /// <summary>
        /// Accesses the root element of the UI hierarchy.
        /// </summary>
        public UIElement Root
        {
            get { return root; }
            set
            {
                if (value == root) return;

                if (root != null)
                {
                    AbandonChild(root);
                    root = null;
                }
                
                if (value != null)
                {
                    AdoptChild(value);
                    root = value;
                }

                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Gets the <see cref="UIElement"/> containing the mouse cursor.
        /// </summary>
        public UIElement HoveredElement
        {
            get { return hoveredElement; }
        }

        /// <summary>
        /// Accesses the <see cref="UIElement"/> which currently has the keyboard focus.
        /// This is the <see cref="UIElement"/> to which key and character events are routed.
        /// </summary>
        public UIElement KeyboardFocusedElement
        {
            get { return keyboardFocusedElement; }
            set
            {
                if (value == keyboardFocusedElement) return;
                if (value != null && value.Manager != this) throw new InvalidOperationException("Cannot give the keyboard focus to an element from another manager.");

                UIElement previous = keyboardFocusedElement;
                keyboardFocusedElement = value;
                if (previous != null) previous.OnKeyboardFocusLost();
                if (keyboardFocusedElement != null) keyboardFocusedElement.OnKeyboardFocusAcquired();

                KeyboardFocusedElementChanged.Raise(this, keyboardFocusedElement);
            }
        }

        /// <summary>
        /// Accesses the <see cref="UIElement"/> which currently has captured the mouse.
        /// This <see cref="UIElement"/> has a veto on mouse events, it gets a chance to process them before the normal hierarchy.
        /// </summary>
        public UIElement MouseCapturedElement
        {
            get { return mouseCapturedElement; }
            set
            {
                if (value == mouseCapturedElement) return;
                if (value != null && value.Manager != this) throw new InvalidOperationException("Cannot capture the mouse by an element from another manager.");

                UIElement previous = mouseCapturedElement;
                mouseCapturedElement = value;
                if (previous != null) previous.OnMouseCaptureLost();
                if (mouseCapturedElement != null) mouseCapturedElement.OnMouseCaptureAcquired();

                MouseCapturedElementChanged.Raise(this, mouseCapturedElement);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Draws the UI hierarchy beneath this <see cref="UIManager"/>.
        /// </summary>
        public void Draw()
        {
            Draw(this, renderer);
        }

        private static void Draw(UIElement element, IGuiRenderer renderer)
        {
            Region rectangle;
            if (element.Visibility != Visibility.Visible || !element.TryGetRectangle(out rectangle) || rectangle.Area == 0)
                return;

            renderer.BeginDraw(element);

            foreach (UIElement child in element.Children)
                Draw(child, renderer);

            renderer.EndDraw(element);
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

            InjectMouseEvent(MouseWheelCaller, MouseButtons.None, amount);
        }

        private static void EnsureValid(MouseButtons button)
        {
            if (!PowerOfTwo.Is((uint)button)) throw new InvalidEnumArgumentException("button", (int)button, typeof(MouseButtons));
        }

        private void InjectMouseEvent(Func<UIElement, MouseState, MouseButtons, float, bool> caller, MouseButtons button, float amount)
        {
            // If an element has captured the mouse, it gets a veto on mouse events,
            // regardless of if they are in its client area.
            if (mouseCapturedElement != null)
            {
                bool handled = caller(mouseCapturedElement, mouseState, button, amount);
                if (handled) return;
            }

            // Update the mouse position and generate mouse entered/exited events.
            UIElement target = GetDescendantAt(mouseState.Position);
            if (target != hoveredElement)
            {
                UIElement commonAncestor = UIElement.FindCommonAncestor(target, hoveredElement);
                if (hoveredElement != null) NotifyMouseExited(hoveredElement, commonAncestor);
                if (target != null) NotifyMouseEntered(target, commonAncestor);
                hoveredElement = target;
            }

            // Let the target or one of its descendant handle the event.
            if (target != null)
            {
                UIElement handler = target;
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
            if (keyboardFocusedElement == null) return;

            Keys key = keyAndModifiers & Keys.KeyCode;
            if (key == Keys.None) return;

            UIElement handler = keyboardFocusedElement;
            do
            {
                if (handler.HandleKey(key, keyAndModifiers & Keys.Modifiers, pressed)) break;
                handler = handler.Parent;
            } while (handler != null);
        }

        /// <summary>
        /// Injects a character event into the UI hierarchy.
        /// </summary>
        /// <param name="character">The character to be injected.</param>
        public void InjectCharacter(char character)
        {
            if (keyboardFocusedElement == null) return;

            keyboardFocusedElement.HandleCharacter(character);
        }

        private void NotifyMouseExited(UIElement target, UIElement firstExcludedAncestor)
        {
            while (target != firstExcludedAncestor)
            {
                target.OnMouseExited();
                target = target.Parent;
            }
        }

        private void NotifyMouseEntered(UIElement target, UIElement firstExcludedAncestor)
        {
            while (target != firstExcludedAncestor)
            {
                UIElement ancestor = target;
                while (ancestor.Parent != firstExcludedAncestor)
                    ancestor = ancestor.Parent;
                ancestor.OnMouseEntered();
                firstExcludedAncestor = ancestor;
            }
        }
        #endregion

        protected override ICollection<UIElement> GetChildren()
        {
            return children;
        }

        protected override Size MeasureWithoutMargin()
        {
            return size;
        }
        #endregion
    }
}
