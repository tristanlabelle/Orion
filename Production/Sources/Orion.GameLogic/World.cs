using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenTK.Math;
using Orion.GameLogic.Pathfinding;
using Orion.GameLogic.Technologies;
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
        private readonly EntityManager entities;
        private readonly UnitTypeRegistry unitTypes = new UnitTypeRegistry();
        private readonly Pathfinder pathfinder;
        private readonly TechnologyTree technologyTree;
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
            entities = new EntityManager(this);
            pathfinder = new Pathfinder();
            technologyTree = new TechnologyTree();
            technologyTree.PopulateWithBaseTechnologies();
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised after this <see cref="World"/> has been updated.
        /// </summary>
        public event GenericEventHandler<World, SimulationStep> Updated;

        private void RaiseUpdated(SimulationStep step)
        {
            var handler = Updated;
            if (handler != null) handler(this, step);
        }

        /// <summary>
        /// Raised when one of this <see cref="World"/>'s <see cref="Faction"/>s has been defeated.
        /// </summary>
        public event GenericEventHandler<World, Faction> FactionDefeated;

        private void RaiseFactionDefeated(Faction faction)
        {
            var handler = FactionDefeated;
            if (handler != null) handler(this, faction);
        }
        #endregion

        #region Properties
        public Terrain Terrain
        {
            get { return terrain; }
        }

        public TechnologyTree TechTree
        {
            get { return technologyTree; }
        }

        /// <summary>
        /// Gets the sequence of <see cref="Faction"/>s part of this <see cref="World"/>.
        /// </summary>
        public IEnumerable<Faction> Factions
        {
            get { return factions; }
        }

        /// <summary>
        /// Gets the <see cref="EntityRegistry"/> containing the <see cref="Entities"/>s of this <see cref="World"/>.
        /// </summary>
        public EntityManager Entities
        {
            get { return entities; }
        }

        public UnitTypeRegistry UnitTypes
        {
            get { return unitTypes; }
        }

        /// <summary>
        /// Gets the size of this <see cref="World"/>, in tiles.
        /// </summary>
        public Size Size
        {
            get { return terrain.Size; }
        }

        /// <summary>
        /// Gets a <see cref="Rectangle"/> that bounds this <see cref="World"/>, in tiles.
        /// </summary>
        public Rectangle Bounds
        {
            get { return new Rectangle(0, 0, Size.Width, Size.Height); }
        }

        public Pathfinder Pathfinder
        {
            get { return pathfinder; }
        }
        #endregion

        #region Methods
        #region Pathfinding
        public bool IsFree(Point point, CollisionLayer layer)
        {
            if (layer == CollisionLayer.Ground && !terrain.IsWalkable(point)) return false;
            return entities.GetEntityAt(point, layer) == null;
        }

        public bool IsFree(Region region, CollisionLayer layer)
        {
            return region.Points.All(point => IsFree(point, layer));
        }

        /// <summary>
        /// Finds a path from a source to a destination.
        /// </summary>
        /// <param name="source">The position where the path should start.</param>
        /// <param name="destinationDistanceEvaluator">
        /// A delegate to a method which evaluates the distance to the destination.
        /// </param>
        /// <param name="isWalkable">A delegate to a method that tests if tiles are walkable.</param>
        /// <returns>The path that was found, or <c>null</c> if there is none.</returns>
        public Path FindPath(Point source, Func<Point, float> destinationDistanceEvaluator,
            Func<Point, bool> isWalkable)
        {
            Argument.EnsureNotNull(isWalkable, "destinationDistanceEvaluator");
            Argument.EnsureNotNull(isWalkable, "isWalkable");

            if (!Bounds.ContainsPoint(source))
                throw new ArgumentOutOfRangeException("source");

            int maxNumberOfNodes = (int)(destinationDistanceEvaluator(source) * 40);
            if (maxNumberOfNodes < 25) maxNumberOfNodes = 100;
            if (maxNumberOfNodes > 5000) maxNumberOfNodes = 5000;

            return pathfinder.Find(source, destinationDistanceEvaluator, isWalkable, maxNumberOfNodes);
        }
        #endregion

        #region Factions
        /// <summary>
        /// Creates a new <see cref="Faction"/> and adds it to this <see cref="World"/>.
        /// </summary>
        /// <param name="name">The name of the <see cref="Faction"/> to be created.</param>
        /// <param name="color">The <see cref="Color"/> of the <see cref="Faction"/> to be created.</param>
        /// <returns>A newly created <see cref="Faction"/> with that name and color.</returns>
        public Faction CreateFaction(string name, Color color)
        {
            Handle handle = new Handle((uint)factions.Count);
            Faction faction = new Faction(handle, this, name, color);
            faction.Defeated += RaiseFactionDefeated;
            factions.Add(faction);
            faction.AladdiumAmount = 200;
            return faction;
        }

        /// <summary>
        /// Creates a new <see cref="Faction"/> that will not interact with the game in any way.
        /// For use with a <see cref="Spectator"/>.
        /// </summary>
        /// <returns>A newly created spectator faction.</returns>
        public Faction CreateSpectatorFaction()
        {
            const string spectatorFactionName = "\rSpectator";
            Color spectatorFactionColor = Color.Black;

            Handle handle = new Handle(0xFFFFFFFF);
            Faction faction = new Faction(handle, this, spectatorFactionName, spectatorFactionColor);
            return faction;
        }

        /// <summary>
        /// Gets a <see cref="Faction"/> of this <see cref="World"/> from its unique identifier.
        /// </summary>
        /// <param name="handle">The handle of the <see cref="Faction"/> to be found.</param>
        /// <returns>
        /// The <see cref="Faction"/> with that handle, or <c>null</c> if the identifier is invalid.
        /// </returns>
        public Faction FindFactionFromHandle(Handle handle)
        {
            if (handle.Value < 0 || handle.Value >= factions.Count) return null;
            return factions[(int)handle.Value];
        }
        #endregion

        public bool IsWithinBounds(Point point)
        {
            Region region = (Region)Size;
            return region.Contains(point);
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
            else if (x >= Size.Width) x = Size.Width - 1;

            if (y < 0) y = 0;
            else if (y >= Size.Height) y = Size.Height - 1;

            return new Point16((short)x, (short)y);
        }

        /// <summary>
        /// Updates this <see cref="World"/> and its <see cref="Unit"/>s for a frame.
        /// </summary>
        /// <param name="step">Information on this simulation step.</param>
        public void Update(SimulationStep step)
        {
            entities.Update(step);
            RaiseUpdated(step);
        }
        #endregion
    }
}
