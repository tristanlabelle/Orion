using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

using Orion.Geometry;

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
            #region Fields
            private readonly World world;
            #endregion

            #region Constructors
            public UnitCollection(World world)
            {
                Argument.EnsureNotNull(world, "world");
                this.world = world;
            }
            #endregion

            #region Methods
            protected override void InsertItem(int index, Unit item)
            {
                Argument.EnsureNotNull(item, "item");
                Argument.EnsureEqual(item.World, world, "item.World");
                if (!Contains(item)) base.InsertItem(index, item);
            }

            protected override void SetItem(int index, Unit item)
            {
                throw new NotSupportedException();
            }
            #endregion
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

        /// <summary>
        /// Gets the width of this <see cref="World"/>, in tiles.
        /// </summary>
        public int Width
        {
            get
            {
                // To be later replaced by Terrain.Width.
                return 100;
            }
        }

        /// <summary>
        /// Gets the height of this <see cref="World"/>, in tiles.
        /// </summary>
        public int Height
        {
            get
            {
                // To be later replaced by Terrain.Width.
                return 100;
            }
        }

        /// <summary>
        /// Gets a <see cref="Rectangle"/> that bounds this <see cref="World"/>, in tiles.
        /// </summary>
        public Rectangle Bounds
        {
            get { return new Rectangle(0, 0, Width, Height); }
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
