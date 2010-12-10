using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui2
{
    partial class UIElement
    {
        public sealed class SingleChildCollection : ICollection<UIElement>
        {
            #region Fields
            private readonly Func<UIElement> getter;
            private readonly Action<UIElement> setter;
            private readonly UIElement[] iterationArray = new UIElement[1];
            #endregion

            #region Constructors
            public SingleChildCollection(Func<UIElement> getter, Action<UIElement> setter)
            {
                Argument.EnsureNotNull(getter, "getter");
                Argument.EnsureNotNull(setter, "setter");

                this.getter = getter;
                this.setter = setter;
            }
            #endregion

            #region Properties
            public int Count
            {
                get { return getter() == null ? 0 : 1; }
            }

            public UIElement Value
            {
                get { return getter(); }
                set { setter(value); }
            }
            #endregion

            #region Methods
            public void Add(UIElement item)
            {
                Argument.EnsureNotNull(item, "item");

                setter(item);
            }

            public void Clear()
            {
                setter(null);
            }

            public bool Contains(UIElement item)
            {
                return item != null && getter() == item;
            }

            public void CopyTo(UIElement[] array, int arrayIndex)
            {
                UIElement value = getter();
                if (value != null) array[arrayIndex] = value;
            }

            public bool Remove(UIElement item)
            {
                if (item == null || getter() == null) return false;
                setter(null);
                return true;
            }

            public IEnumerator<UIElement> GetEnumerator()
            {
                iterationArray[0] = getter();
                var sequence = iterationArray[0] == null ? Enumerable.Empty<UIElement>() : iterationArray;
                return sequence.GetEnumerator();
            }
            #endregion

            #region IEnumerable Members
            bool ICollection<UIElement>.IsReadOnly
            {
                get { return false; }
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
            #endregion
        }
    }
}
