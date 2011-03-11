using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Game.Simulation;
using Orion.Engine.Collections;
using System.Collections.ObjectModel;
using Orion.Engine;

namespace Orion.Game.Presentation
{
    /// <summary>
    /// Manages the selection groups. Selection groups being a set of units assigned to
    /// an index key so that they can quickly be re-selected at once.
    /// </summary>
    public sealed class SelectionGroupManager : ReadOnlyCollection<ICollection<Entity>>
    {
        #region Constructors
        public SelectionGroupManager(World world, int groupCount)
            : base(CreateGroups(world, groupCount))
        {
            world.EntityDied += OnEntityDied;
        }

        private static IList<ICollection<Entity>> CreateGroups(World world, int count)
        {
            Argument.EnsureNotNull(world, "world");
            Argument.EnsurePositive(count, "count");

            Func<Entity, bool> validator = entity =>
                entity != null && entity.IsAlive && entity.World == world;

            var groups = new ICollection<Entity>[count];
            for (int i = 0; i < groups.Length; ++i)
                groups[i] = new ValidatingCollection<Entity>(new HashSet<Entity>(), validator);

            return groups;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Sets the contents a a selection group by its index.
        /// </summary>
        /// <param name="index">The selection group index.</param>
        /// <param name="entities">The entities to be set to this selection group.</param>
        public void Set(int index, IEnumerable<Entity> entities)
        {
            Argument.EnsureNotNull(entities, "entities");

            var group = this[index];
            group.Clear();
            group.AddRange(entities);
        }

        private void OnEntityDied(World sender, Entity entity)
        {
            // Premature optimization: cast to an array to prevent the allocation of an enumerator.
            foreach (var group in (ICollection<Entity>[])Items)
                group.Remove(entity);
        }
        #endregion
    }
}
