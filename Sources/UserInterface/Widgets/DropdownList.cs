﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Math;
using Orion.Engine.Graphics;
using Orion.Geometry;
using Orion.Graphics;

namespace Orion.UserInterface.Widgets
{
    public class DropdownList<T> : Frame
    {
        #region Nested Types
        private class DropdownMenuRow : View
        {
            #region Fields
            private readonly DropdownList<T> parent;
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

                Action<DropdownList<T>, T> handler = parent.SelectionChanged;
                if (handler != null) handler(parent, Value);
                return base.OnMouseUp(args);
            }

            protected internal override void Draw(GraphicsContext context)
            {
                if (!parent.Enabled) context.FillColor = new ColorRgba(Colors.Blue, 0.25f);
                else context.FillColor = Colors.Blue;

                context.Fill(Bounds);
                if (IsMouseOver && parent.selectedItem != this)
                {
                    context.FillColor = new ColorRgba(Colors.Black, 0.25f);
                    context.Fill(Bounds);
                }

                context.FillColor = parent.TextColor;
                parent.renderer.Draw(Value, context);
            }
            #endregion
        }
        #endregion

        #region Fields
        private readonly ListFrame menu;
        private readonly DropdownListRowValueRenderer<T> renderer;
        private ColorRgba textColor = Colors.White;
        private Func<T, string> stringConverter = (value) => value.ToString();
        private DropdownMenuRow selectedItem;
        private Responder latestRespondingAncestor;
        private Action<Responder, MouseEventArgs> parentMouseUp;
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
            menu = new ListFrame(Bounds);
            parentMouseUp = ParentMouseUp;
        }
        #endregion

        #region Events
        public event Action<DropdownList<T>, T> SelectionChanged;
        #endregion

        #region Properties
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        public ColorRgba TextColor
        {
            get { return textColor; }
            set { textColor = value; }
        }

        public Func<T, string> StringConverter
        {
            get { return stringConverter; }
            set
            {
                Argument.EnsureNotNull(value, "StringConverter");
                this.stringConverter = value;
            }
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
            if (Items.Contains(item))
                throw new InvalidOperationException("The same DropdownList can't contain twice the same item");

            DropdownMenuRow row = new DropdownMenuRow(this, item);
            menu.Children.Add(row);

            if (menu.Children.Count == 1)
                SelectedItem = item;

            menu.Bounds = menu.FullBounds;
            //menu.Frame = Bounds.TranslatedBy(0, -menu.Bounds.Height - Bounds.Height).ResizedTo(menu.Bounds.Size);
            menu.Frame = new Rectangle(Frame.Min, menu.Bounds.Size).TranslatedBy(0, -menu.Bounds.Height);
        }

        protected internal override void OnAddToParent(ViewContainer parent)
        {
            latestRespondingAncestor = (Responder)Root;
            latestRespondingAncestor.MouseUp += parentMouseUp;
            base.OnAddToParent(parent);
        }

        protected internal override void OnRemovedFromParent(ViewContainer parent)
        {
            latestRespondingAncestor.MouseUp -= parentMouseUp;
            base.OnRemovedFromParent(parent);
        }

        protected internal override void OnAncestryChanged(ViewContainer ancestor)
        {
            latestRespondingAncestor.MouseUp -= parentMouseUp;
            latestRespondingAncestor = (Responder)Root;
            latestRespondingAncestor.MouseUp += parentMouseUp;
            base.OnAncestryChanged(ancestor);
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

        protected override void Dispose(bool disposing)
        {
            if (disposing) SelectionChanged = null;
            base.Dispose(disposing);
        }
        #endregion
    }
}
