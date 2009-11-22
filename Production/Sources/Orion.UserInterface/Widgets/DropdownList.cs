using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Color = System.Drawing.Color;

using Orion.Geometry;
using Orion.Graphics;
using OpenTK.Math;

namespace Orion.UserInterface.Widgets
{
    public class DropdownList<T> : Frame
    {
        #region Nested Types
        private class DropdownMenu : View
        {
            #region Fields
            private DropdownList<T> parent;
            #endregion

            #region Constructors
            public DropdownMenu(DropdownList<T> parent)
                : base(parent.Bounds)
            {
                this.parent = parent;
            }
            #endregion

            #region Properties
            public new DropdownList<T> Parent
            {
                get { return parent; }
            }

            public override Rectangle Bounds
            {
                get { return new Rectangle(0,0, Parent.Bounds.Width, parent.Bounds.Height * Children.Count); }
                set { throw new InvalidOperationException("Dropdown menus must be let manage their Bounds themselves"); }
            }

            public override Rectangle Frame
            {
                get
                {
                    Vector2 translation = new Vector2(0, -parent.Bounds.Height * Children.Count) + parent.Frame.Min;
                    return Bounds.TranslatedBy(translation);
                }
                set { throw new InvalidOperationException("Dropdown menus must be let manage their Frame themselves"); }
            }
            #endregion

            #region Methods
            protected override bool OnMouseDown(MouseEventArgs args)
            {
                return false;
            }

            protected internal override void OnAddChild(ViewContainer child)
            {
                base.Bounds = Bounds;
                base.OnAddChild(child);
            }

            protected internal override void Draw(GraphicsContext context)
            {
                context.FillColor = Color.DeepSkyBlue;
                context.StrokeColor = Color.Black;
                context.Fill(Bounds);
                context.Stroke(Bounds);
            }

            public void AddItem(T item)
            {
                DropdownMenuRow row = new DropdownMenuRow(parent, item);
                row.Frame = row.Frame.TranslatedTo(0, Bounds.MaxY);
                Children.Add(row);
            }
            #endregion
        }

        private class DropdownMenuRow : View
        {
            #region Fields
            private readonly DropdownList<T> parent;
            private bool isSelectedItem;
            public readonly T Value;
            #endregion

            #region Constructors
            public DropdownMenuRow(DropdownMenuRow row)
                : this(row.parent, row.Value)
            { }

            public DropdownMenuRow(DropdownList<T> parent, T element)
                : base(parent.Bounds)
            {
                this.parent = parent;
                Value = element;
            }
            #endregion

            #region Properties
            public new DropdownList<T> Parent
            {
                get { return parent; }
            }
            #endregion

            #region Methods
            protected override bool OnMouseUp(MouseEventArgs args)
            {
                if (parent.Enabled) parent.SelectedItem = this.Value;

                GenericEventHandler<DropdownList<T>, T> handler = parent.SelectionChanged;
                if (handler != null) handler(parent, Value);
                return base.OnMouseUp(args);
            }

            protected internal override void Draw(GraphicsContext context)
            {
                if (!parent.Enabled) context.FillColor = Color.FromArgb(0x40, Color.Blue);
                else context.FillColor = Color.Blue;

                context.Fill(Bounds);
                if (IsMouseOver && parent.selectedItem != this)
                {
                    context.FillColor = Color.FromArgb(0x40, Color.Black);
                    context.Fill(Bounds);
                }
                context.FillColor = Color.White;
                parent.renderer.Draw(Value, context);
            }
            #endregion
        }
        #endregion

        #region Fields
        private readonly DropdownMenu menu;
        private readonly DropdownListRowValueRenderer<T> renderer;
        private DropdownMenuRow selectedItem;
        private Responder latestRespondingAncestor;
        private GenericEventHandler<Responder, MouseEventArgs> parentMouseUp;
        private bool enabled = true;
        #endregion

        #region Constructors
        public DropdownList(Rectangle frame)
            : this(frame, new DropdownListRowValueRenderer<T>())
        { }

        public DropdownList(Rectangle frame, DropdownListRowValueRenderer<T> renderer)
            : base(frame)
        {
            this.renderer = renderer;
            menu = new DropdownMenu(this);
            parentMouseUp = ParentMouseUp;
        }
        #endregion

        #region Events
        public event GenericEventHandler<DropdownList<T>, T> SelectionChanged;
        #endregion

        #region Properties

        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        public T SelectedItem
        {
            get
            {
                if (selectedItem == null) return default(T);
                return selectedItem.Value;
            }
            set
            {
                if (value == null) throw new ArgumentException("Dropdown lists can't select a null value", "SelectedItem");
                DropdownMenuRow row = menu.Children.OfType<DropdownMenuRow>().FirstOrDefault(r => r.Value.Equals(value));
                if (row == null) throw new InvalidOperationException("Cannot select an item not present in the list");
                Children.Remove(selectedItem);
                selectedItem = new DropdownMenuRow(row);
                Children.Add(selectedItem);
            }
        }

        public IEnumerable<T> Items
        {
            get { return menu.Children.Select(row => ((DropdownMenuRow)row).Value); }
        }

        #endregion

        #region Methods

        public void AddItem(T item)
        {
            menu.AddItem(item);
            if (menu.Children.Count == 1)
                SelectedItem = item;
        }

        protected internal override void OnAddToParent(ViewContainer parent)
        {
            latestRespondingAncestor = (Responder)Root;
            latestRespondingAncestor.MouseUp += parentMouseUp;
            base.OnAddToParent(parent);
        }

        protected internal override void OnRemoveFromParent(ViewContainer parent)
        {
            latestRespondingAncestor.MouseUp -= parentMouseUp;
            base.OnRemoveFromParent(parent);
        }

        protected internal override void OnAncestryChange(ViewContainer ancestor)
        {
            latestRespondingAncestor.MouseUp -= parentMouseUp;
            latestRespondingAncestor = (Responder)Root;
            latestRespondingAncestor.MouseUp += parentMouseUp;
            base.OnAncestryChange(ancestor);
        }

        protected override bool OnMouseDown(MouseEventArgs args)
        {
            if(Enabled) Parent.Children.Add(menu);
            return base.OnMouseDown(args);
        }

        private void ParentMouseUp(Responder source, MouseEventArgs args)
        {
            Parent.Children.Remove(menu);
        }

        public override void Dispose()
        {
            SelectionChanged = null;
            base.Dispose();
        }

        #endregion
    }
}
