using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using OpenTK.Math;

using Orion.Geometry;
using Orion.GameLogic.Tasks;

using Point = System.Drawing.Point;

namespace Orion.GameLogic
{
    /// <summary>
    /// A collection of <see cref="Entity">entities</see> optimized for spatial queries.
    /// </summary>
    [Serializable]
    public sealed class EntityRegistry : IEnumerable<Entity>
    {
        #region Fields
        private readonly World world;
        private readonly Dictionary<Handle, Entity> entities = new Dictionary<Handle, Entity>();
        private readonly EntityGrid grid;
        private readonly EntityZoneManager zoneManager;
        private readonly Func<Handle> uidGenerator;

        // Temporary collections used to defer modification of "entities"
        private readonly HashSet<Entity> entitiesToAdd = new HashSet<Entity>();
        private readonly Dictionary<Entity, Vector2> entitiesToMove = new Dictionary<Entity, Vector2>();
        private readonly HashSet<Entity> entitiesToRemove = new HashSet<Entity>();

        private readonly GenericEventHandler<Entity> entityDiedEventHandler;
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
        /// <param name="columnCount">The number of spatial subdivisions along the x axis.</param>
        /// <param name="rowCount">The number of spatial subdivisions along the y axis.</param>
        /// <param name="handleGenerator">A delegate to a method which generates unique identifiers.</param>
        internal EntityRegistry(World world, int columnCount, int rowCount, Func<Handle> uidGenerator)
        {
            Argument.EnsureNotNull(world, "world");
            Argument.EnsureStrictlyPositive(columnCount, "columnCount");
            Argument.EnsureStrictlyPositive(rowCount, "rowCount");
            Argument.EnsureNotNull(uidGenerator, "uidGenerator");

            this.world = world;
            this.grid = new EntityGrid(world.Terrain);
            this.zoneManager = new EntityZoneManager(world.Size);
            this.uidGenerator = uidGenerator;
            this.entityDiedEventHandler = OnEntityDied;
            this.entityMovedEventHandler = OnEntityMoved;
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when an entity dies.
        /// </summary>
        public event GenericEventHandler<EntityRegistry, Entity> Died;

        private void OnDied(Entity entity)
        {
            GenericEventHandler<EntityRegistry, Entity> handler = Died;
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
        private void OnEntityDied(Entity entity)
        {
            Argument.EnsureNotNull(entity, "entity");
            Debug.WriteLine("Entity {0} died.");
            entitiesToRemove.Add(entity);
            if (entity.IsSolid) grid.Remove(entity);
        }

        private void OnEntityMoved(Entity entity, ValueChangedEventArgs<Vector2> eventArgs)
        {
            Argument.EnsureNotNull(entity, "entity");

            if (!entitiesToMove.ContainsKey(entity))
                entitiesToMove.Add(entity, eventArgs.OldValue);

            if (entity.IsSolid)
            {
                Rectangle oldBoundingRectangle = new Rectangle(
                    eventArgs.OldValue.X, eventArgs.OldValue.Y,
                    entity.Size.Width, entity.Size.Height);

                grid.Remove(entity, oldBoundingRectangle);
                grid.Add(entity);
            }
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
            Handle uid = uidGenerator();
            Unit unit = new Unit(uid, type, faction, point);
            InitializeEntity(unit);
            Debug.WriteLine("Created unit: {0} at {1}.".FormatInvariant(unit, point));
            return unit;
        }

        public ResourceNode CreateResourceNode(ResourceType type, Point point)
        {
            Handle uid = uidGenerator();
            ResourceNode node = new ResourceNode(world, uid, type, ResourceNode.DefaultTotalAmount, point);
            InitializeEntity(node);
            Debug.WriteLine("Created resource node: {0} at {1}.".FormatInvariant(node, point));
            return node;
        }

        private void InitializeEntity(Entity entity)
        {
            entity.Moved += entityMovedEventHandler;
            entity.Died += entityDiedEventHandler;

            if (entity.IsSolid) grid.Add(entity);

            entitiesToAdd.Add(entity);
        }
        #endregion

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

        /// <summary>
        /// Updates the <see cref="Entity"/>s in this <see cref="UnitRegistry"/> for a frame of the game.
        /// </summary>
        /// <param name="timeDeltaInSeconds">The time elapsed since the last frame, in seconds.</param>
        /// <remarks>
        /// Used by <see cref="World"/>.
        /// </remarks>
        public void Update(float timeDeltaInSeconds)
        {
            CommitDeferredChanges();

            foreach (Entity entity in entities.Values)
                entity.Update(timeDeltaInSeconds);
        }

        #region Commit Deferred Changes
        private void CommitDeferredChanges()
        {
            CommitDeferredAdds();
            CommitDeferredMoves();
            CommitDeferredRemoves();
        }

        private void CommitDeferredAdds()
        {
            foreach (Entity entity in entitiesToAdd)
            {
                entities.Add(entity.Handle, entity);
                zoneManager.Add(entity);
            }
            entitiesToAdd.Clear();
        }

       private void CommitDeferredMoves()
       {
           foreach (var pair in entitiesToMove)
           {
               zoneManager.UpdateZone(pair.Key, pair.Value);
           }
           entitiesToMove.Clear();
       }

       private void CommitDeferredRemoves()
       {
           foreach (Entity entity in entitiesToRemove)
           {
               entities.Remove(entity.Handle);
               zoneManager.Remove(entity);
               OnDied(entity);
           }
           entitiesToRemove.Clear();
       }
        #endregion

        public Entity GetEntityAt(Point point)
        {
            return grid[point];
        }

        #region Enumeration
        /// <summary>
        /// Gets an enumerator that iterates over the <see cref="Entity"/>s in this registry.
        /// </summary>
        /// <returns>A new <see cref="Entity"/> enumerator.</returns>
        public Dictionary<Handle, Entity>.ValueCollection.Enumerator GetEnumerator()
        {
            return entities.Values.GetEnumerator();
        }

        /// <summary>
        /// Gets the <see cref="Entity"/>s which are in a given rectangular area.
        /// </summary>
        /// <param name="area">The area in which to check.</param>
        /// <returns>A sequence of <see cref="Entity"/>s in that area.</returns>
        public IEnumerable<Entity> InArea(Rectangle area)
        {
            return zoneManager.InArea(area);
        }

        /// <summary>
        /// Gets the <see cref="Entity"/>s which are in a given circular area.
        /// </summary>
        /// <param name="area">The area in which to check.</param>
        /// <returns>A sequence of <see cref="Entity"/>s in that area.</returns>
        public IEnumerable<Entity> InArea(Circle area)
        {
            return zoneManager.InArea(area.BoundingRectangle)
                .Where(entity => area.ContainsPoint(entity.Center));
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
    }
}
