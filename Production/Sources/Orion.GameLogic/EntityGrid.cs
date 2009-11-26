using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Geometry;
using System.Diagnostics;

namespace Orion.GameLogic
{
    /// <summary>
    /// Holds a grid of reference to entities and prevents overlaps.
    /// </summary>
    public sealed class EntityGrid
    {
        #region Fields
        private readonly Terrain terrain;
        private readonly Entity[,] grid;
        #endregion

        #region Constructors
        public EntityGrid(Terrain terrain)
        {
            Argument.EnsureNotNull(terrain, "terrain");

            this.terrain = terrain;
            this.grid = new Entity[terrain.Size.Width, terrain.Size.Height];
        }
        #endregion

        #region Properties
        public Size Size
        {
            get { return terrain.Size; }
        }
        #endregion

        #region Indexers
        public Entity this[Point point]
        {
            get { return GetEntityAt(point); }
        }
        #endregion

        #region Methods
        #region GetRegion
        public Region GetRegion(Rectangle rectangle)
        {
            int minX = Math.Max(0, (int)rectangle.MinX);
            int minY = Math.Max(0, (int)rectangle.MinY);
            int maxX = Math.Min(Size.Width - 1, (int)rectangle.MaxX);
            int maxY = Math.Min(Size.Height - 1, (int)rectangle.MaxY);

            return Region.FromMinExclusiveMax(new Point(minX, minY), new Point(maxX + 1, maxY + 1));
        }

        public Region GetRegion(Entity entity)
        {
            Argument.EnsureNotNull(entity, "entity");
            return GetRegion(entity.CollisionRectangle);
        }
        #endregion

        #region Queries
        public Entity GetEntityAt(Point point)
        {
            return grid[point.X, point.Y];
        }

        public bool IsFree(Point point)
        {
            return !terrain.IsWalkable(point) && this[point] == null;
        }

        public bool IsFree(Region region)
        {
            for (int x = region.Min.X; x < region.ExclusiveMax.X; ++x)
            {
                for (int y = region.Min.Y; y < region.ExclusiveMax.Y; ++y)
                {
                    Point point = new Point(x, y);
                    if (!IsFree(point)) return false;
                }
            }
            return true;
        }
        #endregion

        #region Adding/Removal
        public void Add(Entity entity)
        {
            Argument.EnsureNotNull(entity, "entity");
            if (!entity.IsSolid)
            {
                throw new ArgumentException(
                    "The entity must be non-solid to be added to the entity grid.",
                    "entity");
            }

            Region region = GetRegion(entity.BoundingRectangle);

            for (int x = region.Min.X; x < region.ExclusiveMax.X; ++x)
            {
                for (int y = region.Min.Y; y < region.ExclusiveMax.Y; ++y)
                {
                    //Debug.Assert(world.Terrain.IsWalkable(new Point(x, y)), "Adding unit to non-walkable tile.");
                    //Debug.Assert(grid[x, y] == null, "Overwriting {0}.".FormatInvariant(grid[x, y]));
                    grid[x, y] = entity;
                }
            }
        }

        public void Remove(Entity entity, Rectangle boundingRectangle)
        {
            Argument.EnsureNotNull(entity, "entity");

            Region region = GetRegion(boundingRectangle);

            for (int x = region.Min.X; x < region.ExclusiveMax.X; ++x)
            {
                for (int y = region.Min.Y; y < region.ExclusiveMax.Y; ++y)
                {
                    //Debug.Assert(world.Terrain.IsWalkable(new Point(x, y)));
                    //Debug.Assert(grid[x, y] == entity, "There was no entity to remove.");
                    grid[x, y] = null;
                }
            }
        }

        public void Remove(Entity entity)
        {
            Argument.EnsureNotNull(entity, "entity");
            Remove(entity, entity.BoundingRectangle);
        }
        #endregion
        #endregion
    }
}
