using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui2
{
    /// <summary>
    /// A layout <see cref="Control"/> which arranges its children as a form,
    /// with headers (typically text) next to fields.
    /// </summary>
    public sealed partial class FormLayout : Control
    {
        #region Fields
        private readonly EntryCollection entries;
        private int headerContentGap;
        private int entryGap;
        private int minEntrySize;
        private int cachedHeaderColumnWidth;
        #endregion

        #region Constructors
        public FormLayout()
        {
            entries = new EntryCollection(this);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the collection of entries in this <see cref="FormLayout"/>.
        /// </summary>
        public EntryCollection Entries
        {
            get { return entries; }
        }

        /// <summary>
        /// Accesses the size of the gap between the header and its content control, in pixels.
        /// </summary>
        public int HeaderContentGap
        {
            get { return headerContentGap; }
            set
            {
                if (value == headerContentGap) return;
                Argument.EnsurePositive(value, "HeaderContentGap");

                headerContentGap = value;
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Accesses the size of the gap between successive entries, in pixels.
        /// </summary>
        public int EntryGap
        {
            get { return entryGap; }
            set
            {
                if (value == entryGap) return;
                Argument.EnsurePositive(value, "EntryGap");

                entryGap = value;
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Accesses the minimum size of a form entry, in pixels.
        /// </summary>
        public int MinEntrySize
        {
            get { return minEntrySize; }
            set
            {
                if (value == minEntrySize) return;
                Argument.EnsurePositive(value, "MinEntrySize");

                minEntrySize = value;
                InvalidateMeasure();
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds an entry with a header and a content control to this <see cref="FormLayout"/>.
        /// </summary>
        /// <param name="header">The header control of the entry.</param>
        /// <param name="content">The content control of the entry.</param>
        public void AddEntry(Control header, Control content)
        {
            entries.Add(header, content);
        }

        protected override IEnumerable<Control> GetChildren()
        {
            foreach (FormLayoutEntry entry in entries)
            {
                if (entry.Header != null) yield return entry.Header;
                if (entry.Content != null) yield return entry.Content;
            }
        }

        protected override Size MeasureSize(Size availableSize)
        {
            cachedHeaderColumnWidth = 0;
            int contentColumnWidth = 0;
            int height = 0;

            for (int i = 0; i < entries.Count; ++i)
            {
                if (i > 0) height += entryGap;

                FormLayoutEntry entry = entries[i];
                int entryHeight = 0;

                if (entry.Header != null)
                {
                    Size headerSize = entry.Header.Measure(Size.MaxValue);
                    entryHeight = headerSize.Height;
                    if (headerSize.Width > cachedHeaderColumnWidth) cachedHeaderColumnWidth = headerSize.Width;
                }

                if (entry.Content != null)
                {
                    Size contentSize = entry.Content.Measure(Size.MaxValue);
                    if (entryHeight < contentSize.Height) entryHeight = contentSize.Height;
                    if (contentSize.Width > contentColumnWidth) contentColumnWidth = contentSize.Width;
                }

                height += entryHeight;
            }

            return new Size(cachedHeaderColumnWidth + headerContentGap + contentColumnWidth, height);
        }

        protected override void ArrangeChildren()
        {
            Region rectangle = Rectangle;

            int y = 0;
            for (int i = 0; i < entries.Count; ++i)
            {
                if (i > 0) y += entryGap;

                FormLayoutEntry entry = entries[i];
                int entryHeight = minEntrySize;

                if (entry.Header != null)
                {
                    Size headerSize = entry.Header.DesiredOuterSize;
                    if (headerSize.Height > entryHeight) entryHeight = headerSize.Height;
                }

                if (entry.Content != null)
                {
                    Size contentSize = entry.Content.DesiredOuterSize;
                    if (contentSize.Height > entryHeight) entryHeight = contentSize.Height;
                }

                if (entry.Header != null)
                {
                    Region headerRectangle = new Region(rectangle.MinX, rectangle.MinY + y, cachedHeaderColumnWidth, entryHeight);
                    DefaultArrangeChild(entry.Header, headerRectangle);
                }

                if (entry.Content != null)
                {
                    Region contentRectangle = new Region(
                        rectangle.MinX + cachedHeaderColumnWidth + headerContentGap, rectangle.MinY + y,
                        Math.Max(0, rectangle.Width - cachedHeaderColumnWidth - headerContentGap), entryHeight);
                    DefaultArrangeChild(entry.Content, contentRectangle);
                }

                y += entryHeight;
            }
        }
        #endregion
    }
}
