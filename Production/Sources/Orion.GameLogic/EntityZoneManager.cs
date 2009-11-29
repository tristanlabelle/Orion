using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Math;
using Orion.Geometry;
using System.Diagnostics;

namespace Orion.GameLogic
{
    /// <summary>
    /// Manages collection of entities optimized for spacial requests.
    /// </summary>
    public sealed class EntityZoneManager
    {
        #region Fields
        private readonly Size worldSize;
        private readonly BufferPool<Entity> bufferPool;

        /// <summary>
        /// Holds the entity zones in use.
        /// An entity can be in multiple of those if its bounding rectangle straddles two or more zones.
        /// </summary>
        private readonly PooledList<Entity>[,] zones;
        #endregion

        #region Constructors
        public EntityZoneManager(Size worldSize)
        {
            this.worldSize = worldSize;
            this.bufferPool = new BufferPool<Entity>(AllocateBuffer);
            this.zones = new PooledList<Entity>[8, 8];
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

        /// <summary>
        /// Gets the size of a zone, in world units.
        /// </summary>
        private Vector2 ZoneSize
        {
            get
            {
                return new Vector2(
                    worldSize.Width / (float)ZoneCountX,
                    worldSize.Height / (float)ZoneCountY);
            }
        }
        #endregion

        #region Methods
        private Region GetZoneRegion(Rectangle rectangle)
        {
            Vector2 zoneSize = ZoneSize;

            int minX = Math.Max(0, (int)(rectangle.MinX / zoneSize.X));
            int minY = Math.Max(0, (int)(rectangle.MinY / zoneSize.Y));
            int maxX = Math.Min(ZoneCountX - 1, (int)(rectangle.MaxX / zoneSize.X));
            int maxY = Math.Min(ZoneCountY - 1, (int)(rectangle.MaxY / zoneSize.Y));

            return Region.FromMinInclusiveMax(
                new Point(minX, minY),
                new Point(maxX, maxY));
        }

        #region Queries
        /// <summary>
        /// Gets the <see cref="Entity"/>s which are in a given rectangular area.
        /// </summary>
        /// <param name="area">The area in which to check.</param>
        /// <returns>A sequence of <see cref="Entity"/>s in that area.</returns>
        public IEnumerable<Entity> InArea(Rectangle area)
        {
            Region zoneRegion = GetZoneRegion(area);
            for (int x = zoneRegion.Min.X; x < zoneRegion.ExclusiveMax.X; ++x)
            {
                for (int y = zoneRegion.Min.Y; y < zoneRegion.ExclusiveMax.Y; ++y)
                {
                    PooledList<Entity> zone = zones[x, y];
                    for (int i = 0; i < zone.Count; ++i)
                    {
                        Entity entity = zone[i];
                        if (area.ContainsPoint(entity.Center))
                            yield return entity;
                    }
                }
            }
        }
        #endregion

        #region Updating
        #region Adding
        public void Add(Entity entity)
        {
            Argument.EnsureNotNull(entity, "entity");

            Region zoneRegion = GetZoneRegion(entity.BoundingRectangle);
            Add(entity, zoneRegion);
        }

        private void Add(Entity entity, Region zoneRegion)
        {
            for (int x = zoneRegion.MinX; x < zoneRegion.ExclusiveMaxX; ++x)
            {
                for (int y = zoneRegion.MinY; y < zoneRegion.ExclusiveMaxY; ++y)
                {
                    Point point = new Point(x, y);
                    AddToZone(point, entity);
                }
            }
        }

        private void AddToZone(Point point, Entity entity)
        {
            PooledList<Entity> zone = zones[point.X, point.Y];
            Debug.Assert(!zone.Contains(entity), "The zone already contains that entity.");
            zone.Add(entity);
        }
        #endregion

        #region Removing
        public void Remove(Entity entity)
        {
            Argument.EnsureNotNull(entity, "entity");
            Remove(entity, entity.BoundingRectangle);
        }

        public void Remove(Entity entity, Rectangle boundingRectangle)
        {
            Argument.EnsureNotNull(entity, "entity");

            Region zoneRegion = GetZoneRegion(boundingRectangle);
            Remove(entity, zoneRegion);
        }

        private void Remove(Entity entity, Region zoneRegion)
        {
            for (int x = zoneRegion.MinX; x < zoneRegion.ExclusiveMaxX; ++x)
            {
                for (int y = zoneRegion.MinY; y < zoneRegion.ExclusiveMaxY; ++y)
                {
                    Point point = new Point(x, y);
                    RemoveFromZone(point, entity);
                }
            }
        }

        private void RemoveFromZone(Point point, Entity entity)
        {
            PooledList<Entity> zone = zones[point.X, point.Y];
            Debug.Assert(zone.Contains(entity), "The zone does not contain that entity.");
            zone.Remove(entity);
        }
        #endregion

        public void UpdateZone(Entity entity, Vector2 oldPosition)
        {
            Rectangle oldBoundingRectangle = new Rectangle(oldPosition.X, oldPosition.Y,
                entity.Size.Width, entity.Size.Height);
            Region oldZoneRegion = GetZoneRegion(oldBoundingRectangle);
            Region newZoneRegion = GetZoneRegion(entity.BoundingRectangle);

            if (newZoneRegion != oldZoneRegion)
            {
                Remove(entity, oldZoneRegion);
                Add(entity, newZoneRegion);
            }
        }
        #endregion

        #region Buffer Allocator
        private static Entity[] AllocateBuffer(int minimumSize)
        {
            Argument.EnsurePositive(minimumSize, "minimumSize");

            uint allocationSize = CeilingPowerOfTwo((uint)minimumSize);
            if (allocationSize < 16) allocationSize = 16;
            return new Entity[allocationSize];
        }

        private static uint CeilingPowerOfTwo(uint value)
        {
            // Source: http://graphics.stanford.edu/~seander/bithacks.html#RoundUpPowerOf2Float
            // Edge case 0 is handled by under and overflowing.
            --value;
            value |= value >> 1;
            value |= value >> 2;
            value |= value >> 4;
            value |= value >> 8;
            value |= value >> 16;
            return value + 1;
        }
        #endregion
        #endregion
    }
}
