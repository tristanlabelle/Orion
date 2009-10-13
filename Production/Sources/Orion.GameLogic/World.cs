using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

using Orion.Geometry;
using Orion.Core;

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
        private readonly UnitRegistry units;
        private List<RessourceNode> ressourceNodes = new List<RessourceNode>();
        private List<Building> buildings = new List<Building>();
        private Terrain terrain;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="World"/>.
        /// </summary>
        public World()
        {
            units = new UnitRegistry(this, 5, 5);
            this.terrain = TerrainGenerator.GenerateNewTerrain(this.Width, this.Height, new MersenneTwister());
        }
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
        /// Gets the <see cref="UnitRegistry"/> containing the <see cref="Unit"/>s of this <see cref="World"/>.
        /// </summary>
        public UnitRegistry Units
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

        public Terrain Terrain
        {
            get { return terrain; }
        }
        #endregion

        #region Methods
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
        /// Updates this <see cref="World"/> and its <see cref="Unit"/>s for a frame.
        /// </summary>
        /// <param name="timeDeltaInSeconds">The time elapsed since the last frame, in seconds.</param>
        public void Update(float timeDeltaInSeconds)
        {
            units.Update(timeDeltaInSeconds);
        }
        #endregion
    }
}
