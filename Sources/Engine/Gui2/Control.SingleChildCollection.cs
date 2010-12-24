using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui2
{
    partial class Control
    {
        public sealed class SingleChildCollection : ICollection<Control>
        {
            #region Fields
            private readonly Func<Control> getter;
            private readonly Action<Control> setter;
            private readonly Control[] iterationArray = new Control[1];
            #endregion

            #region Constructors
            public SingleChildCollection(Func<Control> getter, Action<Control> setter)
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

            public Control Value
            {
                get { return getter(); }
                set { setter(value); }
            }
            #endregion

            #region Methods
            public void Add(Control item)
            {
                Argument.EnsureNotNull(item, "item");

                setter(item);
            }

            public void Clear()
            {
                setter(null);
            }

            public bool Contains(Control item)
            {
                return item != null && getter() == item;
            }

            public void CopyTo(Control[] array, int arrayIndex)
            {
                Control value = getter();
                if (value != null) array[arrayIndex] = value;
            }

            public bool Remove(Control item)
            {
                if (item == null || getter() == null) return false;
                setter(null);
                return true;
            }

            public IEnumerator<Control> GetEnumerator()
            {
                iterationArray[0] = getter();
                var sequence = iterationArray[0] == null ? Enumerable.Empty<Control>() : iterationArray;
                return sequence.GetEnumerator();
            }
            #endregion

            #region IEnumerable Members
            bool ICollection<Control>.IsReadOnly
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
