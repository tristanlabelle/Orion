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
            get { return grid[point.X, point.Y]; }
            private set { grid[point.X, point.Y] = value; }
        }
        #endregion

        #region Methods
        #region Queries
        public bool IsFree(Point point)
        {
            return this[point] == null;
        }

        public bool IsFree(Region region)
        {
            return region.Points.All(point => IsFree(point));
        }
        #endregion

        #region Adding/Removal
        public void Add(Entity entity, Region region)
        {
            Argument.EnsureNotNull(entity, "entity");
            Debug.Assert(entity.CollisionLayer != CollisionLayer.None,
                "A non-collidable entity is being added to the grid.");
            Debug.Assert(entity.IsAlive, "A dead entity is being added to the grid.");

            foreach (Point point in region.Points)
            {
                Debug.Assert(this[point] == null,
                    "Cell {0} is occupied by {1}.".FormatInvariant(point, this[point]));
                this[point] = entity;
            }
        }

        public void Add(Entity entity)
        {
            Argument.EnsureNotNull(entity, "entity");
            Add(entity, entity.GridRegion);
        }

        public void Remove(Entity entity, Region region)
        {
            Argument.EnsureNotNull(entity, "entity");
            Debug.Assert(entity.CollisionLayer != CollisionLayer.None,
                "A non-collidable entity is being removed from the grid.");

            foreach (Point point in region.Points)
            {
                Debug.Assert(this[point] == entity,
                    "Cell {0} should have been occupied by {1} but was occupied by {2}."
                    .FormatInvariant(point, entity, this[point]));
                this[point] = null;
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
