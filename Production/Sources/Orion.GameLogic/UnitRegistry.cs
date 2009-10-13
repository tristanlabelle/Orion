using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using OpenTK.Math;

using Orion.Geometry;

using Point = System.Drawing.Point;

namespace Orion.GameLogic
{
    /// <summary>
    /// A collection of <see cref="Unit"/>s optimized for spatial queries.
    /// </summary>
    [Serializable]
    public sealed class UnitRegistry : IEnumerable<Unit>
    {
        #region Nested Types
        /// <summary>
        /// Represents a rectangular spatial subdivision used to group nearby <see cref="Unit"/>s.
        /// </summary>
        [DebuggerDisplay("Count = {Count}")]
        private struct Zone
        {
            #region Fields
            public Unit[] Units;
            public int Count;
            #endregion

            #region Indexers
            /// <summary>
            /// Gets a <see cref="Unit"/> from this <see cref="Zone"/> by its index.
            /// </summary>
            /// <param name="index">The index of the <see cref="Unit"/> to be retrieved.</param>
            /// <returns>The <see cref="Unit"/> at that index.</returns>
            public Unit this[int index]
            {
                get { return Units[index]; }
            }
            #endregion

            #region Methods
            /// <summary>
            /// Removes a <see cref="Unit"/> from this zone.
            /// </summary>
            /// <param name="item">The <see cref="Unit"/> to be removed.</param>
            /// <returns><c>True</c> if an <see cref="Unit"/> was removed, <c>false</c> if it wasn't found.</returns>
            public bool Remove(Unit unit)
            {
                for (int i = 0; i < Count; ++i)
                {
                    if (Units[i] == unit)
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
            /// Adds a <see cref="Unit"/> to this <see cref="Zone"/>
            /// The <see cref="Unit"/> is assumed to be absent.
            /// </summary>
            /// <param name="item">The <see cref="Unit"/> to be added.</param>
            public void Add(Unit unit)
            {
                if (Units == null) Units = new Unit[16];
                else if (Count == Units.Length)
                {
                    Unit[] newItems = new Unit[Units.Length * 2];
                    Array.Copy(Units, newItems, Units.Length);
                    Units = newItems;
                }

                Units[Count] = unit;
                ++Count;
            }
            #endregion
        }

        [Flags]
        private enum Event
        {
            Created = 1,
            Moved = 2,
            Died = 4
        }
        #endregion

        #region Instance
        #region Fields
        private readonly World world;
        private readonly List<Unit> units = new List<Unit>();
        private readonly Zone[,] zones;
        private readonly Dictionary<Unit, Event> events = new Dictionary<Unit, Event>();
        private readonly GenericEventHandler<Unit> unitDiedEventHandler;
        private readonly GenericEventHandler<Unit> unitMovedEventHandler;
        private int nextUnitID;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="SpatialCollection{Unit}"/> from the spatial
        /// bounds of the container and its number of subdivision along the axes.
        /// </summary>
        /// <param name="world">
        /// The <see cref="World"/> that to which the <see cref="Unit"/>s in this <see cref="UnitRegistry"/> belong.
        /// </param>
        /// <param name="columnCount">The number of spatial subdivisions along the x axis.</param>
        /// <param name="rowCount">The number of spatial subdivisions along the y axis.</param>
        internal UnitRegistry(World world, int columnCount, int rowCount)
        {
            Argument.EnsureStrictlyPositive(columnCount, "columnCount");
            Argument.EnsureStrictlyPositive(rowCount, "rowCount");

            this.world = world;
            this.zones = new Zone[columnCount, rowCount];
            this.unitDiedEventHandler = OnUnitDied;
            this.unitMovedEventHandler = OnUnitMoved;
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
        private void OnUnitDied(Unit unit)
        {
            Argument.EnsureNotNull(unit, "unit");
            events[unit] = Event.Died;
        }

        private void OnUnitMoved(Unit unit)
        {
            Argument.EnsureNotNull(unit, "unit");

            Event @event = Event.Moved;
            if (events.TryGetValue(unit, out @event))
            {
                if ((@event & Event.Moved) != Event.Moved)
                    events[unit] = @event | Event.Moved;
            }
            else
            {
                events.Add(unit, Event.Moved);
            }
        }
        #endregion

        /// <summary>
        /// Used by <see cref="Faction"/> to create new <see cref="Unit"/>
        /// from its <see cref="UnitType"/> and <see cref="Faction"/>.
        /// </summary>
        /// <param name="type">The <see cref="UnitType"/> of the <see cref="Unit"/> to be created.</param>
        /// <param name="faction">The <see cref="Faction"/> which creates the <see cref="Unit"/>.</param>
        /// <returns>The newly created <see cref="Unit"/>.</returns>
        internal Unit Create(UnitType type, Faction faction)
        {
            Unit unit = new Unit(nextUnitID++, type, faction);
            unit.Moved += unitMovedEventHandler;
            unit.Died += unitDiedEventHandler;

            events.Add(unit, Event.Created);

            return unit;
        }

        /// <summary>
        /// Gets a <see cref="Unit"/> of this <see cref="UnitRegistry"/> from its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the <see cref="Unit"/> to be found.</param>
        /// <returns>
        /// The <see cref="Unit"/> with that identifier, or <c>null</c> if the identifier is invalid.
        /// </returns>
        public Unit FindFromID(int id)
        {
            for (int i = 0; i < units.Count; ++i)
                if (units[i].ID == id)
                    return units[i];

            return null;
        }

        /// <summary>
        /// Updates the <see cref="Unit"/>s in this <see cref="UnitRegistry"/> for a frame of the game.
        /// </summary>
        /// <param name="timeDeltaInSeconds">The time elapsed since the last frame, in seconds.</param>
        /// <remarks>
        /// Used by <see cref="World"/>.
        /// </remarks>
        internal void Update(float timeDeltaInSeconds)
        {
            foreach (KeyValuePair<Unit, Event> @event in events)
            {
                bool hasBeenCreated = ((@event.Value & Event.Created) == Event.Created);
                bool hasMoved = ((@event.Value & Event.Moved) == Event.Moved);
                bool hasDied = ((@event.Value & Event.Died) == Event.Died);

                if (hasDied)
                {
                    if (!hasBeenCreated) Remove(@event.Key);
                    continue;
                }

                if (hasBeenCreated) Add(@event.Key);
                if (hasMoved) UpdateZone(@event.Key);
            }

            events.Clear();

            for (int i = 0; i < units.Count; ++i)
                units[i].Update(timeDeltaInSeconds);
        }

        #region Private Collection Modification
        private void Add(Unit unit)
        {
            units.Add(unit);
            AddToZone(unit);
        }

        private void AddToZone(Unit unit)
        {
            Point zoneCoords = GetClampedZoneCoords(unit.Position);
            zones[zoneCoords.X, zoneCoords.Y].Add(unit);
            unit.lastKnownPosition = unit.Position;
        }

        private void Remove(Unit unit)
        {
            units.Remove(unit);
            RemoveFromZone(unit);
        }

        private void RemoveFromZone(Unit unit)
        {
            Point zoneCoords = GetClampedZoneCoords(unit.lastKnownPosition);
            zones[zoneCoords.X, zoneCoords.Y].Remove(unit);
        }

        private void UpdateZone(Unit unit)
        {
            Point oldZoneCoords = GetClampedZoneCoords(unit.lastKnownPosition);
            Point newZoneCoords = GetClampedZoneCoords(unit.Position);

            if (newZoneCoords != oldZoneCoords)
            {
                zones[oldZoneCoords.X, oldZoneCoords.Y].Remove(unit);
                zones[newZoneCoords.X, newZoneCoords.Y].Add(unit);
            }

            unit.lastKnownPosition = unit.Position;
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
        /// Gets an enumerator that iterates over the <see cref="Unit"/>s in this registry.
        /// </summary>
        /// <returns>A new <see cref="Unit"/> enumerator.</returns>
        public List<Unit>.Enumerator GetEnumerator()
        {
            return units.GetEnumerator();
        }

        /// <summary>
        /// Gets the <see cref="Unit"/>s which are in a given rectangular area.
        /// </summary>
        /// <param name="area">The area in which to check.</param>
        /// <returns>A sequence of <see cref="Unit"/>s in that area.</returns>
        public IEnumerable<Unit> InArea(Rectangle area)
        {
            if (!world.Bounds.Intersects(area))
                yield break;

            Point minZoneCoords = GetClampedZoneCoords(area.Origin);
            Point maxZoneCoords = GetClampedZoneCoords(area.Max);

            for (int x = minZoneCoords.X; x <= maxZoneCoords.X; ++x)
            {
                for (int y = minZoneCoords.Y; y <= maxZoneCoords.Y; ++y)
                {
                    Zone zone = zones[x, y];
                    for (int i = 0; i < zone.Count; ++i)
                    {
                        Unit unit = zone[i];
                        if (area.ContainsPoint(unit.lastKnownPosition))
                            yield return unit;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="Unit"/>s which are in a given circular area.
        /// </summary>
        /// <param name="area">The area in which to check.</param>
        /// <returns>A sequence of <see cref="Unit"/>s in that area.</returns>
        public IEnumerable<Unit> InArea(Circle area)
        {
            if (!Intersection.Test(world.Bounds, area))
                yield break;

            Rectangle rectangle = area.BoundingRectangle;

            Point minZoneCoords = GetClampedZoneCoords(rectangle.Origin);
            Point maxZoneCoords = GetClampedZoneCoords(rectangle.Max);

            for (int x = minZoneCoords.X; x <= maxZoneCoords.X; ++x)
            {
                for (int y = minZoneCoords.Y; y <= maxZoneCoords.Y; ++y)
                {
                    Zone zone = zones[x, y];
                    for (int i = 0; i < zone.Count; ++i)
                    {
                        Unit unit = zone[i];
                        if (area.ContainsPoint(unit.lastKnownPosition))
                            yield return unit;
                    }
                }
            }
        }
        #endregion
        #endregion

        #region Explicit Members
        #region IEnumerable<Unit> Members
        IEnumerator<Unit> IEnumerable<Unit>.GetEnumerator()
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
