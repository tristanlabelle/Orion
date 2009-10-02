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

        /// <summary>
        /// Gets the time, in seconds, needed to create <see cref="Unit"/>s
        /// of this <see cref="UnitType"/>.
        /// </summary>
        public float CreationTime
        {
            get { return 5; }
        }

        /// <summary>
        /// Gets the maximum amount of health points a unit of this type can have.
        /// </summary>
        public float MaxHealth
        {
            get { return 10; }
        }

        /// <summary>
        /// Gets the speed at which <see cref="Unit"/>s with this <see cref="UnitType"/> move.
        /// </summary>
        public float MovementSpeed
        {
            get { return 20; }
        }
        /// <summary>
        /// Gets the distance at which <see cref="Unit"/>s with this  <see cref="UnitType"/> can reach the enemy to attack him.
        /// </summary>
        public float AttackRange
        {
            get { return 2; }
        }
        /// <summary>
        /// Gets the damage at which <see cref="Unit"/>s whit this  <see cref="UnitType"/> can inflict damage to the enemy.
        /// </summary>
        public float AttackDamage
        {
            get { return 1; }
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
