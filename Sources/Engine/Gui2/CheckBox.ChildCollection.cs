using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Orion.Engine.Gui2
{
    partial class CheckBox
    {
        public sealed class ChildCollection : ICollection<UIElement>
        {
            #region Fields
            private readonly CheckBox checkBox;
            #endregion

            #region Constructors
            internal ChildCollection(CheckBox checkBox)
            {
                Argument.EnsureNotNull(checkBox, "checkBox");

                this.checkBox = checkBox;
            }
            #endregion

            #region Properties
            public int Count
            {
                get { return checkBox.content == null ? 1 : 2; }
            }
            #endregion

            #region Methods
            public void Add(UIElement item)
            {
                Argument.EnsureNotNull(item, "item");
                if (checkBox.content != null) throw new InvalidOperationException("Cannot add an element to a check box which already has some content.");

                checkBox.Content = item;
            }

            public void Clear()
            {
                checkBox.Content = null;
            }

            public bool Contains(UIElement item)
            {
                return item != null && (item == checkBox.button || item == checkBox.content);
            }

            public bool Remove(UIElement item)
            {
                if (item == null) return false;

                if (item == checkBox.content)
                {
                    checkBox.Content = null;
                    return true;
                }

                return false;
            }

            public IEnumerator<UIElement> GetEnumerator()
            {
                yield return checkBox.button;
                if (checkBox.content != null) yield return checkBox.content;
            }
            #endregion

            #region Explicit Members
            void ICollection<UIElement>.CopyTo(UIElement[] array, int arrayIndex)
            {
                array[arrayIndex] = checkBox.button;
                if (checkBox.content != null) array[arrayIndex + 1] = checkBox.content;
            }

            bool ICollection<UIElement>.IsReadOnly
            {
                get { return false; }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
            #endregion
        }
    }
}
