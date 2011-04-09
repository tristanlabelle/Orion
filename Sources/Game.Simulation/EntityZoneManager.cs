using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Engine.Geometry;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Simulation
{
    /// <summary>
    /// Manages collection of entities optimized for spacial requests.
    /// </summary>
    public sealed class EntityZoneManager
    {
        #region Fields
        /// <summary>
        /// The width and height of zones, in tiles.
        /// </summary>
        private static readonly int ZoneSize = 8;

        private readonly Size worldSize;
        private readonly BufferPool<Entity> bufferPool;

        /// <summary>
        /// Holds the entity zones in use.
        /// An entity can be in a single zone at a time, based on its center.
        /// </summary>
        private readonly PooledList<Entity>[,] zones;
        #endregion

        #region Constructors
        public EntityZoneManager(Size worldSize)
        {
            this.worldSize = worldSize;
            this.bufferPool = new BufferPool<Entity>(AllocateBuffer);

            int zoneCountX = (worldSize.Width + ZoneSize - 1) / ZoneSize;
            int zoneCountY = (worldSize.Height + ZoneSize - 1) / ZoneSize;
            this.zones = new PooledList<Entity>[zoneCountX, zoneCountY];
            for (int x = 0; x < zones.GetLength(0); ++x)
                for (int y = 0; y < zones.GetLength(1); ++y)
                    zones[x, y] = new PooledList<Entity>(bufferPool);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the number of zones there are along the x axis.
        /// </summary>
        private int ZoneCountX
        {
            get { return zones.GetLength(0); }
        }

        /// <summary>
        /// Gets the number of zones there are along the y axis.
        /// </summary>
        private int ZoneCountY
        {
            get { return zones.GetLength(1); }
        }
        #endregion

        #region Methods
        private Point GetZonePoint(Vector2 point)
        {
            int x = Clamp((int)point.X / ZoneSize, 0, ZoneCountX - 1);
            int y = Clamp((int)point.Y / ZoneSize, 0, ZoneCountY - 1);
            return new Point(x, y);
        }

        private int Clamp(int value, int inclusiveMin, int inclusiveMax)
        {
            if (value < inclusiveMin) return inclusiveMin;
            if (value > inclusiveMax) return inclusiveMax;
            return value;
        }

        #region Queries
        /// <summary>
        /// Gets the <see cref="Entity"/>s which are in a given rectangular area.
        /// </summary>
        /// <param name="area">The area in which to check.</param>
        /// <returns>A sequence of <see cref="Entity"/>s intersecting that area.</returns>
        public IEnumerable<Entity> Intersecting(Rectangle area)
        {
            int minZoneX = Clamp((int)((area.MinX - Entity.MaxSize * 0.5f) / ZoneSize), 0, ZoneCountX - 1);
            int minZoneY = Clamp((int)((area.MinY - Entity.MaxSize * 0.5f) / ZoneSize), 0, ZoneCountY - 1);
            int inclusiveMaxZoneX = Clamp((int)((area.MaxX + Entity.MaxSize * 0.5f) / ZoneSize), 0, ZoneCountX - 1);
            int inclusiveMaxZoneY = Clamp((int)((area.MaxY + Entity.MaxSize * 0.5f) / ZoneSize), 0, ZoneCountY - 1);

            for (int y = minZoneY; y <= inclusiveMaxZoneY; ++y)
            {
                for (int x = minZoneX; x <= inclusiveMaxZoneX; ++x)
                {
                    PooledList<Entity> zone = zones[x, y];
                    for (int i = 0; i < zone.Count; ++i)
                    {
                        Entity entity = zone[i];
                        Spatial spatial = entity.Spatial;
                        if (spatial != null && Rectangle.Intersects(area, entity.Spatial.BoundingRectangle))
                            yield return entity;
                    }
                }
            }
        }

        public IEnumerable<Entity> Intersecting(Vector2 point)
        {
            Rectangle rectangle = new Rectangle(point.X, point.Y, 0, 0);
            return Intersecting(rectangle);
        }
        #endregion

        #region Updating
        /// <summary>
        /// Adds an entity to the zone it belongs to.
        /// Assumes the entity is not present.
        /// </summary>
        /// <param name="entity">The entity to be added.</param>
        public void Add(Entity entity)
        {
            Argument.EnsureNotNull(entity, "entity");

            Point zonePoint = GetZonePoint(entity.Center);
            Add(entity, zonePoint);
        }

        private void Add(Entity entity, Point zonePoint)
        {
            var zone = zones[zonePoint.X, zonePoint.Y];

#if DEBUG
            // #if'd so Contains is not executed in release.
            if (zone.Contains(entity))
            {
                Debug.Fail("The zone at {0} already contains the entity to be added, {1}."
                    .FormatInvariant(zonePoint, entity));
            }
#endif

            zone.Add(entity);
        }

        /// <summary>
        /// Removes an entity to the zone it belongs to.
        /// Assumes the entity is present.
        /// </summary>
        /// <param name="entity">The entity to be removed.</param>
        public void Remove(Entity entity)
        {
            Argument.EnsureNotNull(entity, "entity");
            Remove(entity, entity.Center);
        }

        /// <summary>
        /// Removes an entity from the zone it belonged to when its center was at a given position.
        /// </summary>
        /// <param name="entity">The entity to be removed.</param>
        /// <param name="center">The center of the entity to be removed.</param>
        private void Remove(Entity entity, Vector2 center)
        {
            Point zonePoint = GetZonePoint(center);
            Remove(entity, zonePoint);
        }

        private void Remove(Entity entity, Point zonePoint)
        {
            var zone = zones[zonePoint.X, zonePoint.Y];
            zone.Remove(entity);

            // Here there used to be an assert about zones not containing entities
            // but it was triggered by a legit use of the Transporter component.
        }

        /// <summary>
        /// Updates the zone in which an entity is according to its old and new position.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="oldCenter">The last entity center known to this manager.</param>
        public void UpdateZone(Entity entity, Vector2 oldCenter)
        {
            Argument.EnsureNotNull(entity, "entity");

            Point oldZonePoint = GetZonePoint(oldCenter);
            Point newZonePoint = GetZonePoint(entity.Center);

            if (newZonePoint != oldZonePoint)
            {
                Remove(entity, oldZonePoint);
                Add(entity, newZonePoint);
            }
        }
        #endregion

        #region Buffer Allocator
        private static Entity[] AllocateBuffer(int minimumSize)
        {
            Argument.EnsurePositive(minimumSize, "minimumSize");

            uint allocationSize = PowerOfTwo.Ceiling((uint)minimumSize);
            if (allocationSize < 16) allocationSize = 16;
            return new Entity[allocationSize];
        }
        #endregion
        #endregion
    }
}
