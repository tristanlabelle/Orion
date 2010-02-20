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

        /// <summary>
        /// A temporary set that is reused between queries to avoid unnecessary allocations.
        /// </summary>
        private readonly HashSet<Entity> queryTempSet = new HashSet<Entity>();
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

            int minX = Clamp((int)(rectangle.MinX / zoneSize.X), 0, ZoneCountX - 1);
            int minY = Clamp((int)(rectangle.MinY / zoneSize.Y), 0, ZoneCountY - 1);
            int maxX = Clamp((int)Math.Ceiling(rectangle.MaxX / zoneSize.X), 0, ZoneCountX - 1);
            int maxY = Clamp((int)Math.Ceiling(rectangle.MaxY / zoneSize.Y), 0, ZoneCountY - 1);

            return Region.FromMinInclusiveMax(
                new Point(minX, minY),
                new Point(maxX, maxY));
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
        /// <returns>A sequence of <see cref="Entity"/>s in that area.</returns>
        public IEnumerable<Entity> Intersecting(Rectangle area)
        {
            queryTempSet.Clear();
            AddIntersectingToTempSet(area);
            Entity[] entities = queryTempSet.ToArray();
            queryTempSet.Clear();
            return entities;
        }

        private void AddIntersectingToTempSet(Rectangle area)
        {
            Region zoneRegion = GetZoneRegion(area);
            foreach (Point point in zoneRegion.Points)
            {
                PooledList<Entity> zone = zones[point.X, point.Y];
                for (int i = 0; i < zone.Count; ++i)
                {
                    Entity entity = zone[i];
                    if (Rectangle.Intersects(area, entity.BoundingRectangle))
                        queryTempSet.Add(entity);
                }
            }
        }

        public IEnumerable<Entity> Intersecting(Vector2 point)
        {
            Vector2 zoneSize = ZoneSize;
            int zoneX = Clamp((int)(point.X / zoneSize.X), 0, ZoneCountX - 1);
            int zoneY = Clamp((int)(point.Y / zoneSize.Y), 0, ZoneCountY - 1);
            PooledList<Entity> zone = zones[zoneX, zoneY];
            for (int i = 0; i < zone.Count; ++i)
            {
                Entity entity = zone[i];
                if (entity.BoundingRectangle.ContainsPoint(point))
                    yield return entity;
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
            foreach (Point point in zoneRegion.Points)
                AddToZone(point, entity);
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
            foreach (Point point in zoneRegion.Points)
                RemoveFromZone(point, entity);
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

            uint allocationSize = PowerOfTwo.Ceiling((uint)minimumSize);
            if (allocationSize < 16) allocationSize = 16;
            return new Entity[allocationSize];
        }
        #endregion
        #endregion
    }
}
