using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenTK.Math;
using Orion.Geometry;
using Orion.GameLogic.Tasks;

namespace Orion.GameLogic
{
    /// <summary>
    /// A collection of <see cref="Entity">entities</see> optimized for spatial queries.
    /// </summary>
    [Serializable]
    public sealed class EntityManager : IEnumerable<Entity>
    {
        #region Nested Types
        [Flags]
        private enum DeferredChangeType
        {
            Add = 1,
            Move = 2,
            Remove = 4
        }

        private struct DeferredChange
        {
            #region Fields
            private readonly DeferredChangeType types;
            private readonly Vector2 oldPosition;
            #endregion

            #region Constructors
            public DeferredChange(DeferredChangeType types, Vector2 oldPosition)
            {
                this.types = types;
                this.oldPosition = oldPosition;
            }

            public DeferredChange(DeferredChangeType types)
                : this(types, Vector2.Zero) { }
            #endregion

            #region Properties
            public Vector2 OldPosition
            {
                get { return oldPosition; }
            }
            #endregion

            #region Methods
            public DeferredChange CreateCombined(DeferredChangeType type, Vector2 oldPosition)
            {
                return new DeferredChange(types | type, oldPosition);
            }

            public DeferredChange CreateCombined(DeferredChangeType type)
            {
                return CreateCombined(type, oldPosition);
            }

            public bool HasType(DeferredChangeType type)
            {
                return (types & type) != 0;
            }
            #endregion
        }
        #endregion

        #region Instance
        #region Fields
        private readonly World world;
        private readonly Func<Handle> handleGenerator = Handle.CreateGenerator();
        private readonly SortedList<Handle, Entity> entities = new SortedList<Handle, Entity>();
        private readonly EntityGrid groundGrid;
        private readonly EntityGrid airGrid;
        private readonly EntityZoneManager zoneManager;

        // Used to defer modification of the "entities" collection.
        private bool isUpdating;
        private readonly Dictionary<Entity, DeferredChange> deferredChanges
            = new Dictionary<Entity, DeferredChange>();

        private readonly Action<Entity> entityDiedEventHandler;
        private readonly ValueChangedEventHandler<Entity, Vector2> entityMovedEventHandler;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="SpatialCollection{Entity}"/> from the spatial
        /// bounds of the container and its number of subdivision along the axes.
        /// </summary>
        /// <param name="world">
        /// The <see cref="World"/> that to which the <see cref="Entity"/>s in this <see cref="UnitRegistry"/> belong.
        /// </param>
        internal EntityManager(World world)
        {
            Argument.EnsureNotNull(world, "world");

            this.world = world;
            this.groundGrid = new EntityGrid(world.Size);
            this.airGrid = new EntityGrid(world.Size);
            this.zoneManager = new EntityZoneManager(world.Size);
            this.entityDiedEventHandler = OnEntityDied;
            this.entityMovedEventHandler = OnEntityMoved;
        }
        #endregion

        #region Events
        public event Action<EntityManager, Entity> Added;

        private void RaiseAdded(Entity entity)
        {
            var handler = Added;
            if (handler != null) handler(this, entity);
        }

        /// <summary>
        /// Raised when an <see cref="Entity"/> gets removed.
        /// </summary>
        public event Action<EntityManager, Entity> Removed;

        private void RaiseRemoved(Entity entity)
        {
            var handler = Removed;
            if (handler != null) handler(this, entity);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the spatial bounds of this collection.
        /// </summary>
        public Rectangle Bounds
        {
            get { return world.Bounds; }
        }
        #endregion

        #region Methods
        #region Event Handlers
        private void OnEntityMoved(Entity entity, Vector2 oldPosition, Vector2 newPosition)
        {
            Argument.EnsureNotNull(entity, "entity");

            Move(entity, oldPosition);
        }

        private void OnEntityDied(Entity entity)
        {
            Argument.EnsureNotNull(entity, "entity");
#if DEBUG
            // #if'd so the FormatInvariant is not executed in release.
            Debug.WriteLine("Entity {0} died.".FormatInvariant(entity));
#endif

            Remove(entity);
        }
        #endregion

        #region Entity Creation
        /// <summary>
        /// Used by <see cref="Faction"/> to create new <see cref="Unit"/>
        /// from its <see cref="UnitType"/> and <see cref="Faction"/>.
        /// </summary>
        /// <param name="type">The <see cref="UnitType"/> of the <see cref="Unit"/> to be created.</param>
        /// <param name="faction">The <see cref="Faction"/> which creates the <see cref="Unit"/>.</param>
        /// <param name="point">The initial position of the <see cref="Unit"/> to be created.</param>
        /// <returns>The newly created <see cref="Entity"/>.</returns>
        internal Unit CreateUnit(UnitType type, Faction faction, Point point)
        {
            Handle handle = handleGenerator();
            Unit unit = new Unit(handle, type, faction, point);
            InitializeEntity(unit);
            
#if DEBUG
            // #if'd so the FormatInvariant is not executed in release.
            Debug.WriteLine("Created unit: {0} at {1}.".FormatInvariant(unit, point));
#endif

            return unit;
        }

        public ResourceNode CreateResourceNode(ResourceType type, Point point)
        {
            Handle handle = handleGenerator();
            ResourceNode node = new ResourceNode(world, handle, type, ResourceNode.DefaultTotalAmount, point);
            InitializeEntity(node);

#if DEBUG
            // #if'd so the FormatInvariant is not executed in release.
            Debug.WriteLine("Created resource node: {0} at {1}.".FormatInvariant(node, point));
#endif

            return node;
        }

        private void InitializeEntity(Entity entity)
        {
            entity.Moved += entityMovedEventHandler;
            entity.Died += entityDiedEventHandler;

            Add(entity);
        }
        #endregion

        /// <summary>
        /// Updates the <see cref="Entity"/>s in this <see cref="UnitRegistry"/> for a frame of the game.
        /// </summary>
        /// <param name="step">Information on this simulation step.</param>
        /// <remarks>
        /// Used by <see cref="World"/>.
        /// </remarks>
        public void Update(SimulationStep step)
        {
            if (isUpdating) throw new InvalidOperationException("Cannot nest Update calls.");

            try
            {
                isUpdating = true;
                foreach (Entity entity in entities.Values)
                    entity.Update(step);

                CommitDeferredChanges();
            }
            finally
            {
                isUpdating = false;
            }
        }

        #region Collection Modifications
        public void Add(Entity entity)
        {
            Argument.EnsureNotNull(entity, "entity");

            if (isUpdating) DeferAdd(entity);
            else CommitAdd(entity);

            EntityGrid grid = GetGrid(entity.CollisionLayer);
            if (grid != null) grid.Add(entity);
        }

        private void Move(Entity entity, Vector2 oldPosition)
        {
            if (isUpdating) DeferMove(entity, oldPosition);
            else CommitMove(entity, oldPosition);

            EntityGrid grid = GetGrid(entity.CollisionLayer);
            if (grid != null)
            {
                Region oldRegion = Entity.GetGridRegion(oldPosition, entity.Size);
                Region newRegion = Entity.GetGridRegion(entity.Position, entity.Size);
                if (newRegion != oldRegion)
                {
                    grid.Remove(entity, oldRegion);
                    grid.Add(entity, newRegion);
                }
            }
        }

        public void Remove(Entity entity)
        {
            Argument.EnsureNotNull(entity, "entity");

            if (isUpdating) DeferRemove(entity);
            else CommitRemove(entity);

            EntityGrid grid = GetGrid(entity.CollisionLayer);
            if (grid != null) grid.Remove(entity);
        }

        #region Deferring
        private void DeferAdd(Entity entity)
        {
            deferredChanges.Add(entity, new DeferredChange(DeferredChangeType.Add, Vector2.Zero));
        }

        private void DeferMove(Entity entity, Vector2 oldPosition)
        {
            DeferredChange change;
            deferredChanges.TryGetValue(entity, out change);
            if (!change.HasType(DeferredChangeType.Move))
            {
                change = change.CreateCombined(DeferredChangeType.Move, oldPosition);
                deferredChanges[entity] = change;
            }
        }

        private void DeferRemove(Entity entity)
        {
            DeferredChange change;
            deferredChanges.TryGetValue(entity, out change);
            Debug.Assert(!change.HasType(DeferredChangeType.Remove), "An entity has died twice.");
            change = change.CreateCombined(DeferredChangeType.Remove);
            deferredChanges[entity] = change;
        }
        #endregion

        #region Commiting
        public void CommitDeferredChanges()
        {
            foreach (KeyValuePair<Entity, DeferredChange> pair in deferredChanges)
            {
                Entity entity = pair.Key;
                DeferredChange change = pair.Value;
                if (change.HasType(DeferredChangeType.Add))
                {
                    if (change.HasType(DeferredChangeType.Remove))
                    {
                        Debug.Fail("An entity has been both added and removed in the same frame, that's peculiar.");
                        continue; // Nop, we're not going to add it to remove it thereafter
                    }

                    CommitAdd(entity);
                }

                if (change.HasType(DeferredChangeType.Move))
                    CommitMove(entity, change.OldPosition);

                if (change.HasType(DeferredChangeType.Remove))
                    CommitRemove(entity);
            }
            deferredChanges.Clear();
        }

        private void CommitAdd(Entity entity)
        {
            entities.Add(entity.Handle, entity);
            zoneManager.Add(entity);
            RaiseAdded(entity);
        }

        private void CommitMove(Entity entity, Vector2 oldPosition)
        {
            zoneManager.UpdateZone(entity, oldPosition);
        }

        private void CommitRemove(Entity entity)
        {
            entities.Remove(entity.Handle);
            zoneManager.Remove(entity);
            RaiseRemoved(entity);
        }
        #endregion
        #endregion

        #region Queries
        private EntityGrid GetGrid(CollisionLayer layer)
        {
            if (layer == CollisionLayer.Ground) return groundGrid;
            if (layer == CollisionLayer.Air) return airGrid;
            return null;
        }

        /// <summary>
        /// Gets a <see cref="Entity"/> of this <see cref="UnitRegistry"/> from its unique identifier.
        /// </summary>
        /// <param name="handle">The handle of the <see cref="Entity"/> to be found.</param>
        /// <returns>
        /// The <see cref="Entity"/> with that identifier,
        /// or <c>null</c> if no <see cref="Entity"/> has this identifier.
        /// </returns>
        public Entity FromHandle(Handle handle)
        {
            Entity entity;
            entities.TryGetValue(handle, out entity);
            return entity;
        }

        public Entity GetEntityAt(Point point, CollisionLayer layer)
        {
            if (!world.IsWithinBounds(point))
            {
                Debug.Fail("Point out of world bounds.");
                return null;
            }

            EntityGrid grid = GetGrid(layer);
            if (grid == null) return null;
            return grid[point];
        }

        public Entity GetGroundEntityAt(Point point)
        {
            if (!world.IsWithinBounds(point))
            {
                Debug.Fail("Point out of world bounds.");
                return null;
            }

            return groundGrid[point];
        }

        public Entity GetAirEntityAt(Point point)
        {
            if (!world.IsWithinBounds(point))
            {
                Debug.Fail("Point out of world bounds.");
                return null;
            }

            return airGrid[point];
        }

        public Unit GetUnitAt(Point point)
        {
            if (!world.IsWithinBounds(point))
            {
                Debug.Fail("Point out of world bounds.");
                return null;
            }

            return airGrid[point] as Unit ?? groundGrid[point] as Unit;
        }

        /// <summary>
        /// Gets an enumerator that iterates over the <see cref="Entity"/>s in this registry.
        /// </summary>
        /// <returns>A new <see cref="Entity"/> enumerator.</returns>
        public IEnumerator<Entity> GetEnumerator()
        {
            return entities.Values.GetEnumerator();
        }

        /// <summary>
        /// Gets the <see cref="Entity"/>s which intersect a rectangular area.
        /// </summary>
        /// <param name="area">The area in which to check.</param>
        /// <returns>A sequence of <see cref="Entity"/>s intersecting that area.</returns>
        public IEnumerable<Entity> Intersecting(Rectangle area)
        {
            return zoneManager.Intersecting(area);
        }

        /// <summary>
        /// Gets the <see cref="Entity"/>s which intersect a given circular area.
        /// </summary>
        /// <param name="area">The area in which to check.</param>
        /// <returns>A sequence of <see cref="Entity"/>s intersecting that area.</returns>
        public IEnumerable<Entity> Intersecting(Circle area)
        {
            return zoneManager.Intersecting(area.BoundingRectangle)
                .Where(entity => Intersection.Test(area, entity.BoundingRectangle));
        }

        public IEnumerable<Entity> Intersecting(Vector2 location)
        {
            return zoneManager.Intersecting(location);
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
