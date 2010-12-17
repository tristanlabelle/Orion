using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Orion.Engine.Graphics;
using Orion.Engine.Input;

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
        /// Gets the element containing the mouse cursor.
        /// </summary>
        public UIElement HoveredElement
        {
            get { return hoveredElement; }
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
        	UIElement target = GetDescendantAt((Point)args.Position);
            if (target != hoveredElement)
            {
                UIElement commonAncestor = UIElement.FindCommonAncestor(target, hoveredElement);
                if (hoveredElement != null) NotifyMouseExited(hoveredElement, commonAncestor);
                if (target != null) NotifyMouseEntered(target, commonAncestor);
            }

            hoveredElement = target;
        	if (target != null) target.PropagateMouseEvent(type, args);
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
