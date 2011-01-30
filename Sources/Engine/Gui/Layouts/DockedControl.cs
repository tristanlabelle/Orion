using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui
{
    /// <summary>
    /// Wraps a <see cref="Control"/> and its <see cref="Dock"/> value.
    /// </summary>
    public struct DockedControl : IEquatable<DockedControl>
    {
        #region Fields
        public readonly Control Control;
        public readonly Direction Dock;
        #endregion

        #region Constructors
        public DockedControl(Control control, Direction dock)
        {
            Argument.EnsureNotNull(control, "control");
            Argument.EnsureDefined(dock, "dock");

            this.Control = control;
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
        public bool Equals(DockedControl other)
        {
            return Control == other.Control && Dock == other.Dock;
        }

        public override bool Equals(object obj)
        {
            return obj is DockedControl && Equals((Control)obj);
        }

        /// <summary>
        /// Tests two instances for equality.
        /// </summary>
        /// <param name="a">The first instance.</param>
        /// <param name="b">The second instance.</param>
        /// <returns>True they are equal, false otherwise.</returns>
        public static bool Equals(DockedControl a, DockedControl b)
        {
            return a.Equals(b);
        }

        public override int GetHashCode()
        {
            return Control.GetHashCode() ^ Dock.GetHashCode();
        }

        public override string ToString()
        {
            return "{0} (Dock: {1})".FormatInvariant(Control, Dock);
        }
        #endregion

        #region Operators
        /// <summary>
        /// Tests two instances for equality.
        /// </summary>
        /// <param name="a">The first instance.</param>
        /// <param name="b">The second instance.</param>
        /// <returns>True they are equal, false otherwise.</returns>
        public static bool operator ==(DockedControl a, DockedControl b)
        {
            return Equals(a, b);
        }

        /// <summary>
        /// Tests two instances for inequality.
        /// </summary>
        /// <param name="a">The first instance.</param>
        /// <param name="b">The second instance.</param>
        /// <returns>True they are different, false otherwise.</returns>
        public static bool operator !=(DockedControl a, DockedControl b)
        {
            return !Equals(a, b);
        }
        #endregion
    }
}
