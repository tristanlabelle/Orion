using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using OpenTK.Math;

using Orion.Geometry;

using Point = System.Drawing.Point;

namespace Orion.GameLogic
{
    /// <summary>
    /// A collection of <see cref="Entity">entities</see> optimized for spatial queries.
    /// </summary>
    [Serializable]
    public sealed class EntityRegistry : IEnumerable<Entity>
    {
        #region Nested Types
        /// <summary>
        /// Represents a rectangular spatial subdivision used to group nearby <see cref="Entity"/>s.
        /// </summary>
        [DebuggerDisplay("Count = {Count}")]
        private struct Zone
        {
            #region Fields
            public Entity[] Units;
            public int Count;
            #endregion

            #region Indexers
            /// <summary>
            /// Gets a <see cref="Entity"/> from this <see cref="Zone"/> by its index.
            /// </summary>
            /// <param name="index">The index of the <see cref="Entity"/> to be retrieved.</param>
            /// <returns>The <see cref="Entity"/> at that index.</returns>
            public Entity this[int index]
            {
                get { return Units[index]; }
            }
            #endregion

            #region Methods
            /// <summary>
            /// Removes a <see cref="Entity"/> from this zone.
            /// </summary>
            /// <param name="item">The <see cref="Entity"/> to be removed.</param>
            /// <returns><c>True</c> if an <see cref="Entity"/> was removed, <c>false</c> if it wasn't found.</returns>
            public bool Remove(Entity entity)
            {
                for (int i = 0; i < Count; ++i)
                {
                    if (Units[i] == entity)
                    {
                        if (i < Count - 1) Units[i] = Units[Count - 1];
                        Units[Count - 1] = null;
                        --Count;
                        return true;
                    }
                }

                return false;
            }

            /// <summary>
            /// Adds a <see cref="Entity"/> to this <see cref="Zone"/>
            /// The <see cref="Entity"/> is assumed to be absent.
            /// </summary>
            /// <param name="item">The <see cref="Entity"/> to be added.</param>
            public void Add(Entity entity)
            {
                if (Units == null) Units = new Entity[16];
                else if (Count == Units.Length)
                {
                    Entity[] newItems = new Entity[Units.Length * 2];
                    Array.Copy(Units, newItems, Units.Length);
                    Units = newItems;
                }

                Units[Count] = entity;
                ++Count;
            }
            #endregion
        }
        #endregion

        #region Instance
        #region Fields
        private readonly World world;
        private readonly List<Entity> entities = new List<Entity>();
        private readonly Zone[,] zones;

        // Temporary collections used to defer modification of "entities"
        private readonly HashSet<Entity> entitiesToAdd = new HashSet<Entity>();
        private readonly Dictionary<Entity, Rectangle> entitiesToMove = new Dictionary<Entity, Rectangle>();
        private readonly HashSet<Entity> entitiesToRemove = new HashSet<Entity>();

        private readonly GenericEventHandler<Entity> entityDiedEventHandler;
        private readonly ValueChangedEventHandler<Entity, Rectangle> entityBoundingRectangleChangedEventHandler;
        private int nextID;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="SpatialCollection{Entity}"/> from the spatial
        /// bounds of the container and its number of subdivision along the axes.
        /// </summary>
        /// <param name="world">
        /// The <see cref="World"/> that to which the <see cref="Entity"/>s in this <see cref="UnitRegistry"/> belong.
        /// </param>
        /// <param name="columnCount">The number of spatial subdivisions along the x axis.</param>
        /// <param name="rowCount">The number of spatial subdivisions along the y axis.</param>
        internal EntityRegistry(World world, int columnCount, int rowCount)
        {
            Argument.EnsureNotNull(world, "world");
            Argument.EnsureStrictlyPositive(columnCount, "columnCount");
            Argument.EnsureStrictlyPositive(rowCount, "rowCount");

            this.world = world;
            this.zones = new Zone[columnCount, rowCount];
            this.entityDiedEventHandler = OnEntityDied;
            this.entityBoundingRectangleChangedEventHandler = OnEntityBoundingRectangleChanged;
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when an entity dies.
        /// </summary>
        public event GenericEventHandler<EntityRegistry, Entity> Died;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the spatial bounds of this collection.
        /// </summary>
        public Rectangle Bounds
        {
            get { return world.Bounds; }
        }

        /// <summary>
        /// Gets the number of zone columns this collection uses.
        /// </summary>
        public int ColumnCount
        {
            get { return zones.GetLength(0); }
        }

        /// <summary>
        /// Gets the number of zone rows this collection uses.
        /// </summary>
        public int RowCount
        {
            get { return zones.GetLength(1); }
        }

        /// <summary>
        /// Gets the size of a zone, in spatial units.
        /// </summary>
        public Vector2 ZoneSize
        {
            get
            {
                return new Vector2(
                    Bounds.Width / ColumnCount,
                    Bounds.Height / RowCount);
            }
        }

        private int UnitsInZones
        {
            get { return zones.Cast<Zone>().Sum(zone => zone.Count); }
        }
        #endregion

        #region Methods
        #region Event Handlers
        private void OnEntityDied(Entity entity)
        {
            Argument.EnsureNotNull(entity, "entity");
            entitiesToRemove.Add(entity);
        }

        private void OnEntityBoundingRectangleChanged(Entity entity, ValueChangedEventArgs<Rectangle> eventArgs)
        {
            Argument.EnsureNotNull(entity, "entity");

            if (!entitiesToMove.ContainsKey(entity))
                entitiesToMove.Add(entity, eventArgs.OldValue);
        }
        #endregion

        #region Entity Creation
        /// <summary>
        /// Used by <see cref="Faction"/> to create new <see cref="Entity"/>
        /// from its <see cref="UnitType"/> and <see cref="Faction"/>.
        /// </summary>
        /// <param name="type">The <see cref="UnitType"/> of the <see cref="Entity"/> to be created.</param>
        /// <param name="faction">The <see cref="Faction"/> which creates the <see cref="Entity"/>.</param>
        /// <returns>The newly created <see cref="Entity"/>.</returns>
        internal Unit CreateUnit(UnitType type, Faction faction)
        {
            Unit unit = new Unit(nextID, type, faction);
            ++nextID;
            InitializeEntity(unit);
            return unit;
        }

        public ResourceNode CreateResourceNode(ResourceType type, int amount, Vector2 position)
        {
            ResourceNode node = new ResourceNode(world, nextID, type, amount, position);
            ++nextID;
            InitializeEntity(node);
            return node;
        }

        private void InitializeEntity(Entity entity)
        {
            entity.BoundingRectangleChanged += entityBoundingRectangleChangedEventHandler;
            entity.Died += entityDiedEventHandler;

            entitiesToAdd.Add(entity);
        }
        #endregion

        /// <summary>
        /// Gets a <see cref="Entity"/> of this <see cref="UnitRegistry"/> from its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the <see cref="Entity"/> to be found.</param>
        /// <returns>
        /// The <see cref="Entity"/> with that identifier, or <c>null</c> if the identifier is invalid.
        /// </returns>
        public Entity FindFromID(int id)
        {
            for (int i = 0; i < entities.Count; ++i)
                if (entities[i].ID == id)
                    return entities[i];

            return null;
        }

        /// <summary>
        /// Updates the <see cref="Entity"/>s in this <see cref="UnitRegistry"/> for a frame of the game.
        /// </summary>
        /// <param name="timeDeltaInSeconds">The time elapsed since the last frame, in seconds.</param>
        /// <remarks>
        /// Used by <see cref="World"/>.
        /// </remarks>
        public void Update(float timeDeltaInSeconds)
        {
            CommitDeferredCollectionChanges();

            for (int i = 0; i < entities.Count; ++i)
                entities[i].Update(timeDeltaInSeconds);
        }

        #region Private Collection Modification
        private void CommitDeferredCollectionChanges()
        {
            foreach (Entity entity in entitiesToAdd)
                Add(entity);
            entitiesToAdd.Clear();

            foreach (var pair in entitiesToMove)
                UpdateZone(pair.Key, pair.Value);
            entitiesToMove.Clear();

            foreach (Entity entity in entitiesToRemove)
                Remove(entity);
            entitiesToRemove.Clear();
        }

        private void Add(Entity entity)
        {
            entities.Add(entity);
            AddToZone(entity);
        }

        private void AddToZone(Entity entity)
        {
            Point zoneCoords = GetClampedZoneCoords(entity.BoundingRectangle.Center);
            zones[zoneCoords.X, zoneCoords.Y].Add(entity);
        }

        private void Remove(Entity entity)
        {
            entities.Remove(entity);
            RemoveFromZone(entity);
            GenericEventHandler<EntityRegistry, Entity> handler = Died;
            if (handler != null)
            {
                handler(this, entity);
            }
        }

        private void RemoveFromZone(Entity entity)
        {
            Point zoneCoords = GetClampedZoneCoords(entity.BoundingRectangle.Center);
            zones[zoneCoords.X, zoneCoords.Y].Remove(entity);
        }

        private void UpdateZone(Entity entity, Rectangle oldRectangle)
        {
            Point oldZoneCoords = GetClampedZoneCoords(oldRectangle.Center);
            Point newZoneCoords = GetClampedZoneCoords(entity.BoundingRectangle.Center);

            if (newZoneCoords != oldZoneCoords)
            {
                zones[oldZoneCoords.X, oldZoneCoords.Y].Remove(entity);
                zones[newZoneCoords.X, newZoneCoords.Y].Add(entity);
            }
        }
        #endregion

        private Point GetClampedZoneCoords(Vector2 position)
        {
            Vector2 normalizedPosition = world.Bounds.ParentToLocal(position);

            Point coords = new Point(
                (int)(normalizedPosition.X * ColumnCount),
                (int)(normalizedPosition.Y * RowCount));

            if (coords.X < 0) coords.X = 0;
            else if (coords.X >= ColumnCount) coords.X = ColumnCount - 1;

            if (coords.Y < 0) coords.Y = 0;
            else if (coords.Y >= RowCount) coords.Y = RowCount - 1;

            return coords;
        }

        #region Enumeration
        /// <summary>
        /// Gets an enumerator that iterates over the <see cref="Entity"/>s in this registry.
        /// </summary>
        /// <returns>A new <see cref="Entity"/> enumerator.</returns>
        public List<Entity>.Enumerator GetEnumerator()
        {
            return entities.GetEnumerator();
        }

        /// <summary>
        /// Gets the <see cref="Entity"/>s which are in a given rectangular area.
        /// </summary>
        /// <param name="area">The area in which to check.</param>
        /// <returns>A sequence of <see cref="Entity"/>s in that area.</returns>
        public IEnumerable<Entity> InArea(Rectangle area)
        {
            if (!Rectangle.Intersects(world.Bounds, area))
                yield break;

            Point minZoneCoords = GetClampedZoneCoords(area.Min);
            Point maxZoneCoords = GetClampedZoneCoords(area.Max);

            for (int x = minZoneCoords.X; x <= maxZoneCoords.X; ++x)
            {
                for (int y = minZoneCoords.Y; y <= maxZoneCoords.Y; ++y)
                {
                    Zone zone = zones[x, y];
                    for (int i = 0; i < zone.Count; ++i)
                    {
                        Entity entity = zone[i];
                        if (area.ContainsPoint(entity.BoundingRectangle.Center))
                            yield return entity;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="Entity"/>s which are in a given circular area.
        /// </summary>
        /// <param name="area">The area in which to check.</param>
        /// <returns>A sequence of <see cref="Entity"/>s in that area.</returns>
        public IEnumerable<Entity> InArea(Circle area)
        {
            if (!Intersection.Test(world.Bounds, area))
                yield break;

            Rectangle rectangle = area.BoundingRectangle;

            Point minZoneCoords = GetClampedZoneCoords(rectangle.Min);
            Point maxZoneCoords = GetClampedZoneCoords(rectangle.Max);

            for (int x = minZoneCoords.X; x <= maxZoneCoords.X; ++x)
            {
                for (int y = minZoneCoords.Y; y <= maxZoneCoords.Y; ++y)
                {
                    Zone zone = zones[x, y];
                    for (int i = 0; i < zone.Count; ++i)
                    {
                        Entity entity = zone[i];
                        if (area.ContainsPoint(entity.BoundingRectangle.Center))
                            yield return entity;
                    }
                }
            }
        }
        #endregion
        #endregion

        #region Explicit Members
        #region IEnumerable<Entity> Members
        IEnumerator<Entity> IEnumerable<Entity>.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region IEnumerable Members
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
        #endregion
        #endregion
    }
}
