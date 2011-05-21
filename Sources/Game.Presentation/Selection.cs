using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Engine.Geometry;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Presentation
{
    /// <summary>
    /// Represents a set of entities which are selected.
    /// </summary>
    public sealed class Selection : ICollection<Entity>
    {
        #region Fields
        public const float nearbyRadius = 10;

        private readonly World world;
        private readonly Faction localFaction;
        private readonly int limit;
        private readonly HashSet<Entity> entities = new HashSet<Entity>();
        private readonly List<Predicate<Entity>> priorityFilters = new List<Predicate<Entity>>();
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="Selection"/> from the world it refers to.
        /// </summary>
        /// <param name="world">The world of the <see cref="Entity">entities</see> that will be in this <see cref="Selection"/>.</param>
        /// <param name="localFaction">The local <see cref="Faction"/> which influances selection priorities.</param>
        /// <param name="limit">The selection limit, in number of <see cref="Entity">entities</see>.</param>
        public Selection(World world, Faction localFaction, int limit)
        {
            Argument.EnsureNotNull(world, "world");
            Argument.EnsurePositive(limit, "limit");

            this.world = world;
            this.localFaction = localFaction;
            this.limit = limit;
            
            world.Updated += OnWorldUpdated;

            // Setup priority filters
            if (localFaction == null)
            {
                // For spectators
                // - Units
                priorityFilters.Add(entity => FactionMembership.GetFaction(entity) != null && !Identity.IsEntityBuilding(entity));
                // - Buildings
                priorityFilters.Add(entity => FactionMembership.GetFaction(entity) != null);
            }
            else
            {
                // - Controlled units
                priorityFilters.Add(entity => FactionMembership.GetFaction(entity) == localFaction && !Identity.IsEntityBuilding(entity));
                // - Controlled buildings
                priorityFilters.Add(entity => FactionMembership.GetFaction(entity) == localFaction);
                // - Other faction units
                priorityFilters.Add(entity => FactionMembership.GetFaction(entity) != null && !Identity.IsEntityBuilding(entity));
                // - Other faction buildings
                priorityFilters.Add(entity => FactionMembership.GetFaction(entity) != null);
            }
            // - Fauna
            priorityFilters.Add(entity => !entity.Components.Has<Harvestable>());
            // - Resources (no filter needed)
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when the contents of the selection has changed.
        /// </summary>
        public event Action<Selection> Changed;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the faction towards which this selection has a bias.
        /// </summary>
        public Faction LocalFaction
        {
            get { return localFaction; }
        }

        /// <summary>
        /// Gets the number of entities in this selection.
        /// </summary>
        public int Count
        {
            get { return entities.Count; }
        }
        #endregion

        #region Methods
        #region Queries
        /// <summary>
        /// Gets a value indicating if this selection contains a given <see cref="Entity"/>.
        /// </summary>
        /// <param name="entity">The <see cref="Entity"/> to be found.</param>
        /// <returns><c>True</c> if the <see cref="Entity"/> is in this collection, <c>false</c> if not.</returns>
        public bool Contains(Entity entity)
        {
            return entities.Contains(entity);
        }

        /// <summary>
        /// Gets an enumerator of the entities in this selection.
        /// </summary>
        /// <returns>A new enumerator.</returns>
        public HashSet<Entity>.Enumerator GetEnumerator()
        {
            return entities.GetEnumerator();
        }
        #endregion

        #region Adding
        /// <summary>
        /// Attempts to add an <see cref="Entity"/> to this selection.
        /// </summary>
        /// <param name="entity">The <see cref="Entity"/> to be added.</param>
        /// <returns><c>True</c> if it was added, <c>false</c> if not.</returns>
        public bool Add(Entity entity)
        {
            Argument.EnsureNotNull(entity, "entity");

            if (!CanBeAdded(entity)) return false;

            entities.Add(entity);
            Changed.Raise(this);
            return true;
        }

        /// <summary>
        /// Adds multiple <see cref="Entity">entities</see> to the selection.
        /// </summary>
        /// <param name="entities">The <see cref="Entity">entities</see> to be added.</param>
        public void Add(IEnumerable<Entity> entities)
        {
            Argument.EnsureNotNull(entities, "entities");

            bool wasAnyEntityAdded = false;
            foreach (Entity entity in entities)
            {
                if (CanBeAdded(entity))
                {
                    this.entities.Add(entity);
                    wasAnyEntityAdded = true;
                }
            }

            if (wasAnyEntityAdded) Changed.Raise(this);
        }

        /// <summary>
        /// Adds the <see cref="Entity">entities</see> which are in a rectangle to the selection.
        /// </summary>
        /// <param name="firstPoint">
        /// The first point of the rectangles. <see cref="Entity">Entities</see> nearest to this will be selected first.
        /// </param>
        /// <param name="secondPoint">The second point of the rectangle.</param>
        public void AddFromRectangle(Vector2 firstPoint, Vector2 secondPoint)
        {
            FromRectangle(firstPoint, secondPoint, true);
        }
        #endregion

        #region Setting
        /// <summary>
        /// Sets the selection as containing a single entity.
        /// </summary>
        /// <param name="entity">The entity to be selected.</param>
        /// <returns><c>True</c> if the entity was selected, <c>false</c> if the selection was simply cleared.</returns>
        public bool Set(Entity entity)
        {
            Argument.EnsureNotNull(entity, "entity");

            bool wasEmpty = entities.Count == 0;
            entities.Clear();

            bool canBeAdded = CanBeAdded(entity);
            if (canBeAdded) entities.Add(entity);

            if (!wasEmpty || canBeAdded) Changed.Raise(this);

            return canBeAdded;
        }

        /// <summary>
        /// Sets the contents of this selection to a sequence of <see cref="Entity">entities</see>.
        /// </summary>
        /// <param name="entities">The <see cref="Entity">entities</see> to be selected.</param>
        public void Set(IEnumerable<Entity> entities)
        {
            Argument.EnsureNotNull(entities, "entities");

            this.entities.Clear();
            foreach (Entity entity in entities)
                if (CanBeAdded(entity))
                    this.entities.Add(entity);

            // This might make false positives, but it shouldn't matter.
            // (if the new selection is the same as the old one, it hasn't really changed)
            Changed.Raise(this);
        }

        /// <summary>
        /// Sets the selection to the <see cref="Entity">entities</see> which are in a rectangle.
        /// </summary>
        /// <param name="firstPoint">
        /// The first point of the rectangles. <see cref="Entity">Entities</see> nearest to this will be selected first.
        /// </param>
        /// <param name="secondPoint">The second point of the rectangle.</param>
        public void SetFromRectangle(Vector2 firstPoint, Vector2 secondPoint)
        {
            FromRectangle(firstPoint, secondPoint, false);
        }

        /// <summary>
        /// Sets the selection to <see cref="Entity">entities</see> similar to a given <see cref="Entity"/>.
        /// </summary>
        /// <param name="entity">The <see cref="Entity"/> for which similar ones are to be found.</param>
        public void SetToNearbySimilar(Entity entity)
        {
            Argument.EnsureNotNull(entity, "entity");

            // An identity component is needed to determine of other entities
            // have the same type.
            if (entity.Spatial == null || entity.Identity == null)
            {
                Set(entity);
                return;
            }

            Faction faction = FactionMembership.GetFaction(entity);
            Circle circle = new Circle(entity.Spatial.Center, nearbyRadius);

            var entities = world.SpatialManager
                .Intersecting(circle)
                .Select(spatial => spatial.Entity)
                .Where(e => Identity.HaveSamePrototype(e, entity) && FactionMembership.GetFaction(e) == faction);

            Set(entities);
        }
        #endregion

        /// <summary>
        /// Toggles the selection state of an entity between selected and not selected.
        /// </summary>
        /// <param name="entity">The entity which's selection is to be toggled.</param>
        public void Toggle(Entity entity)
        {
            Argument.EnsureNotNull(entity, "entity");

            bool wasRemoved = Remove(entity);
            if (wasRemoved) return;

            Add(entity);
        }

        #region Removing
        /// <summary>
        /// Removes an entity from this selection.
        /// </summary>
        /// <param name="entity">The entity to be removed.</param>
        /// <returns><c>True</c> if it was removed, <c>false</c> if it wasn't found.</returns>
        public bool Remove(Entity entity)
        {
            Argument.EnsureNotNull(entity, "entity");

            bool wasRemoved = entities.Remove(entity);
            if (!wasRemoved) return false;

            Changed.Raise(this);
            return true;
        }

        /// <summary>
        /// Clears the contents of this selection.
        /// </summary>
        public void Clear()
        {
            if (entities.Count == 0) return;

            entities.Clear();
            Changed.Raise(this);
        }
        #endregion

        #region Private Implementation Stuff
        private void FromRectangle(Vector2 rectangleStart, Vector2 rectangleEnd, bool add)
        {
            Rectangle rectangle = Rectangle.FromPoints(rectangleStart, rectangleEnd);

            List<Entity> entities = world.SpatialManager
                .Intersecting(rectangle)
                .Select(spatial => spatial.Entity)
                .Where(entity => entity.Identity.IsSelectable)
                .ToList();

            if (add) entities.RemoveAll(entity => Contains(entity));

            // Sort from nearest to selection rectangle start to farthest
            entities.Sort((a,b) => (a.Center - rectangleStart).LengthSquared.CompareTo((b.Center - rectangleStart).LengthSquared));

            // Apply priority rules (so that units are selected before buildings, for example)
            foreach (var entityPredicate in priorityFilters)
            {
                if (entities.Any(entity => entityPredicate(entity)))
                {
                    entities.RemoveAll(entity => !entityPredicate(entity));
                    break;
                }
            }

            if (add) Add(entities);
            else Set(entities);
        }

        private bool CanBeAdded(Entity entity)
        {
            return Count < limit
                && !Contains(entity)
                && IsValid(entity);
        }

        private bool IsValid(Entity entity)
        {
            return entity.IsAlive
                && entity.Spatial != null
                && (localFaction == null || localFaction.CanSee(entity));
        }

        private void OnWorldUpdated(World sender, SimulationStep step)
        {
            int removedCount = entities.RemoveWhere(entity => !IsValid(entity));
            if (removedCount == 0) return;

            Changed.Raise(this);
        }
        #endregion
        #endregion

        #region Object Model
        public override string ToString()
        {
            return entities.ToCommaSeparatedValues();
        }
        #endregion

        #region Explicit Members
        void ICollection<Entity>.Add(Entity item)
        {
            Add(item);
        }

        void ICollection<Entity>.CopyTo(Entity[] array, int arrayIndex)
        {
            entities.CopyTo(array, arrayIndex);
        }

        bool ICollection<Entity>.IsReadOnly
        {
            get { return false; }
        }
        
        IEnumerator<Entity> IEnumerable<Entity>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }
}
