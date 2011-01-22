using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Orion.Engine
{
    /// <summary>
    /// Represents a range of characters within a <see cref="BaseString"/>.
    /// </summary>
    [Serializable]
    [ImmutableObject(true)]
    public struct Substring
    {
        #region Fields
        private readonly string baseString;
        private readonly int startIndex;
        private readonly int length;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="Substring"/> which represents the whole range of a <see cref="T:String"/>.
        /// </summary>
        /// <param name="baseString">The <see cref="T:String"/> to be wrapped.</param>
        public Substring(string baseString)
        {
            Argument.EnsureNotNull(baseString, "baseString");

            this.baseString = baseString;
            this.startIndex = 0;
            this.length = baseString.Length;
        }

        /// <summary>
        /// Initializes a new <see cref="Substring"/> from a range of characters in a <see cref="T:String"/>.
        /// </summary>
        /// <param name="baseString">The <see cref="T:String"/> to be wrapped.</param>
        /// <param name="startIndex">
        /// The index of the first character of the <see cref="Substring"/> in <paramref name="baseString"/>.
        /// </param>
        public Substring(string baseString, int startIndex)
        {
            Argument.EnsureNotNull(baseString, "baseString");
            Argument.EnsureWithin(startIndex, 0, baseString.Length, "startIndex");

            this.baseString = baseString;
            this.startIndex = startIndex;
            this.length = baseString.Length - startIndex;
        }

        /// <summary>
        /// Initializes a new <see cref="Substring"/> from a range of characters in a <see cref="T:String"/>.
        /// </summary>
        /// <param name="baseString">The <see cref="T:String"/> to be wrapped.</param>
        /// <param name="startIndex">
        /// The index of the first character of the <see cref="Substring"/> in <paramref name="baseString"/>.
        /// </param>
        /// <param name="length">The length of the <see cref="Substring"/>.</param>
        public Substring(string baseString, int startIndex, int length)
        {
            Argument.EnsureNotNull(baseString, "baseString");
            Argument.EnsureWithin(startIndex, 0, baseString.Length, "startIndex");
            Argument.EnsureWithin(startIndex, 0, baseString.Length - startIndex, "length");

            this.baseString = baseString;
            this.startIndex = startIndex;
            this.length = length;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the <see cref="T:String"/> for which this <see cref="Substring"/> represents a range.
        /// </summary>
        public string BaseString
        {
            get { return baseString ?? string.Empty; }
        }

        /// <summary>
        /// Gets the index of the first character of this <see cref="Substring"/>
        /// within its wrapped <see cref="T:String"/>.
        /// </summary>
        public int StartIndex
        {
            get { return startIndex; }
        }

        /// <summary>
        /// Gets the length of this <see cref="Substring"/>, in characters.
        /// </summary>
        public int Length
        {
            get { return length; }
        }
        #endregion

        #region Indexers
        /// <summary>
        /// Gets a character within this <see cref="Substring"/> by its index.
        /// </summary>
        /// <param name="index">The index of the character.</param>
        /// <returns>The character at that index.</returns>
        public char this[int index]
        {
            get { return BaseString[startIndex + index]; }
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return length == 0 ? string.Empty : baseString.Substring(startIndex, length);
        }
        #endregion

        #region Operators
        /// <summary>
        /// Implicitely converts a <see cref="T:String"/> to its equivalent <see cref="T:Substring"/>.
        /// </summary>
        /// <param name="string">The <see cref="T:String"/> to be wrapped.</param>
        /// <returns>The resulting <see cref="T:Substring"/>.</returns>
        public static implicit operator Substring(string @string)
        {
            return new Substring(@string);
        }

        /// <summary>
        /// Implicitely converts a <see cref="T:Substring"/> a <see cref="T:String"/>.
        /// </summary>
        /// <param name="substring">The <see cref="T:Substring"/> to be converted.</param>
        /// <returns>The resulting <see cref="T:String"/>.</returns>
        public static implicit operator string(Substring substring)
        {
            return substring.ToString();
        }
        #endregion
    }
}
