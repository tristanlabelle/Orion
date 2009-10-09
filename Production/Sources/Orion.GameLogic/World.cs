using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

using Orion.Geometry;

using Color = System.Drawing.Color;

namespace Orion.GameLogic
{
    /// <summary>
    /// Represents the game map: its terrain and units.
    /// </summary>
    [Serializable]
    public sealed class World
    {
        #region Fields
        private readonly List<Faction> factions = new List<Faction>();
        private readonly List<Unit> units = new List<Unit>();
        private readonly List<Unit> deadUnits = new List<Unit>();
        private readonly GenericEventHandler<Unit> unitDiedEventHandler;
        private int nextUnitID = 0;
        private List<RessourceNode> ressourceNodes = new List<RessourceNode>();
        private List<Building> buildings = new List<Building>();
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="World"/>.
        /// </summary>
        public World()
        {
            unitDiedEventHandler = OnUnitDied;
        }
        #endregion

        #region Events
        #endregion

        #region Properties
        /// <summary>
        /// Gets the sequence of <see cref="Faction"/>s part of this <see cref="World"/>.
        /// </summary>
        public IEnumerable<Faction> Factions
        {
            get { return factions; }
        }

        /// <summary>
        /// Gets the sequence of <see cref="Unit"/>s part of this <see cref="World"/>.
        /// </summary>
        public IEnumerable<Unit> Units
        {
            get { return units; }
        }

        public List<RessourceNode> RessourceNodes
        {
            get { return ressourceNodes; }
        }

        public List<Building> Buildings
        {
            get { return buildings; }
        }

        /// <summary>
        /// Gets the width of this <see cref="World"/>, in tiles.
        /// </summary>
        public int Width
        {
            get
            {
                // To be later replaced by Terrain.Width.
                return 100;
            }
        }

        /// <summary>
        /// Gets the height of this <see cref="World"/>, in tiles.
        /// </summary>
        public int Height
        {
            get
            {
                // To be later replaced by Terrain.Width.
                return 100;
            }
        }

        /// <summary>
        /// Gets a <see cref="Rectangle"/> that bounds this <see cref="World"/>, in tiles.
        /// </summary>
        public Rectangle Bounds
        {
            get { return new Rectangle(0, 0, Width, Height); }
        }
        #endregion

        #region Methods
        private void OnUnitDied(Unit unit)
        {
            Argument.EnsureNotNull(unit, "unit");
            deadUnits.Add(unit);
        }

        /// <summary>
        /// Gets a <see cref="Faction"/> of this <see cref="World"/> from its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the <see cref="Faction"/> to be found.</param>
        /// <returns>
        /// The <see cref="Faction"/> with that identifier, or <c>null</c> if the identifier is invalid.
        /// </returns>
        public Faction FindFactionWithID(int id)
        {
            if (id < 0 || id >= factions.Count) return null;
            return factions[id];
        }

        /// <summary>
        /// Gets a <see cref="Unit"/> of this <see cref="World"/> from its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the <see cref="Unit"/> to be found.</param>
        /// <returns>
        /// The <see cref="Unit"/> with that identifier, or <c>null</c> if the identifier is invalid.
        /// </returns>
        public Unit FindUnitWithID(int id)
        {
            for (int i = 0; i < units.Count; ++i)
                if (units[i].ID == id)
                    return units[i];

            return null;
        }

        /// <summary>
        /// Creates a new <see cref="Faction"/> and adds it to this <see cref="World"/>.
        /// </summary>
        /// <param name="name">The name of the <see cref="Faction"/> to be created.</param>
        /// <param name="color">The <see cref="Color"/> of the <see cref="Faction"/> to be created.</param>
        /// <returns>A newly created <see cref="Faction"/> with that name and color.</returns>
        public Faction CreateFaction(string name, Color color)
        {
            Faction faction = new Faction(factions.Count, this, name, color);
            factions.Add(faction);
            return faction;
        }

        /// <summary>
        /// Used by <see cref="Faction"/> to create new <see cref="Unit"/>
        /// from its <see cref="UnitType"/> and <see cref="Faction"/>.
        /// </summary>
        /// <param name="type">The <see cref="UnitType"/> of the <see cref="Unit"/> to be created.</param>
        /// <param name="faction">The <see cref="Faction"/> which creates the <see cref="Unit"/>.</param>
        /// <returns>The newly created <see cref="Unit"/>.</returns>
        internal Unit CreateUnit(UnitType type, Faction faction)
        {
            Argument.EnsureNotNull(type, "type");
            Argument.EnsureNotNull(faction, "faction");

            Unit unit = new Unit(nextUnitID++, type, faction);
            unit.Died += unitDiedEventHandler;

            units.Add(unit);
            return unit;
        }

        /// <summary>
        /// Updates this <see cref="World"/> and its <see cref="Unit"/>s for a frame.
        /// </summary>
        /// <param name="timeDelta">The time elapsed since the last frame.</param>
        public void Update(float timeDelta)
        {
            foreach (Unit unit in Units)
                unit.Update(timeDelta);

            foreach (Unit unit in deadUnits)
                units.Remove(unit);

            deadUnits.Clear();
        }
        #endregion
    }
}
