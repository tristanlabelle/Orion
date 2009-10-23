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
		public static readonly IEnumerable<UnitType> AllTypes = new UnitType[] { new UnitType("Archer"), new UnitType("Tank"), new UnitType("Jedi"), new UnitType("Building") };
		
        private readonly string name;
        private readonly TagSet tags = new TagSet();
        private readonly Dictionary<UnitStat, int> baseStats = new Dictionary<UnitStat, int>();
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

            // Those can't logically be zero.
            baseStats[UnitStat.MaxHealth] = 1;
            baseStats[UnitStat.SightRange] = 1;

            // Temporarly hard-coded for backward compatibility.
            baseStats[UnitStat.CreationSpeed] = 2;
            baseStats[UnitStat.MaxHealth] = 10;
            baseStats[UnitStat.MovementSpeed] = 20;
            baseStats[UnitStat.AttackRange] = 2;
            baseStats[UnitStat.SightRange] = 5;
            baseStats[UnitStat.AladdiumCost] = 50;
        }
        #endregion

        #region Events
        #endregion

        #region Properties
        /// <summary>
        /// Gets the name of this <see cref="UnitType"/>.
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Gets the set of tags associated with this <see cref="UnitType"/>.
        /// </summary>
        public TagSet Tags
        {
            get { return tags; }
        }

        public bool IsBuilding
        {
            get { return name == "Building"; }
        }
        #endregion

        #region Methods
        public int GetBaseStat(UnitStat stat)
        {
            int value;
            baseStats.TryGetValue(stat, out value);
            return value;
        }

        public void SetBaseStat(UnitStat stat, int value)
        {
            baseStats[stat] = value;
        }

        public override string ToString()
        {
            return name;
        }
        #endregion
    }
}
