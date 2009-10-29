using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

using Color = System.Drawing.Color;

namespace Orion.GameLogic
{
    /// <summary>
    /// Represents a faction, a group of allied units sharing resources and sharing a goal.
    /// </summary>
    [Serializable]
    public sealed class Faction
    {
        #region Nested Types
        /// <summary>
        /// A collection of <see cref="Unit"/>s part of a <see cref="Faction"/>.
        /// </summary>
        public sealed class UnitCollection : ICollection<Unit>
        {
            #region Fields
            private readonly Faction faction;
            #endregion

            #region Constructors
            internal UnitCollection(Faction faction)
            {
                Argument.EnsureNotNull(faction, "faction");
                this.faction = faction;
            }
            #endregion

            #region Properties
            /// <summary>
            /// Gets the number of <see cref="Unit"/>s in this <see cref="Faction"/>.
            /// </summary>
            public int Count
            {
                get { return faction.world.Units.Count(unit => unit.faction == faction); }
            }
            #endregion

            #region Methods
            /// <summary>
            /// Adds a <see cref="Unit"/> to this <see cref="Faction"/>.
            /// </summary>
            /// <param name="unit">The <see cref="Unit"/> to be added.</param>
            public void Add(Unit unit)
            {
                Argument.EnsureNotNull(unit, "unit");
                if (unit.Faction != null)
                {
                    throw new ArgumentException(
                        "Cannot add a unit from another faction to this faction.",
                        "unit");
                }

                unit.faction = faction;
            }

            /// <summary>
            /// Removes all <see cref="Unit"/>s from this <see cref="Faction"/>.
            /// </summary>
            public void Clear()
            {
                foreach (Unit unit in faction.world.Units)
                    if (unit.faction == faction)
                        unit.faction = null;
            }

            /// <summary>
            /// Test if a <see cref="Unit"/> is part of this <see cref="Faction"/>.
            /// </summary>
            /// <param name="unit">The <see cref="Unit"/> to be tested.</param>
            /// <returns><c>True</c> if it is part of this faction, <c>false</c> otherwise.</returns>
            public bool Contains(Unit unit)
            {
                Argument.EnsureNotNull(unit, "unit");
                return unit.faction == faction;
            }

            /// <summary>
            /// Removes a <see cref="Unit"/> from this <see cref="Faction"/>.
            /// </summary>
            /// <param name="unit">The <see cref="Unit"/> to be removed.</param>
            /// <returns>
            /// <c>True</c> if the <see cref="Unit"/> was found and removed, <c>false</c> if it wasn't found.
            /// </returns>
            public bool Remove(Unit unit)
            {
                Argument.EnsureNotNull(unit, "unit");

                if (unit.faction != faction) return false;

                unit.faction = null;
                return true;
            }
            #endregion

            #region ICollection<Unit> Membres
            void ICollection<Unit>.CopyTo(Unit[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            bool ICollection<Unit>.IsReadOnly
            {
                get { return false; }
            }
            #endregion

            #region IEnumerable<Unit> Membres
            /// <summary>
            /// Gets an enumerator over the <see cref="Unit"/>s of this <see cref="Faction"/>.
            /// </summary>
            /// <returns>A new enumerator.</returns>
            public IEnumerator<Unit> GetEnumerator()
            {
                return faction.world.Units.Where(unit => unit.faction == faction).GetEnumerator();
            }
            #endregion

            #region IEnumerable Membres
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
            #endregion
        }
        #endregion

        #region Instance
        #region Fields
        private readonly int id;
        private readonly World world;
        private readonly string name;
        private readonly Color color;
        private readonly UnitCollection units;
        private int aladdiumAmount;
        private int alageneAmount;
        private FogOfWar fog;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="Faction"/> from its name and <see cref="Color"/>.
        /// </summary>
        /// <param name="id">The unique identifier of this <see cref="Faction"/>.</param>
        /// <param name="world">The <see cref="World"/> hosting this <see cref="Faction"/>.</param>
        /// <param name="name">The name of this <see cref="Faction"/>.</param>
        /// <param name="color">The distinctive <see cref="Color"/> of this <see cref="Faction"/>'s units.</param>
        internal Faction(int id, World world, string name, Color color)
        {
            Argument.EnsureNotNull(world, "world");
            Argument.EnsureNotNullNorBlank(name, "name");

            this.id = id;
            this.world = world;
            this.name = name;
            this.color = color;
            this.units = new UnitCollection(this);
            this.fog = new FogOfWar(world.Width, world.Height, this);
        }
        #endregion

        #region Events
        #endregion

        #region Properties
        /// <summary>
        /// Gets the unique identifier of this <see cref="Faction"/>.
        /// </summary>
        public int ID
        {
            get { return id; }
        }

        /// <summary>
        /// Gets the <see cref="World"/> which hosts this faction.
        /// </summary>
        public World World
        {
            get { return world; }
        }

        /// <summary>
        /// Gets the name of this <see cref="Faction"/>.
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Gets the <see cref="Color"/> used to visually identify units of this <see cref="Faction"/>.
        /// </summary>
        public Color Color
        {
            get { return color; }
        }

        /// <summary>
        /// Gets the collection of <see cref="Unit"/>s in this <see cref="Faction"/>.
        /// </summary>
        public ICollection<Unit> Units
        {
            get { return units; }
        }

        /// <summary>
        /// Accesses the amount of the aladdium resource that this <see cref="Faction"/> possesses.
        /// </summary>
        public int AladdiumAmount
        {
            get { return aladdiumAmount; }
            set
            {
                Argument.EnsurePositive(value, "AladdiumAmount");
                aladdiumAmount = value;
            }
        }

        /// <summary>
        /// Accesses the amount of the alagene resource that this <see cref="Faction"/> possesses.
        /// </summary>
        public int AlageneAmount
        {
            get { return alageneAmount; }
            set
            {
                Argument.EnsurePositive(value, "AllageneAmount");
                alageneAmount = value;
            }
        }
        #endregion

        #region Methods
        public int GetStat(UnitType type, UnitStat stat)
        {
            Argument.EnsureNotNull(type, "type");
            return type.GetBaseStat(stat);
        }

        /// <summary>
        /// Creates new <see cref="Unit"/> part of this <see cref="Faction"/>.
        /// </summary>
        /// <param name="type">The <see cref="UnitType"/> of the <see cref="Unit"/> to be created.</param>
        /// <returns>The newly created <see cref="Unit"/>.</returns>
        public Unit CreateUnit(UnitType type)
        {
            Unit unit = world.Units.Create(type, this);
            unit.Moved += new ValueChangedEventHandler<Unit, OpenTK.Math.Vector2>(fog.UpdateUnitSight);
            return unit;
        }

        public override string ToString()
        {
            return name;
        }
        #endregion
        #endregion
    }
}
