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
        public static readonly float NearbyRadius = 10;

        private readonly Faction localFaction;
        private readonly int limit;
        private readonly HashSet<Entity> entities = new HashSet<Entity>();
        #endregion

        #region Constructors
        public Selection(Faction localFaction, int limit)
        {
            Argument.EnsureNotNull(localFaction, "localFaction");
            Argument.EnsureStrictlyPositive(limit, "limit");

            this.localFaction = localFaction;
            this.limit = limit;

            localFaction.World.EntityRemoved += OnEntityRemoved;
            localFaction.World.Updated += OnWorldUpdated;
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

        /// <summary>
        /// Gets the maximum number of entities that can be selected at once.
        /// </summary>
        public int Limit
        {
            get { return limit; }
        }

        /// <summary>
        /// Gets a value indicating if this selection is empty.
        /// </summary>
        public bool IsEmpty
        {
            get { return entities.Count == 0; }
        }

        /// <summary>
        /// Gets a value indicating if this selection is full.
        /// </summary>
        public bool IsFull
        {
            get
            {
                return entities.Count == limit
                    || Type == SelectionType.ResourceNode;
            }
        }

        /// <summary>
        /// Gets the type of contents this selection currently has.
        /// </summary>
        public SelectionType Type
        {
            get
            {
                if (entities.Count == 0) return SelectionType.Empty;
                return entities.FirstOrDefault() is Unit ? SelectionType.Units : SelectionType.ResourceNode;
            }
        }

        /// <summary>
        /// Gets the resource node in this selection, if any.
        /// </summary>
        public Entity ResourceNode
        {
            get { return entities.FirstOrDefault(e => e.HasComponent<Harvestable>()); }
        }

        /// <summary>
        /// Gets the units in this selection, if any.
        /// </summary>
        public IEnumerable<Unit> Units
        {
            get { return entities.OfType<Unit>(); }
        }
        #endregion

        #region Methods
        #region Queries
        /// <summary>
        /// Gets a value indicating if this selection contains a given entity.
        /// </summary>
        /// <param name="entity">The entity to be found.</param>
        /// <returns><c>True</c> if the entity is in this collection, <c>false</c> if not.</returns>
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
        /// Attempts to add an entity to this selection.
        /// </summary>
        /// <param name="entity">The entity to be added.</param>
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
        /// Adds multiple units to the selection.
        /// </summary>
        /// <param name="units">The units to be added.</param>
        public void Add(IEnumerable<Unit> units)
        {
            Argument.EnsureNotNull(units, "units");

            if (Type == SelectionType.ResourceNode) return;

            bool wasUnitAdded = false;
            foreach (Unit unit in units)
            {
                if (CanBeAdded(unit))
                {
                    entities.Add(unit);
                    wasUnitAdded = true;
                }
            }

            if (wasUnitAdded) Changed.Raise(this);
        }

        /// <summary>
        /// Adds the units which are in a rectangle to the selection.
        /// </summary>
        /// <param name="firstPoint">
        /// The first point of the rectangles. Units nearest to this will be selected first.
        /// </param>
        /// <param name="secondPoint">The second point of the rectangle.</param>
        public void AddUnitsInRectangle(Vector2 firstPoint, Vector2 secondPoint)
        {
            if (Type == SelectionType.ResourceNode) return;
            SelectUnitsInRectangle(firstPoint, secondPoint, true);
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
        /// Sets the contents of this selection to a sequence of units.
        /// </summary>
        /// <param name="units">The units to be selected.</param>
        public void Set(IEnumerable<Unit> units)
        {
            Argument.EnsureNotNull(units, "units");

            entities.Clear();
            foreach (Unit unit in units)
                if (CanBeAdded(unit))
                    entities.Add(unit);

            // This might make false positives but I assume it doesn't matter.
            Changed.Raise(this);
        }

        /// <summary>
        /// Sets the selection to the units which are in a rectangle.
        /// </summary>
        /// <param name="firstPoint">
        /// The first point of the rectangles. Units nearest to this will be selected first.
        /// </param>
        /// <param name="secondPoint">The second point of the rectangle.</param>
        public void SetToRectangle(Vector2 firstPoint, Vector2 secondPoint)
        {
            SelectUnitsInRectangle(firstPoint, secondPoint, false);
        }

        /// <summary>
        /// Sets the selection to units similar to a given entity in a given radius.
        /// </summary>
        /// <param name="entity">The entity for which similar units are to be found.</param>
        public void SetToNearbySimilar(Entity entity)
        {
            Argument.EnsureNotNull(entity, "entity");

            if (entity.HasComponent<Harvestable>())
            {
                Set(entity);
                return;
            }

            Unit unit = (Unit)entity;
            Circle circle = new Circle(entity.Center, NearbyRadius);
            var units = localFaction.World.Entities
                .Intersecting(circle)
                .OfType<Unit>()
                .Where(u => u.Type == unit.Type && u.Faction == unit.Faction);

            Set(units);
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
        private void SelectUnitsInRectangle(Vector2 rectangleStart, Vector2 rectangleEnd, bool add)
        {
            Rectangle rectangle = Rectangle.FromPoints(rectangleStart, rectangleEnd);

            List<Unit> units = localFaction.World.Entities
                .Intersecting(rectangle)
                .OfType<Unit>()
                .OrderBy(unit => (unit.Center - rectangleStart).LengthSquared)
                .ToList();

            // Filter out factions
            bool containsControllableUnits = units.Any(unit => unit.Faction.GetDiplomaticStance(localFaction).HasFlag(DiplomaticStance.SharedControl));
            if (containsControllableUnits)
                units.RemoveAll(unit => !unit.Faction.GetDiplomaticStance(localFaction).HasFlag(DiplomaticStance.SharedControl));
            else if (units.Count > 1)
                units.RemoveRange(1, units.Count - 1);

            // Filter out buildings
            bool containsNonBuildingUnits = units.Any(unit => !unit.Type.IsBuilding);
            if (containsNonBuildingUnits) units.RemoveAll(unit => unit.Type.IsBuilding);

            if (add) Add(units);
            else Set(units);
        }

        private bool CanBeAdded(Entity entity)
        {
            return !IsFull
                && !Contains(entity)
                && entity.IsAliveInWorld
                && (localFaction == null || localFaction.CanSee(entity))
                && !(Type == SelectionType.Units && entity.HasComponent<Harvestable>());
        }

        private void OnEntityRemoved(World sender, Entity entity)
        {
            Remove(entity);
        }

        private void OnWorldUpdated(World sender, SimulationStep step)
        {
            RemoveHiddenEntitiesFromSelection();
        }

        private void RemoveHiddenEntitiesFromSelection()
        {
            int removedCount = entities.RemoveWhere(entity => !localFaction.CanSee(entity));
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
