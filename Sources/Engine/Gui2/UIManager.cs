using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Orion.Engine.Graphics;
using Orion.Engine.Input;
using Keys = System.Windows.Forms.Keys;

namespace Orion.Engine.Gui2
{
    /// <summary>
    /// Root of the UI element hierarchy.
    /// </summary>
    public sealed partial class UIManager : UIElement
    {
        #region Fields
        private readonly GraphicsContext graphicsContext;
        private readonly SingleChildCollection children;
        private UIElement root;
        private Size size;
        private Font defaultFont = new Font("Trebuchet MS", 10);
        private ColorRgba defaultTextColor = Colors.Black;
        private UIElement hoveredElement;
        private UIElement keyboardFocusedElement;
        private UIElement mouseCapturedElement;
        #endregion

        #region Constructors
        public UIManager(GraphicsContext graphicsContext)
        {
            Argument.EnsureNotNull(graphicsContext, "graphicsContext");
            
            this.graphicsContext = graphicsContext;
            this.size = graphicsContext.ViewportSize;
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
        
        public Font DefaultFont
        {
            get { return defaultFont; }
            set
            {
                Argument.EnsureNotNull(value, "DefaultFont");
                defaultFont = value;
            }
        }
        
        public ColorRgba DefaultTextColor
        {
            get { return defaultTextColor; }
            set { defaultTextColor = value; }
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
                if (value.Manager != this) throw new InvalidOperationException("Cannot give the keyboard focus to an element from another manager.");

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
                if (value.Manager != this) throw new InvalidOperationException("Cannot capture the mouse by an element from another manager.");

                UIElement previous = mouseCapturedElement;
                mouseCapturedElement = value;
                if (previous != null) previous.OnMouseCaptureLost();
                if (mouseCapturedElement != null) mouseCapturedElement.OnMouseCaptureAcquired();

                MouseCapturedElementChanged.Raise(this, mouseCapturedElement);
            }
        }
        #endregion

        #region Methods
        public Size MeasureString(string text)
        {
            Argument.EnsureNotNull(text, "text");
            OpenTK.Vector2 size = new Text(text).Frame.Size;
            return new Size((int)Math.Ceiling(size.X), (int)Math.Ceiling(size.Y));
        }
        
        /// <summary>
        /// Draws the UI hierarchy beneath this <see cref="UIManager"/>.
        /// </summary>
        public void Draw()
        {
            Draw(graphicsContext);
        }
        
        /// <summary>
        /// Injects a mouse event into the UI hierarchy.
        /// </summary>
        /// <param name="type">The type of the mouse event.</param>
        /// <param name="args">A structure describing the mouse event.</param>
        public void SendMouseEvent(MouseEventType type, MouseEventArgs args)
        {
            // If an element has captured the mouse, it gets a veto on mouse events,
            // regardless of if they are in its client area.
            if (mouseCapturedElement != null)
            {
                bool handled = mouseCapturedElement.HandleMouseEvent(type, args);
                if (handled) return;
            }

            // Update the mouse position and generate mouse entered/exited events.
        	UIElement target = GetDescendantAt((Point)args.Position);
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
                    bool handled = handler.HandleMouseEvent(type, args);
                    if (handled) break;

                    handler = handler.Parent;
                } while (handler != null);
            }
        }

        /// <summary>
        /// Injects a keyboard event into the UI hierarchy.
        /// </summary>
        /// <param name="keyAndModifiers">
        /// A <see cref="Keys"/> enumerant containing both the key pressed and the active modifiers.
        /// </param>
        /// <param name="pressed">A value indicating if the key was pressed or released.</param>
        public void SendKeyEvent(Keys keyAndModifiers, bool pressed)
        {
            if (keyboardFocusedElement == null) return;

            UIElement handler = keyboardFocusedElement;
            do
            {
                if (handler.HandleKeyEvent(keyAndModifiers, pressed)) break;
                handler = handler.Parent;
            } while (handler != null);
        }

        /// <summary>
        /// Injects a character event into the UI hierarchy.
        /// </summary>
        /// <param name="character">The character to be injected.</param>
        public void SendCharacterEvent(char character)
        {
            if (keyboardFocusedElement == null) return;

            keyboardFocusedElement.HandleCharacterEvent(character);
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
