using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Engine.Geometry;
using Orion.Game.Simulation.Tasks;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Simulation
{
    partial class World
    {
        /// <summary>
        /// A collection of <see cref="Entity">entities</see> optimized for spatial queries.
        /// </summary>
        [Serializable]
        public sealed class EntityCollection : IEnumerable<Entity>
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
            private readonly SortedDictionary<Handle, Entity> entities = new SortedDictionary<Handle, Entity>();
            private readonly EntityGrid groundGrid;
            private readonly EntityGrid airGrid;
            private readonly EntityZoneManager zoneManager;

            // Used to defer modification of the "entities" collection.
            private bool isUpdating;
            private readonly Dictionary<Entity, DeferredChange> deferredChanges
                = new Dictionary<Entity, DeferredChange>();
            #endregion

            #region Constructors
            /// <summary>
            /// Initializes a new <see cref="SpatialCollection{Entity}"/> from the spatial
            /// bounds of the container and its number of subdivision along the axes.
            /// </summary>
            /// <param name="world">
            /// The <see cref="World"/> that to which the <see cref="Entity"/>s in this <see cref="UnitRegistry"/> belong.
            /// </param>
            internal EntityCollection(World world)
            {
                Argument.EnsureNotNull(world, "world");

                this.world = world;
                this.groundGrid = new EntityGrid(world.Size);
                this.airGrid = new EntityGrid(world.Size);
                this.zoneManager = new EntityZoneManager(world.Size);
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
            #region Entity Creation
            /// <summary>
            /// Used by <see cref="Faction"/> to create new <see cref="Entity"/>
            /// from its <see cref="Entity"/> and <see cref="Faction"/>.
            /// </summary>
            /// <param name="prototype">The prototype of the <see cref="Entity"/> to be created.</param>
            /// <param name="faction">The <see cref="Faction"/> which creates the <see cref="Entity"/>.</param>
            /// <param name="point">The initial position of the <see cref="Entity"/> to be created.</param>
            /// <returns>The newly created <see cref="Entity"/>.</returns>
            internal Entity CreateUnit(Entity prototype, Faction faction, Point point)
            {
                Argument.EnsureNotNull(prototype, "faction");
                Argument.EnsureNotNull(faction, "faction");

                Entity entity = CreateEntity();
                entity.SpecializeWithPrototype(prototype);

                FactionMembership membership = entity.Components.TryGet<FactionMembership>();
                if (membership == null)
                {
                    membership = new FactionMembership(entity);
                    entity.Components.Add(membership);
                }
                membership.Faction = faction;

                entity.Components.Get<Spatial>().Position = point;
                entity.Components.Add(new TaskQueue(entity));
                Add(entity);

                return entity;
            }

            internal Entity CreateEntity()
            {
                return new Entity(world, handleGenerator());
            }

            public Entity CreateResourceNode(ResourceType type, Point point)
            {
                Entity node = new Entity(world, handleGenerator());
                Identity identity = new Identity(node);
                identity.LeavesRemains = false;
                node.Components.Add(identity);

                Harvestable harvestableComponent = new Harvestable(node);
                harvestableComponent.Amount = World.DefaultResourceAmount;
                harvestableComponent.Type = type;
                node.Components.Add(harvestableComponent);

                Spatial positionComponent = new Spatial(node);
                positionComponent.Size = new Size(2, 2);
                positionComponent.Position = point;
                if (type == ResourceType.Aladdium)
                    positionComponent.CollisionLayer = CollisionLayer.Ground;
                node.Components.Add(positionComponent);
                
                Add(node);
                return node;
            }
            #endregion

            /// <summary>
            /// Updates the <see cref="Entity"/>s in this <see cref="UnitRegistry"/> for a frame of the game.
            /// </summary>
            /// <param name="step">Information on this simulation step.</param>
            /// <remarks>
            /// Used by <see cref="World"/>.
            /// </remarks>
            internal void Update(SimulationStep step)
            {
                if (isUpdating) throw new InvalidOperationException("Cannot nest Update calls.");

                try
                {
                    isUpdating = true;
                    foreach (Entity entity in entities.Values)
                        if (entity.IsAliveInWorld)
                            entity.Update(step);
                }
                finally
                {
                    isUpdating = false;
                }

                CommitDeferredChanges();
            }

            #region Collection Modifications
            public void Add(Entity entity)
            {
                Argument.EnsureNotNull(entity, "entity");

                if (isUpdating) DeferAdd(entity);
                else CommitAdd(entity);

                EntityGrid grid = GetGrid(entity.Spatial.CollisionLayer);
                if (grid != null) grid.Add(entity);
            }

            internal void MoveFrom(Entity entity, Vector2 oldPosition)
            {
                if (isUpdating) DeferMove(entity, oldPosition);
                else CommitMove(entity, oldPosition);

                EntityGrid grid = GetGrid(entity.Spatial.CollisionLayer);
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

            /// <summary>
            /// Removes an entity from this world.
            /// </summary>
            /// <param name="entity">The entity to be removed.</param>
            public void Remove(Entity entity)
            {
                Argument.EnsureNotNull(entity, "entity");

                if (isUpdating) DeferRemove(entity);
                else CommitRemove(entity);

                EntityGrid grid = GetGrid(entity.Spatial.CollisionLayer);
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
                world.OnEntityAdded(entity);
            }

            private void CommitMove(Entity entity, Vector2 oldPosition)
            {
                Vector2 oldCenter = oldPosition + entity.Spatial.BoundingRectangle.Extent;
                zoneManager.UpdateZone(entity, oldCenter);
            }

            private void CommitRemove(Entity entity)
            {
                bool wasRemoved = entities.Remove(entity.Handle);
                if (!wasRemoved) return;

                zoneManager.Remove(entity);
                world.OnEntityRemoved(entity);
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

            public Entity GetTopmostEntityAt(Point point)
            {
                if (!world.IsWithinBounds(point))
                {
                    Debug.Fail("Point out of world bounds.");
                    return null;
                }

                Vector2 tileCenter = new Vector2(point.X + 0.5f, point.Y + 0.5f);
                return Intersecting(tileCenter)
                    .WithMaxOrDefault(e => e.Spatial.CollisionLayer);
            }

            public Entity GetTopmostGridEntityAt(Point point)
            {
                if (!world.IsWithinBounds(point))
                {
                    Debug.Fail("Point out of world bounds.");
                    return null;
                }

                return airGrid[point] ?? groundGrid[point];
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
                    .Where(entity => Intersection.Test(area, entity.Spatial.BoundingRectangle));
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
}
