using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Orion.GameLogic
{
    /// <summary>
    /// A collection storing a set of tags.
    /// </summary>
    public sealed class TagSet : ICollection<string>
    {
        #region Instance
        #region Fields
        private readonly HashSet<string> tags = new HashSet<string>();
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new empty <see cref="TagSet"/>.
        /// </summary>
        public TagSet() { }

        /// <summary>
        /// Initializes a new <see cref="TagSet"/> from a sequence of initial tags.
        /// </summary>
        /// <param name="tags">The tags to be initially included in the <see cref="TagSet"/>.</param>
        public TagSet(IEnumerable<string> tags)
        {
            Argument.EnsureNotNull(tags, "tags");
            foreach (string tag in tags)
                Add(tag);
        }

        /// <summary>
        /// Initializes a new <see cref="TagSet"/> from an array of initial tags.
        /// </summary>
        /// <param name="tags">The tags to be initially included in the <see cref="TagSet"/>.</param>
        public TagSet(params string[] tags)
            : this((IEnumerable<string>)tags) { }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the number of tags in this <see cref="TagSet"/>.
        /// </summary>
        public int Count
        {
            get { return tags.Count; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds a tag to this <see cref="TagSet"/>.
        /// </summary>
        /// <param name="tag">The tag to be added.</param>
        /// <returns><c>True</c> if the tag was added, <c>false</c> if it was already present.</returns>
        public bool Add(string tag)
        {
            Argument.EnsureNotNullNorBlank(tag, "tag");
            return tags.Add(tag.Trim().ToLowerInvariant());
        }

        /// <summary>
        /// Gets an enumerator that iterates over the tags of this <see cref="TagSet"/>.
        /// </summary>
        /// <returns>A new enumerator.</returns>
        public HashSet<string>.Enumerator GetEnumerator()
        {
            return tags.GetEnumerator();
        }

        /// <summary>
        /// Tests if this <see cref="TagSet"/> contains a given tag.
        /// </summary>
        /// <param name="tag">The tag to be found.</param>
        /// <returns>
        /// <c>True</c> if <paramref name="tag"/> was found within whis <see cref="Tagset"/>,
        /// <c>false</c> if it wasn't.
        /// </returns>
        public bool Contains(string tag)
        {
            if (tag == null) return false;
            return tags.Contains(tag.Trim().ToLowerInvariant());
        }

        /// <summary>
        /// Removes a given tag from this <see cref="TagSet"/>.
        /// </summary>
        /// <param name="tag">The tag to be removed.</param>
        /// <returns><c>True</c> if it was found and removed, <c>false</c> if it wasn't found.</returns>
        public bool Remove(string tag)
        {
            if (tag == null) return false;
            return tags.Remove(tag.Trim().ToLowerInvariant());
        }

        /// <summary>
        /// Removes all tags from this <see cref="TagSet"/>.
        /// </summary>
        public void Clear()
        {
            tags.Clear();
        }
        #endregion

        #region Explicit Members
        #region ICollection<string> Members
        void ICollection<string>.Add(string item)
        {
            Add(item);
        }

        void ICollection<string>.CopyTo(string[] array, int arrayIndex)
        {
            tags.CopyTo(array, arrayIndex);
        }

        bool ICollection<string>.IsReadOnly
        {
            get { return false; }
        }
        #endregion

        #region IEnumerable<string> Members
        IEnumerator<string> IEnumerable<string>.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region IEnumerable Members
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
        #endregion
        #endregion

        #region Static
        #region Methods
        private static bool IsValidTag(string tag)
        {
            return tag != null && tag.Any(c => !char.IsWhiteSpace(c));
        }
        #endregion
        #endregion
    }
}
