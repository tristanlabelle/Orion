using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Commandment
{
    /// <summary>
    /// Marks a <see cref="Command"/>-derived class as being serializable.
    /// </summary>
    public sealed class SerializableCommandAttribute : Attribute
    {
        #region Fields
        private readonly byte id;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="SerializableCommandAttribute"/>
        /// from the id assigned to the <see cref="Command"/>.
        /// </summary>
        /// <param name="id">The id of the <see cref="Command"/>.</param>
        public SerializableCommandAttribute(byte id)
        {
            this.id = id;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the id of this <see cref="Command"/>.
        /// </summary>
        public byte ID
        {
            get { return id; }
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return "Command #{0}".FormatInvariant(id);
        }
        #endregion
    }
}
