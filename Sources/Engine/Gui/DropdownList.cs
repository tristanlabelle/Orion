using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Geometry;
using Orion.Engine.Input;
using OpenTK.Math;

namespace Orion.Engine.Gui
{
    public class DropdownList<T> : Panel
    {
        #region Fields
        private bool enabled = true;

        private IEnumerable<T> items;
        private T selectedItem;
        
        private ListPanel options;
        private Label shownTitle;

        private Func<T, string> toStringer;
        private Action<Responder, MouseEventArgs> closeDropdownList;
        #endregion

        #region Constructors
        public DropdownList(Rectangle frame)
            : base(frame)
        {
            items = new T[0];
            closeDropdownList = CloseDropdownList;
            toStringer = t => t.ToString();
        }

        public DropdownList(Rectangle frame, IEnumerable<T> items)
            : this(frame)
        {
            Items = items;
        }
        #endregion

        #region Properties
        public IEnumerable<T> Items
        {
            get { return items; }
            set
            {
                items = value;
                SelectedItem = items.First();
            }
        }

        public T SelectedItem
        {
            get { return selectedItem; }
            set
            {
                if (!object.Equals(selectedItem, value))
                {
                    selectedItem = value;
                    OnSelectionChanged(selectedItem);
                }
            }
        }

        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        public Func<T, string> ToStringDelegate
        {
            get { return toStringer; }
            set { toStringer = value; }
        }

        private Responder TopmostResponder
        {
            get
            {
                Responder parent = this;
                Responder checkedParent;
                do
                {
                    checkedParent = parent.Parent as Responder;
                    if (checkedParent != null)
                        parent = checkedParent;
                } while (checkedParent != null);
                return parent;
            }
        }
        #endregion

        #region Events
        public event Action<DropdownList<T>, T> SelectionChanged;
        #endregion

        #region Methods
        #region Event Handling
        private void OnSelectionChanged(T selection)
        {
            if (shownTitle != null)
                Children.Remove(shownTitle);

            shownTitle = new Label(toStringer(selectedItem));
            Children.Add(shownTitle);

            var handler = SelectionChanged;
            if (handler != null) handler(this, selection);
        }

        protected override bool OnMouseButtonPressed(MouseEventArgs args)
        {
            base.OnMouseButtonPressed(args);

            if (enabled)
            {
                T[] itemArray = items.ToArray();
                Rectangle listRectangle = Frame;
                Vector2 top = new Vector2(listRectangle.MinX, listRectangle.MaxY);
                listRectangle = listRectangle.TranslatedTo(0, 0).ScaledBy(1, itemArray.Length);
                listRectangle = listRectangle.TranslatedTo(top.X, top.Y - listRectangle.Height);
                listRectangle = ConvertToTopmostCoordinatesSystem(listRectangle);
                options = new ListPanel(listRectangle);
                foreach (T item in itemArray)
                    options.Children.Add(CreateRow(item));

                TopmostResponder.Children.Add(options);
                TopmostResponder.MouseButtonReleased += closeDropdownList;
            }
            
            return false;
        }

        private Panel CreateRow(T item)
        {
            Panel row = new Panel(Frame);
            row.MouseButtonReleased += (r, args) => SelectedItem = item;

            Label title = new Label(toStringer(item));
            row.Children.Add(title);
            return row;
        }

        private void CloseDropdownList(Responder sender, MouseEventArgs args)
        {
            TopmostResponder.Children.Remove(options);
            options = null;
            TopmostResponder.MouseButtonReleased -= closeDropdownList;
        }

        private Rectangle ConvertToTopmostCoordinatesSystem(Rectangle rect)
        {
            Responder parent = this.Parent as Responder;
            while (parent != null)
            {
                Vector2 scaleBy = new Vector2(parent.Bounds.Width / parent.Frame.Width, parent.Bounds.Height / parent.Frame.Height);
                rect = rect.TranslatedBy(parent.Frame.Min).ScaledBy(scaleBy).TranslatedBy(-parent.Bounds.Min);
                parent = parent.Parent as Responder;
            }
            return rect;
        }
        #endregion
        #endregion
    }
}
