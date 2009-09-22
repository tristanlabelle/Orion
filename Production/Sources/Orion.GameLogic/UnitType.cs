using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.GameLogic
{
    /// <summary>
    /// Describes a type of unit (including buildings and vehicles).
    /// </summary>
    [Serializable]
    public sealed class UnitType
    {
        #region Fields
        private readonly string name;
        private readonly TagSet tags = new TagSet();
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="UnitType"/> from its name.
        /// </summary>
        /// <param name="name">The name of this <see cref="UnitType"/>.</param>
        public UnitType(string name)
        {
            Argument.EnsureNotNullNorBlank(name, "name");
            this.name = name;
        }
        #endregion

        #region Events
        #endregion

        #region Properties
        /// <summary>
        /// Gets the set of this <see cref="UnitType"/>'s tags.
        /// </summary>
        public TagSet Tags
        {
            get { return tags; }
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return name;
        }
        #endregion
    }
}
