using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Simulation
{
    /// <summary>
    /// Holds a grid of reference to entities and prevents overlaps.
    /// </summary>
    public sealed class EntityGrid
    {
        #region Fields
        private readonly Entity[,] grid;
        #endregion

        #region Constructors
        public EntityGrid(Size size)
        {
            this.grid = new Entity[size.Width, size.Height];
        }
        #endregion

        #region Properties
        public Size Size
        {
            get { return new Size(grid.GetLength(0), grid.GetLength(1)); }
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
            Debug.Assert(entity.HasComponent<Spatial>(), "An immaterial entity is being added to the grid.");
            Debug.Assert(entity.GetComponent<Spatial>().CollisionLayer != CollisionLayer.None,
                "A non-collidable entity is being added to the grid.");
            Debug.Assert(entity.IsAliveInWorld, "An entity that is not alive in the world is being added to the grid.");

            foreach (Point point in region.Points)
            {
                Debug.Assert(this[point] == null, "Cell {0} is occupied by {1}.".FormatInvariant(point, this[point]));
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
            Debug.Assert(entity.HasComponent<Spatial>(), "An immaterial entity is being removed from the grid.");
            if (entity.GetComponent<Spatial>().CollisionLayer == CollisionLayer.None)
            {
                Debug.Fail("A non-collidable entity is being removed from the grid.");
                return;
            }

            foreach (Point point in region.Points)
            {
#if DEBUG
                if (this[point] != entity)
                {
                    Debug.Fail("Cell {0} should have been occupied by {1} but was occupied by {2}."
                        .FormatInvariant(point, entity, this[point]));
                }
#endif
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
