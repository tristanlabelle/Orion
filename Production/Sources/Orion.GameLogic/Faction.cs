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
        private sealed class UnitCollection : Collection<Unit>
        {
            #region Fields
            private readonly Faction faction;
            #endregion

            #region Constructors
            public UnitCollection(Faction faction)
            {
                Argument.EnsureNotNull(faction, "faction");
                this.faction = faction;
            }
            #endregion

            #region Methods
            protected override void InsertItem(int index, Unit item)
            {
                Argument.EnsureNotNull(item, "item");
                if (item.Faction == faction) return;
                if (item.Faction != null)
                    throw new ArgumentException("Expected a unit without any faction affiliation.", "item");
                if (item.World != faction.World)
                    throw new ArgumentException("Cannot add to a faction a unit from another world.", "item");

                base.InsertItem(index, item);
                item.faction = faction;
            }

            protected override void RemoveItem(int index)
            {
                this[index].faction = null;
                base.RemoveItem(index);
            }

            protected override void ClearItems()
            {
                foreach (Unit unit in this)
                    unit.faction = null;

                base.ClearItems();
            }

            protected override void SetItem(int index, Unit item)
            {
                throw new NotSupportedException();
            }
            #endregion
        }
        #endregion

        #region Instance
        #region Fields
        private readonly World world;
        private readonly string name;
        private readonly Color color;
        private readonly UnitCollection units;
        private int aladdiumAmount;
        private int allageneAmount;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="Faction"/> from its name and <see cref="Color"/>.
        /// </summary>
        /// <param name="world">The <see cref="World"/> hosting this <see cref="Faction"/>.</param>
        /// <param name="name">The name of this <see cref="Faction"/>.</param>
        /// <param name="color">The distinctive <see cref="Color"/> of this <see cref="Faction"/>'s units.</param>
        public Faction(World world, string name, Color color)
        {
            Argument.EnsureNotNull(world, "world");
            Argument.EnsureNotNullNorBlank(name, "name");

            this.world = world;
            this.name = name;
            this.color = color;
            this.units = new UnitCollection(this);
        }
        #endregion

        #region Events
        #endregion

        #region Properties
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
        /// Accesses the amount of the allagene resource that this <see cref="Faction"/> possesses.
        /// </summary>
        public int AllageneAmount
        {
            get { return allageneAmount; }
            set
            {
                Argument.EnsurePositive(value, "AllageneAmount");
                allageneAmount = value;
            }
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return name;
        }
        #endregion
        #endregion
    }
}
