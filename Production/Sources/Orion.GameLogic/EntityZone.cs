using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Orion.GameLogic
{
    /// <summary>
    /// Represents a rectangular spatial subdivision used by an
    /// <see cref="EntityRegistry"/> to group nearby
    /// <see cref="Entity">entities</see> and optimize spatial queries.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    internal sealed class EntityZone
    {
        #region Fields
        private readonly BufferPool<Entity> bufferPool;
        private Entity[] unitBuffer;
        private int count;
        #endregion

        #region Constructors
        public EntityZone(BufferPool<Entity> bufferPool)
        {
            Argument.EnsureNotNull(bufferPool, "bufferPool");
            this.bufferPool = bufferPool;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the number of units in this zone.
        /// </summary>
        public int Count
        {
            get { return count; }
        }
        #endregion

        #region Indexers
        /// <summary>
        /// Gets a <see cref="Entity"/> from this <see cref="Zone"/> by its index.
        /// </summary>
        /// <param name="index">The index of the <see cref="Entity"/> to be retrieved.</param>
        /// <returns>The <see cref="Entity"/> at that index.</returns>
        public Entity this[int index]
        {
            get { return unitBuffer[index]; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Removes a <see cref="Entity"/> from this zone.
        /// </summary>
        /// <param name="item">The <see cref="Entity"/> to be removed.</param>
        /// <returns><c>True</c> if an <see cref="Entity"/> was removed, <c>false</c> if it wasn't found.</returns>
        public bool Remove(Entity entity)
        {
            int index = IndexOf(entity);
            if (index == -1) return false;
            RemoveAt(index);
            return true;
        }

        /// <summary>
        /// Gets the index of the first (and hopefully only) occurance
        /// of an <see cref="Entity"/> in this <see cref="Zone"/>.
        /// </summary>
        /// <param name="entity">The <see cref="Entity"/> to be found.</param>
        /// <returns>The index of <paramref name="entity"/>, or <c>-1</c> if it wasn't found.</returns>
        public int IndexOf(Entity entity)
        {
            for (int i = 0; i < count; ++i)
                if (unitBuffer[i] == entity)
                    return i;
            return -1;
        }

        /// <summary>
        /// Removes an <see cref="Entity"/> at an index in this <see cref="Zone"/>
        /// </summary>
        /// <param name="index">The index of the entity to be removed.</param>
        public void RemoveAt(int index)
        {
            if (index < count - 1) unitBuffer[index] = unitBuffer[count - 1];
            unitBuffer[count - 1] = null;
            --count;

            if (count == 0)
            {
                // Return our buffer to the pool as we do not need it anymore.
                bufferPool.Add(unitBuffer);
                unitBuffer = null;
            }
            else if (count <= unitBuffer.Length / 3)
            {
                // The zone is getting quite empty, attempt to get a smaller
                // buffer so there's less wasted space and other pool clients
                // can benefit from our big buffer.
                Entity[] newUnitBuffer = bufferPool.GetPooled(count);
                if (newUnitBuffer != null && newUnitBuffer.Length < unitBuffer.Length)
                {
                    Array.Copy(unitBuffer, newUnitBuffer, count);
                    bufferPool.Add(unitBuffer);
                    unitBuffer = newUnitBuffer;
                }
            }
        }

        /// <summary>
        /// Adds a <see cref="Entity"/> to this <see cref="Zone"/>
        /// The <see cref="Entity"/> is assumed to be absent.
        /// </summary>
        /// <param name="item">The <see cref="Entity"/> to be added.</param>
        public void Add(Entity entity)
        {
            if (unitBuffer == null)
            {
                unitBuffer = bufferPool.Get(1);
            }
            else if (count == unitBuffer.Length)
            {
                Entity[] newUnitBuffer = bufferPool.Get(unitBuffer.Length + 1);
                Array.Copy(unitBuffer, newUnitBuffer, unitBuffer.Length);
                bufferPool.Add(unitBuffer);
                unitBuffer = newUnitBuffer;
            }

            unitBuffer[count] = entity;
            ++count;
        }
        #endregion
    }
}
