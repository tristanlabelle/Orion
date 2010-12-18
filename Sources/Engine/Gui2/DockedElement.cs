using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui2
{
    /// <summary>
    /// Wraps a <see cref="UIElement"/> and its <see cref="Dock"/> value.
    /// </summary>
    public struct DockedElement : IEquatable<DockedElement>
    {
        #region Fields
        public readonly UIElement Element;
        public readonly Dock Dock;
        #endregion

        #region Constructors
        public DockedElement(UIElement element, Dock dock)
        {
            Argument.EnsureNotNull(element, "element");
            Argument.EnsureDefined(dock, "dock");

            this.Element = element;
            this.Dock = dock;
        }
        #endregion

        #region Properties
        #endregion

        #region Methods
        /// <summary>
        /// Tests for equality with another instance.
        /// </summary>
        /// <param name="other">The instance to be tested with.</param>
        /// <returns>True they are equal, false otherwise.</returns>
        public bool Equals(DockedElement other)
        {
            return Element == other.Element && Dock == other.Dock;
        }

        public override bool Equals(object obj)
        {
            return obj is DockedElement && Equals((UIElement)obj);
        }

        /// <summary>
        /// Tests two instances for equality.
        /// </summary>
        /// <param name="a">The first instance.</param>
        /// <param name="b">The second instance.</param>
        /// <returns>True they are equal, false otherwise.</returns>
        public static bool Equals(DockedElement a, DockedElement b)
        {
            return a.Equals(b);
        }

        public override int GetHashCode()
        {
            return Element.GetHashCode() ^ Dock.GetHashCode();
        }

        public override string ToString()
        {
            return "{0} (Dock: {1})".FormatInvariant(Element, Dock);
        }
        #endregion

        #region Operators
        /// <summary>
        /// Tests two instances for equality.
        /// </summary>
        /// <param name="a">The first instance.</param>
        /// <param name="b">The second instance.</param>
        /// <returns>True they are equal, false otherwise.</returns>
        public static bool operator ==(DockedElement a, DockedElement b)
        {
            return Equals(a, b);
        }

        /// <summary>
        /// Tests two instances for inequality.
        /// </summary>
        /// <param name="a">The first instance.</param>
        /// <param name="b">The second instance.</param>
        /// <returns>True they are different, false otherwise.</returns>
        public static bool operator !=(DockedElement a, DockedElement b)
        {
            return !Equals(a, b);
        }
        #endregion
    }
}
