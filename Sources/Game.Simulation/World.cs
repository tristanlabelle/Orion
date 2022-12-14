using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Game.Simulation.Pathfinding;
using Orion.Game.Simulation.Technologies;
using Orion.Game.Simulation.Components;

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
        private readonly SpatialManager spatialManager;
        private readonly Pathfinder pathfinder;
        private readonly Random random;
        private readonly int maxFoodAmount;
        private SimulationStep lastSimulationStep;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="World"/>.
        /// </summary>
        /// <param name="size">The size of this world, in tiles.</param>
        /// <param name="random">The random number generator that can be used within the simulation.</param>
        /// <param name="maxFood">The maximum allowed amount of food per faction.</param>
        public World(Size size, Random random, int maxFood)
        {
            Argument.EnsureNotNull(random, "random");

            this.terrain = new Terrain(size);
            this.entities = new EntityCollection(this);
            this.spatialManager = new SpatialManager(size, maxFood * 5);
            this.pathfinder = new Pathfinder(size);
            this.random = random;
            this.maxFoodAmount = maxFood;
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
        /// Raised when an <see cref="Entity"/> hits another one.
        /// </summary>
        public event Action<World, HitEventArgs> HitOccured;

        /// <summary>
        /// Raised when an <see cref="Entity"/> has died.
        /// </summary>
        public event Action<World, Entity> EntityDied;

        /// <summary>
        /// Raised when a building <see cref="Entity"/>'s construction completes.
        /// </summary>
        public event Action<World, Entity> BuildingConstructed;

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

        /// <summary>
        /// Raised when one of this <see cref="World"/>'s <see cref="Faction"/>s change diplomatic stance.
        /// </summary>
        /// <remarks>Convenience aggregator of the <see cref="Faction.DiplomaticStanceChanged"/> event.</remarks>
        public event Action<World, DiplomaticStanceChange> DiplomaticStanceChanged;
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

        public SimulationStep LastSimulationStep
        {
            get { return lastSimulationStep; }
        }

        /// <summary>
        /// Gets the current game time.
        /// </summary>
        public TimeSpan SimulationTime
        {
            get { return lastSimulationStep.Time; }
        }

        /// <summary>
        /// Gets the current game time, in seconds.
        /// </summary>
        public float SimulationTimeInSeconds
        {
            get { return lastSimulationStep.TimeInSeconds; }
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

        /// <summary>
        /// Gets the <see cref="SpatialManager"/> which allows optimized spatial queries
        /// within this world's <see cref="Entity">entities</see> with a <see cref="Spatial"/> component.
        /// </summary>
        public SpatialManager SpatialManager
        {
            get { return spatialManager; }
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

            return IsFree_NoBoundsCheck(point, layer);
        }

        public bool IsFree(Region region, CollisionLayer layer)
        {
            int exclusiveMaxX = region.ExclusiveMaxX;
            int exclusiveMaxY = region.ExclusiveMaxY;

            if (region.MinX < 0 || region.MinY < 0
                || exclusiveMaxX > Width || exclusiveMaxY > Height)
            {
                return false;
            }

            for (int y = region.MinY; y < exclusiveMaxY; ++y)
                for (int x = region.MinX; x < exclusiveMaxX; ++x)
                    if (!IsFree_NoBoundsCheck(new Point(x, y), layer))
                        return false;

            return true;
        }

        private bool IsFree_NoBoundsCheck(Point point, CollisionLayer layer)
        {
            if (layer == CollisionLayer.Ground && terrain[point] != TileType.Walkable) return false;
            return spatialManager.GetGridObstacleAt(point, layer) == null;
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
            faction.DiplomaticStanceChanged += (source, change) => DiplomaticStanceChanged.Raise(this, change);
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

        public bool IsWithinBounds(Region region)
        {
            Region worldRegion = (Region)Size;
            return worldRegion.Contains(region);
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
        /// Updates this <see cref="World"/> and its <see cref="Entity"/>s for a frame.
        /// </summary>
        /// <param name="step">Information on this simulation step.</param>
        public void Update(SimulationStep step)
        {
            lastSimulationStep = step;
            entities.Update(step);
            Updated.Raise(this, step);
        }

        /// <summary>
        /// Gets a hash code based on the state of the world and its contents.
        /// </summary>
        /// <returns>The resulting hash code.</returns>
        public int GetStateHashCode()
        {
            int hashCode = Terrain.Size.GetHashCode();
            foreach (Entity entity in entities)
                hashCode ^= entity.GetStateHashCode();

            return hashCode;
        }

        #region Event Raising and listening
        /// <remarks>Invoked by World.EntityCollection.</remarks>
        private void OnEntityAdded(Entity entity)
        {
            EntityAdded.Raise(this, entity);
        }

        /// <remarks>Invoked by <see cref="World.EntityCollection"/>.</remarks>
        private void OnEntityRemoved(Entity entity)
        {
            EntityRemoved.Raise(this, entity);
        }

        /// <remarks>Invoked by <see cref="Entity"/>.</remarks>
        internal void OnEntityDied(Entity entity)
        {
            EntityDied.Raise(this, entity);
            entities.Remove(entity);

            if (entity.Components.Has<Health>())
                CreateRuins(entity);
        }

        internal void RaiseBuildingConstructed(Entity entity)
        {
            Argument.EnsureNotNull(entity, "entity");

            BuildingConstructed.Raise(this, entity);
        }

        private void CreateRuins(Entity entity)
        {
            Spatial spatial = entity.Spatial;
            if (spatial == null)
            {
                Debug.Fail("Cannot create ruins for an entity without a spatial component.");
                return;
            }

            Entity ruins = entities.CreateEmpty();

            ruins.Components.Add(new Identity(ruins)
            {
                IsSelectable = false,
                Name = entity.Identity.IsBuilding ? "Ruins" : "Skeleton"
            });

            ruins.Components.Add(new Sprite(ruins)
            {
                Rotates = false,
                IsVisibleOnMinimap = false
            });

            ruins.Components.Add(new TimedExistence(ruins)
            {
                LifeSpan = entity.Identity.IsBuilding ? 30 : 120
            });

            ruins.Components.Add(new Spatial(ruins)
            {
                Position = spatial.Position,
                CollisionLayer = CollisionLayer.None,
                Size = spatial.Size
            });

            if (entity.Components.Has<FactionMembership>())
            {
                FactionMembership membership = new FactionMembership(ruins);
                membership.Faction = entity.Components.Get<FactionMembership>().Faction;
                ruins.Components.Add(membership);
            }

            entities.Add(ruins);
        }

        /// <remarks>Invoked by <see cref="Attacker"/>.</remarks>
        internal void RaiseHitOccured(HitEventArgs args)
        {
            HitOccured.Raise(this, args);
        }

        /// <remarks>Invoked by <see cref="Kamikaze"/>.</remarks>
        internal void OnExplosionOccured(Circle circle)
        {
            if (ExplosionOccured != null) ExplosionOccured(this, circle);
        }

        /// <remarks>Invoked by <see cref="Faction"/>.</remarks>
        internal void OnFactionDefeated(Faction faction)
        {
            FactionDefeated.Raise(this, faction);
        }
        #endregion
        #endregion
    }
}
