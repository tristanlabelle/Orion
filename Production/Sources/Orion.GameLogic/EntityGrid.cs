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
        #region Queries
        public Entity GetEntityAt(Point point)
        {
            return grid[point.X, point.Y];
        }

        public bool IsFree(Point point)
        {
            return GetEntityAt(point) == null;
        }

        public bool IsFree(Region region)
        {
            for (int x = region.MinX; x < region.ExclusiveMaxX; ++x)
            {
                for (int y = region.MinY; y < region.ExclusiveMaxY; ++y)
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

            Region region = entity.GridRegion;

            for (int x = region.MinX; x < region.ExclusiveMaxX; ++x)
            {
                for (int y = region.MinY; y < region.ExclusiveMaxY; ++y)
                {
                    //Debug.Assert(world.Terrain.IsWalkable(new Point(x, y)), "Adding unit to non-walkable tile.");
                    //Debug.Assert(grid[x, y] == null, "Overwriting {0}.".FormatInvariant(grid[x, y]));
                    grid[x, y] = entity;
                }
            }
        }

        public void Remove(Entity entity, Region region)
        {
            Argument.EnsureNotNull(entity, "entity");

            for (int x = region.MinX; x < region.ExclusiveMaxX; ++x)
            {
                for (int y = region.MinY; y < region.ExclusiveMaxY; ++y)
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
            Remove(entity, entity.GridRegion);
        }
        #endregion
        #endregion
    }
}
