using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Game.Simulation.Pathfinding;
using Orion.Game.Simulation.Technologies;

namespace Orion.Game.Simulation
{
    /// <summary>
    /// Represents the game map: its terrain and units.
    /// </summary>
    [Serializable]
    public sealed partial class World
    {
        #region Fields
        private const int minimumPathfindingNodeCount = 150;
        private const int maximumPathfindingNodeCount = 5000;

        private readonly Terrain terrain;
        private readonly List<Faction> factions = new List<Faction>();
        private readonly EntityCollection entities;
        private readonly Pathfinder pathfinder;
        private readonly Random random;
        private readonly int maxFoodAmount;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="World"/>.
        /// </summary>
        /// <param name="terrain">The <see cref="Terrain"/> of this world.</param>
        public World(Terrain terrain, Random random, int maxFood)
        {
            Argument.EnsureNotNull(terrain, "terrain");

            this.maxFoodAmount = maxFood;
            this.terrain = terrain;
            this.entities = new EntityCollection(this);
            this.pathfinder = new Pathfinder(terrain.Size);
            this.random = random;
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when an entity gets added to this world.
        /// </summary>
        public event Action<World, Entity> EntityAdded;

        /// <summary>
        /// Raised when an entity has been removed from this world.
        /// </summary>
        public event Action<World, Entity> EntityRemoved;

        /// <summary>
        /// Raised after this <see cref="World"/> has been updated.
        /// </summary>
        public event Action<World, SimulationStep> Updated;

        /// <summary>
        /// Raised when a unit hits another unit.
        /// </summary>
        /// <remarks>
        /// Convenience aggregator of the <see cref="Unit.Hitting"/> event.
        /// </remarks>
        public event Action<World, HitEventArgs> UnitHitting;

        /// <summary>
        /// Raised when an entity has died.
        /// </summary>
        /// <remarks>
        /// Convenience aggregator of the <see cref="Entity.Died"/> event.
        /// </remarks>
        public event Action<World, Entity> EntityDied;

        /// <summary>
        /// Raised when an explosion occurs.
        /// </summary>
        public event Action<World, Circle> ExplosionOccured;

        /// <summary>
        /// Raised when one of this <see cref="World"/>'s <see cref="Faction"/>s has been defeated.
        /// </summary>
        /// <remarks>
        /// Convenience aggregator of the <see cref="Faction.Defeated"/> event.
        /// </remarks>
        public event Action<World, Faction> FactionDefeated;
        #endregion

        #region Properties
        public int MaximumFoodAmount
        {
            get { return maxFoodAmount; }
        }

        public Terrain Terrain
        {
            get { return terrain; }
        }

        /// <summary>
        /// Gets the random number generator used within this world.
        /// </summary>
        internal Random Random
        {
            get { return random; }
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
        public EntityCollection Entities
        {
            get { return entities; }
        }

        #region Size/Bounds
        /// <summary>
        /// Gets the size of this <see cref="World"/>, in tiles.
        /// </summary>
        public Size Size
        {
            get { return terrain.Size; }
        }

        public int Width
        {
            get { return terrain.Size.Width; }
        }

        public int Height
        {
            get { return terrain.Size.Height; }
        }

        /// <summary>
        /// Gets a <see cref="Rectangle"/> that bounds this <see cref="World"/>, in tiles.
        /// </summary>
        public Rectangle Bounds
        {
            get { return new Rectangle(0, 0, Size.Width, Size.Height); }
        }
        #endregion

        public Pathfinder Pathfinder
        {
            get { return pathfinder; }
        }
        #endregion

        #region Methods
        #region Pathfinding
        public bool IsFree(Point point, CollisionLayer layer)
        {
            if (!IsWithinBounds(point))
            {
                Debug.Fail("Testing if an out-of-bounds point is free.");
                return false;
            }
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

            int nodeLimit = (int)(destinationDistanceEvaluator(source) * 40);
            if (nodeLimit < minimumPathfindingNodeCount)
                nodeLimit = minimumPathfindingNodeCount;
            else if (nodeLimit > maximumPathfindingNodeCount)
                nodeLimit = maximumPathfindingNodeCount;

            return pathfinder.Find(source, destinationDistanceEvaluator, isWalkable, nodeLimit);
        }
        #endregion

        #region Factions
        /// <summary>
        /// Creates a new <see cref="Faction"/> and adds it to this <see cref="World"/>.
        /// </summary>
        /// <param name="name">The name of the <see cref="Faction"/> to be created.</param>
        /// <param name="color">The <see cref="Color"/> of the <see cref="Faction"/> to be created.</param>
        /// <returns>A newly created <see cref="Faction"/> with that name and color.</returns>
        public Faction CreateFaction(string name, ColorRgb color)
        {
            Handle handle = new Handle((uint)factions.Count);
            Faction faction = new Faction(handle, this, name, color);
            factions.Add(faction);
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
            ColorRgb spectatorFactionColor = Colors.Black;

            Handle handle = new Handle(0xFFFFFFFF);
            Faction faction = new Faction(handle, this, spectatorFactionName, spectatorFactionColor);
            faction.LocalFogOfWar.Disable();
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

        #region Location
        public Vector2 Clamp(Vector2 destination)
        {
            // Clamp the destination within the world bounds.
            // The world bounds maximums are be exclusive.
            destination = Bounds.Clamp(destination);
            if (destination.X >= Width) destination.X = Width - 0.0001f;
            if (destination.Y >= Size.Height) destination.Y = Height - 0.0001f;
            return destination;
        }

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
        #endregion

        /// <summary>
        /// Updates this <see cref="World"/> and its <see cref="Unit"/>s for a frame.
        /// </summary>
        /// <param name="step">Information on this simulation step.</param>
        public void Update(SimulationStep step)
        {
            entities.Update(step);
            Updated.Raise(this, step);
        }

        #region Event Raising and listening
        /// <remarks>Invoked by World.EntityCollection.</remarks>
        private void OnEntityAdded(Entity entity)
        {
            EntityAdded.Raise(this, entity);
        }

        /// <remarks>Invoked by World.EntityCollection.</remarks>
        private void OnEntityRemoved(Entity entity)
        {
            EntityRemoved.Raise(this, entity);
        }

        /// <remarks>Invoked by Entity.</remarks>
        internal void OnEntityMoved(Entity entity, Vector2 oldPosition, Vector2 newPosition)
        {
            entities.MoveFrom(entity, oldPosition);
        }

        /// <remarks>Invoked by Entity.</remarks>
        internal void OnEntityDied(Entity entity)
        {
            EntityDied.Raise(this, entity);

            bool isEmbarkedUnit = entity is Unit && ((Unit)entity).IsEmbarked;
            if (!isEmbarkedUnit) entities.Remove(entity);
        }

        /// <remarks>Invoked by Unit.</remarks>
        internal void OnUnitHitting(HitEventArgs args)
        {
            UnitHitting.Raise(this, args);
        }

        /// <remarks>Invoked by Unit.</remarks>
        internal void OnExplosionOccured(Circle circle)
        {
            if (ExplosionOccured != null) ExplosionOccured(this, circle);
        }

        /// <remarks>Invoked by Faction.</remarks>
        internal void OnFactionDefeated(Faction faction)
        {
            FactionDefeated.Raise(this, faction);
        }
        #endregion
        #endregion
    }
}
