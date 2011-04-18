using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Collections;
using Orion.Engine;
using OpenTK;
using Orion.Game.Simulation.Components;
using Orion.Engine.Geometry;
using System.Diagnostics;

namespace Orion.Game.Simulation
{
    /// <summary>
    /// Holds <see cref="Entity">entities</see> with a <see cref="Spatial"/> component
    /// in a way that is optimized for spatial queries.
    /// </summary>
    public sealed class SpatialManager
    {
        #region Fields
        private readonly SpatialGrid groundGrid;
        private readonly SpatialGrid airGrid;
        private readonly SpatialHashTable hashTable;
        #endregion

        #region Constructors
        public SpatialManager(Size worldSize, int maximumPopulationHint)
        {
            Argument.EnsurePositive(maximumPopulationHint, "maximumPopulationHint");

            this.groundGrid = new SpatialGrid(worldSize);
            this.airGrid = new SpatialGrid(worldSize);
            this.hashTable = new SpatialHashTable(worldSize, maximumPopulationHint);
        }
        #endregion

        #region Methods
        public Spatial GetGroundGridObstacleAt(Point point)
        {
            return groundGrid[point];
        }

        public Spatial GetAirGridObstacleAt(Point point)
        {
            return airGrid[point];
        }

        public Spatial GetGridObstacleAt(Point point, CollisionLayer collisionLayer)
        {
            switch(collisionLayer)
            {
                case CollisionLayer.Ground: return groundGrid[point];
                case CollisionLayer.Air: return airGrid[point];
                default: return null;
            }
        }

        public Spatial GetTopmostAt(Vector2 point)
        {
            return hashTable.EnumerateIntersecting(new Rectangle(point, Vector2.Zero))
                .WithMaxOrDefault(s => s.CollisionLayer);
        }

        public IEnumerable<Spatial> Intersecting(Rectangle area)
        {
            return hashTable.EnumerateIntersecting(area)
                .NonDeferred();
        }

        public IEnumerable<Spatial> Intersecting(Vector2 point)
        {
            return Intersecting(new Rectangle(point, Vector2.Zero));
        }

        public IEnumerable<Spatial> Intersecting(Circle circle)
        {
            return hashTable.EnumerateIntersecting(circle.BoundingRectangle)
                .Where(spatial => Intersection.Test(circle, spatial.BoundingRectangle))
                .NonDeferred();
        }

        #region Internal Interface for the Spatial Component
        /// <summary>
        /// Adds a <see cref="Spatial"/> to this <see cref="SpatialManager"/>'s collections.
        /// </summary>
        /// <param name="spatial">The <see cref="Spatial"/> to be added.</param>
        /// <remarks>
        /// Used by <see cref="Spatial"/>.
        /// </remarks>
        internal void Add(Spatial spatial)
        {
            Argument.EnsureNotNull(spatial, "spatial");

            hashTable.Add(spatial);
            AddToGrid(spatial, spatial.CollisionLayer);
        }

        /// <summary>
        /// Removes a <see cref="Spatial"/> from this <see cref="SpatialManager"/>'s collections.
        /// </summary>
        /// <param name="spatial">The <see cref="Spatial"/> to be removed.</param>
        /// <remarks>
        /// Used by <see cref="Spatial"/>.
        /// </remarks>
        internal void Remove(Spatial spatial)
        {
            Argument.EnsureNotNull(spatial, "spatial");

            hashTable.Remove(spatial);
            RemoveFromGrid(spatial, spatial.CollisionLayer);
        }

        internal void UpdatePosition(Spatial spatial, Vector2 previousPosition, Vector2 newPosition)
        {
            hashTable.UpdatePosition(spatial, previousPosition, newPosition);

            if (spatial.CollisionLayer == CollisionLayer.Ground)
                groundGrid.UpdatePosition(spatial, previousPosition, newPosition);
            else if (spatial.CollisionLayer == CollisionLayer.Air)
                airGrid.UpdatePosition(spatial, previousPosition, newPosition);
        }

        internal void UpdateCollisionLayer(Spatial spatial, CollisionLayer previousLayer, CollisionLayer newLayer)
        {
            RemoveFromGrid(spatial, previousLayer);
            AddToGrid(spatial, newLayer);
        }
        #endregion

        internal void CommitDeferredChanges()
        {
            
        }

        private void AddToGrid(Spatial spatial, CollisionLayer collisionLayer)
        {
            if (collisionLayer == CollisionLayer.Ground)
                groundGrid.Add(spatial, spatial.GridRegion);
            else if (collisionLayer == CollisionLayer.Air)
                airGrid.Add(spatial, spatial.GridRegion);
        }

        private void RemoveFromGrid(Spatial spatial, CollisionLayer collisionLayer)
        {
            if (collisionLayer == CollisionLayer.Ground)
                groundGrid.Remove(spatial, spatial.GridRegion);
            else if (collisionLayer == CollisionLayer.Air)
                airGrid.Remove(spatial, spatial.GridRegion);
        }
        #endregion
    }
}
