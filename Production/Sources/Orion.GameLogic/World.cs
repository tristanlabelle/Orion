using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Orion.GameLogic
{
    /// <summary>
    /// Represents the game map: its terrain and units.
    /// </summary>
    [Serializable]
    public sealed class World
    {
        #region Nested Types
        private sealed class UnitCollection : Collection<Unit>
        {
            private readonly World world;

            public UnitCollection(World world)
            {
                Argument.EnsureNotNull(world, "world");
                this.world = world;
            }

            protected override void InsertItem(int index, Unit item)
            {
                Argument.EnsureNotNull(item, "item");
                Argument.EnsureEqual(item.World, world, "item.World");
                if (!Contains(item)) base.InsertItem(index, item);
            }
        }
        #endregion

        #region Instance
        #region Fields
        private readonly UnitCollection units;
        #endregion

        #region Constructors
        public World()
        {
            units = new UnitCollection(this);
        }
        #endregion

        #region Events
        #endregion

        #region Properties
        /// <summary>
        /// Gets the collection of <see cref="Unit"/>s part of this <see cref="World"/>.
        /// </summary>
        public ICollection<Unit> Units
        {
            get { return units; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Updates this <see cref="World"/> and its <see cref="Unit"/>s for a frame.
        /// </summary>
        /// <param name="timeDelta">The time elapsed since the last frame.</param>
        public void Update(float timeDelta)
        {
            foreach (Unit unit in Units)
                unit.Update(timeDelta);
        }
        #endregion
        #endregion
    }
}
