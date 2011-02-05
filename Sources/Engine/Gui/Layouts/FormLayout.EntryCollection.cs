using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Orion.Engine.Gui
{
    partial class FormLayout
    {
        /// <summary>
        /// The collection of entries in the <see cref="FormLayout."/>
        /// </summary>
        public sealed class EntryCollection : Collection<FormLayoutEntry>
        {
            #region Fields
            private readonly FormLayout form;
            #endregion

            #region Constructors
            internal EntryCollection(FormLayout form)
            {
                this.form = form;
            }
            #endregion

            #region Methods
            /// <summary>
            /// Adds a new entry with a specified header and content.
            /// </summary>
            /// <param name="header">The header of this entry, or <c>null</c> if no header is desired.</param>
            /// <param name="content">The content of this entry, or <c>null</c> if no content is desired.</param>
            public void Add(Control header, Control content)
            {
                Add(new FormLayoutEntry(header, content));
            }

            protected override void SetItem(int index, FormLayoutEntry item)
            {
                RemoveItem(index);
                InsertItem(index, item);
            }

            protected override void RemoveItem(int index)
            {
                FormLayoutEntry entry = this[index];
                base.RemoveItem(index);

                if (entry.Header != null) form.AbandonChild(entry.Header);
                if (entry.Content != null) form.AbandonChild(entry.Content);
                form.InvalidateMeasure();
            }

            protected override void InsertItem(int index, FormLayoutEntry item)
            {
                if (item.Header != null) form.AdoptChild(item.Header);
                if (item.Content != null) form.AdoptChild(item.Content);
                base.InsertItem(index, item);

                form.InvalidateMeasure();
            }

            protected override void ClearItems()
            {
                while (Count > 0) RemoveItem(Count - 1);
            }
            #endregion
        }
    }
}
