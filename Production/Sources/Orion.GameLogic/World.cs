using System;
using System.Collections.Generic;
using OpenTK.Math;
using Orion.GameLogic.Pathfinding;
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
        private readonly Terrain terrain;
        private readonly List<Faction> factions = new List<Faction>();
        private readonly UnitRegistry units;
        private readonly UnitTypeRegistry unitTypes;
        private List<ResourceNode> resourceNodes = new List<ResourceNode>();
        readonly Pathfinder pathFinder;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="World"/>.
        /// </summary>
        /// <param name="terrain">The <see cref="Terrain"/> of this world.</param>
        public World(Terrain terrain)
        {
            Argument.EnsureNotNull(terrain, "terrain");
            this.terrain = terrain;
            units = new UnitRegistry(this, 5, 5);
            unitTypes = new UnitTypeRegistry();
            pathFinder = new Pathfinder(this);
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

        public UnitTypeRegistry UnitTypes
        {
            get { return unitTypes; }
        }

        public List<ResourceNode> ResourceNodes
        {
            get { return resourceNodes; }
        }

        /// <summary>
        /// Gets the width of this <see cref="World"/>, in tiles.
        /// </summary>
        public int Width
        {
            get { return terrain.Width; }
        }

        /// <summary>
        /// Gets the height of this <see cref="World"/>, in tiles.
        /// </summary>
        public int Height
        {
            get { return terrain.Height; }
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

        public Pathfinder PathFinder
        {
            get { return pathFinder; }
        }

        #endregion

        #region Methods
        #region Factions
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
        #endregion

        public bool IsWithinBounds(Point16 point)
        {
            return point.X >= 0 && point.Y >= 0
                && point.X < Width && point.Y < Height;
        }
        public bool IsWithinBounds(Vector2 point)
        {
            return point.X >= 0 && point.Y >= 0
                && point.X < Width && point.Y < Height;
        }

        /// <summary>
        /// Gets the coordinates of the tile on which a point is, clamped within this <see cref="World"/>'s bounds.
        /// </summary>
        /// <param name="point">A point which's tile coords are to be retrieved.</param>
        /// <returns>The coordinates of the tile on which that point falls.</returns>
        public Point16 GetClampedTileCoordinates(Vector2 point)
        {
            int x = (int)point.X;
            int y = (int)point.Y;

            if (x < 0) x = 0;
            else if (x >= Width) x = Width - 1;

            if (y < 0) y = 0;
            else if (y >= Height) y = Height - 1;

            return new Point16((short)x, (short)y);
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
