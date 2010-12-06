using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui2
{
    partial class UIManager
    {
        private sealed class ChildCollection : ICollection<UIElement>
        {
            #region Fields
            private readonly UIManager uiManager;
            #endregion

            #region Constructors
            internal ChildCollection(UIManager uiManager)
            {
                Argument.EnsureNotNull(uiManager, "uiManager");

                this.uiManager = uiManager;
            }
            #endregion

            #region Properties
            public int Count
            {
                get { return uiManager.Root == null ? 0 : 1; }
            }
            #endregion

            #region Methods
            public void Add(UIElement item)
            {
                Argument.EnsureNotNull(item, "item");

                if (uiManager.Root != null) throw new InvalidOperationException();
                uiManager.Root = item;
            }

            public void Clear()
            {
                uiManager.Root = null;
            }

            public bool Contains(UIElement item)
            {
                return item != null && uiManager.Root == item;
            }

            public void CopyTo(UIElement[] array, int arrayIndex)
            {
                if (uiManager.Root != null) array[arrayIndex] = uiManager.Root;
            }

            public bool Remove(UIElement item)
            {
                if (item == null || uiManager.Root != item) return false;
                uiManager.Root = null;
                return true;
            }

            public IEnumerator<UIElement> GetEnumerator()
            {
                return Enumerable.Repeat(uiManager.Root, Count).GetEnumerator();
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
