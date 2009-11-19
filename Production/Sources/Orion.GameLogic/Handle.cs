using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Orion.GameLogic
{
    /// <summary>
    /// Encapsulates a handle to a game object.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Size = sizeof(uint))]
    public struct Handle : IEquatable<Handle>
    {
        #region Instance
        #region Fields
        private readonly uint value;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="Handle"/> from its value.
        /// </summary>
        /// <param name="value">The value of the handle.</param>
        public Handle(uint value)
        {
            this.value = value;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the value of this <see cref="Uid"/>.
        /// </summary>
        public uint Value
        {
            get { return value; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Tests for equality with another <see cref="Uid"/>.
        /// </summary>
        /// <param name="other">The <see cref="Uid"/> to compare with.</param>
        /// <returns><c>True</c> if they are equal, <c>false</c> if not.</returns>
        public bool Equals(Handle other)
        {
            return value == other.value;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Handle)) return false;
            return Equals((Handle)obj);
        }

        public override int GetHashCode()
        {
            return (int)value;
        }

        public override string ToString()
        {
            return "#{0}".FormatInvariant(value);
        }
        #endregion
        #endregion

        #region Static
        #region Methods
        /// <summary>
        /// Compares two <see cref="Uid"/>s for equality.
        /// </summary>
        /// <param name="first">The first <see cref="Uid"/>.</param>
        /// <param name="first">The second <see cref="Uid"/>.</param>
        /// <returns><c>True</c> if they are equal, <c>false</c> if not.</returns>
        public static bool Equals(Handle first, Handle second)
        {
            return first.Equals(second);
        }
        #endregion

        #region Operators
        public static bool operator ==(Handle lhs, Handle rhs)
        {
            return Equals(lhs, rhs);
        }

        public static bool operator !=(Handle lhs, Handle rhs)
        {
            return !Equals(lhs, rhs);
        }
        #endregion
        #endregion
    }
}
