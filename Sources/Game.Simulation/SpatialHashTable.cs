using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Collections;
using Orion.Game.Simulation.Components;
using Orion.Engine;
using System.Diagnostics;
using OpenTK;
using Orion.Engine.Geometry;

namespace Orion.Game.Simulation
{
    /// <summary>
    /// A spatial hashing collection which allows fast queries for <see cref="Spatial"/> components.
    /// </summary>
    internal sealed class SpatialHashTable
    {
        #region Fields
        private static readonly int maxEntitySize = 4;
        private static readonly float maxEntityExtent = maxEntitySize * 0.5f;

        /// <summary>
        /// The size, in tiles, of the square regions which map to a bucket
        /// in the spatial hash table.
        /// This also defines the exclusive maximum size of entities.
        /// </summary>
        private const int bucketRegionSize = 8;
        private const int minimumBucketCount = 50;

        /// <summary>
        /// The load factor of the spatial hash table when the hinted maximum population is reached.
        /// A bigger value takes less memory but does not perform as well.
        /// </summary>
        private const double maximumPopulationLoadFactor = 5.0;

        private readonly BufferPool<Spatial> bufferPool = new BufferPool<Spatial>(AllocateBuffer);

        /// <summary>
        /// Buckets of the spatial hash table.
        /// The entity centers are associated to a region of a certain
        /// size, then the coordinates of that region are hashed
        /// to obtain the bucket index of the entity.
        /// </summary>
        private readonly PooledList<Spatial>[] buckets;

        private readonly int bucketIndexingHashingPrime;
        #endregion

        #region Constructors
        public SpatialHashTable(Size worldSize, int maximumPopulationHint)
        {
            Argument.EnsurePositive(maximumPopulationHint, "maximumPopulationHint");

            int bucketCount = HashingPrimes.GetNext((int)Math.Max(minimumBucketCount, maximumPopulationHint / maximumPopulationLoadFactor));
            this.buckets = new PooledList<Spatial>[bucketCount];
            this.bucketIndexingHashingPrime = HashingPrimes.GetNext(worldSize.Width);

            for (int i = 0; i < buckets.Length; ++i)
                buckets[i] = new PooledList<Spatial>(bufferPool);
        }
        #endregion

        #region Methods
        public void Add(Spatial spatial)
        {
            Argument.EnsureNotNull(spatial, "spatial");

            if (spatial.Width > maxEntitySize || spatial.Height > maxEntitySize)
            {
                string message = "Spatial component of entity '{0}' exceeds maximum entity size ({1})."
                    .FormatInvariant(spatial.Entity, maxEntitySize);
                throw new ArgumentException(message, "spatial");
            }

            GetBucket(spatial.Position, spatial.Size).Add(spatial);
        }

        public void Remove(Spatial spatial)
        {
            Argument.EnsureNotNull(spatial, "spatial");

            bool wasRemoved = GetBucket(spatial.Position, spatial.Size).Remove(spatial);
            Debug.Assert(wasRemoved, "Entity was not found in spatial hash table bucket.");
        }
        
        public void UpdatePosition(Spatial spatial, Vector2 previousPosition, Vector2 newPosition)
        {
            var previousBucket = GetBucket(previousPosition, spatial.Size);
            var newBucket = GetBucket(newPosition, spatial.Size);
            if (newBucket == previousBucket) return;

            bool wasRemoved = previousBucket.Remove(spatial);
            Debug.Assert(wasRemoved, "Entity was not found in spatial hash table bucket.");
            newBucket.Add(spatial);
        }

        /// <summary>
        /// Gets the <see cref="Spatial"/>s which are in a given rectangular area.
        /// </summary>
        /// <param name="area">The area in which to check.</param>
        /// <returns>A sequence of <see cref="Spatial"/>s intersecting that area.</returns>
        public IEnumerable<Spatial> EnumerateIntersecting(Rectangle area)
        {
            int minBucketX, minBucketY, inclusiveMaxBucketX, inclusiveMaxBucketY;
            GetBucketPoint(area.MinX - maxEntityExtent, area.MinY - maxEntityExtent, out minBucketX, out minBucketY);
            GetBucketPoint(area.MaxX + maxEntityExtent, area.MaxY + maxEntityExtent, out inclusiveMaxBucketX, out inclusiveMaxBucketY);

            for (int y = minBucketY; y <= inclusiveMaxBucketY; ++y)
            {
                for (int x = minBucketX; x <= inclusiveMaxBucketX; ++x)
                {
                    var bucket = GetBucket(x, y);
                    for (int i = 0; i < bucket.Count; ++i)
                    {
                        Spatial spatial = bucket[i];
                        if (Rectangle.Intersects(area, spatial.BoundingRectangle))
                            yield return spatial;
                    }
                }
            }
        }

        private void GetBucketPoint(float pointX, float pointY, out int bucketX, out int bucketY)
        {
            bucketX = (int)pointX / bucketRegionSize;
            bucketY = (int)pointY / bucketRegionSize;
        }

        private PooledList<Spatial> GetBucket(int bucketX, int bucketY)
        {
            unchecked
            {
                int hash = bucketX + bucketY * bucketIndexingHashingPrime;
                return buckets[(uint)hash % buckets.Length];
            }
        }

        private PooledList<Spatial> GetBucket(Vector2 position, Size size)
        {
            int bucketX, bucketY;
            GetBucketPoint(position.X + size.Width * 0.5f, position.Y + size.Height * 0.5f, out bucketX, out bucketY);
            return GetBucket(bucketX, bucketY);
        }

        private static Spatial[] AllocateBuffer(int minimumSize)
        {
            Argument.EnsurePositive(minimumSize, "minimumSize");

            uint allocationSize = PowerOfTwo.Ceiling((uint)minimumSize);
            if (allocationSize < 16) allocationSize = 16;
            return new Spatial[allocationSize];
        }
        #endregion
    }
}
