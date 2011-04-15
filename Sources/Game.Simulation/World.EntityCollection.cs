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
            private enum DeferredChangeType
            {
                Add,
                Remove
            }
            #endregion

            #region Instance
            #region Fields
            private readonly World world;
            private readonly Func<Handle> handleGenerator = Handle.CreateGenerator();
            private readonly SortedDictionary<Handle, Entity> entities = new SortedDictionary<Handle, Entity>();

            // Used to defer modification of the "entities" collection.
            private bool isUpdating;
            private readonly Dictionary<Entity, DeferredChangeType> deferredChanges
                = new Dictionary<Entity, DeferredChangeType>();
            #endregion

            #region Constructors
            /// <summary>
            /// Initializes a new <see cref="EntityCollection"/> from the spatial
            /// bounds of the container and its number of subdivision along the axes.
            /// </summary>
            /// <param name="world">
            /// The <see cref="World"/> that to which the <see cref="Entity"/>s in this collection belong.
            /// </param>
            internal EntityCollection(World world)
            {
                Argument.EnsureNotNull(world, "world");

                this.world = world;
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
                Argument.EnsureNotNull(prototype, "prototype");
                Argument.EnsureNotNull(faction, "faction");

                Entity entity = CreateEmpty();
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

            internal Entity CreateEmpty()
            {
                return new Entity(world, handleGenerator());
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
                        if (entity.IsAlive)
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

                if (isUpdating) deferredChanges.Add(entity, DeferredChangeType.Add);
                else CommitAdd(entity);
            }

            /// <summary>
            /// Removes an entity from this world.
            /// </summary>
            /// <param name="entity">The entity to be removed.</param>
            public void Remove(Entity entity)
            {
                Argument.EnsureNotNull(entity, "entity");

                if (isUpdating) deferredChanges.Add(entity, DeferredChangeType.Remove);
                else CommitRemove(entity);
            }

            public void CommitDeferredChanges()
            {
                foreach (KeyValuePair<Entity, DeferredChangeType> pair in deferredChanges)
                {
                    if (pair.Value == DeferredChangeType.Add) CommitAdd(pair.Key);
                    else CommitRemove(pair.Key);
                }
                deferredChanges.Clear();
            }

            private void CommitAdd(Entity entity)
            {
                entities.Add(entity.Handle, entity);

                Debug.Assert(!entity.IsAwake, "An entity added to the world was already awake.");
                entity.Wake();
                world.OnEntityAdded(entity);
            }

            private void CommitRemove(Entity entity)
            {
                bool wasRemoved = entities.Remove(entity.Handle);
                if (!wasRemoved) return;

                entity.Sleep();
                world.OnEntityRemoved(entity);
            }
            #endregion

            #region Queries
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
            /// Gets an enumerator that iterates over the <see cref="Entity"/>s in this registry.
            /// </summary>
            /// <returns>A new <see cref="Entity"/> enumerator.</returns>
            public IEnumerator<Entity> GetEnumerator()
            {
                return entities.Values.GetEnumerator();
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
