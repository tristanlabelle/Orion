using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;

namespace Orion.Game.Simulation
{
    /// <summary>
    /// Stores the values describing a faction to be created.
    /// </summary>
    public sealed class FactionSettings
    {
        #region Fields
        private string name = "Unnamed";
        private ColorRgb color = Colors.PureRed;
        private int aladdiumAmount;
        private int alageneAmount;
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the name of the faction.
        /// </summary>
        public string Name
        {
            get { return name; }
            set
            {
                Argument.EnsureNotNull(value, "name");
                name = value;
            }
        }

        /// <summary>
        /// Accesses the color of the faction.
        /// </summary>
        public ColorRgb Color
        {
            get { return color; }
            set { color = ColorRgb.Clamp(value); }
        }

        /// <summary>
        /// Accesses the initial amount of aladdium the faction should have.
        /// </summary>
        public int AladdiumAmount
        {
            get { return aladdiumAmount; }
            set
            {
                Argument.EnsurePositive(value, "AladdiumAmount");
                aladdiumAmount = value;
            }
        }

        /// <summary>
        /// Accesses the initial amount of alagene the faction should have.
        /// </summary>
        public int AlageneAmount
        {
            get { return alageneAmount; }
            set
            {
                Argument.EnsurePositive(value, "AlageneAmount");
                alageneAmount = value;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Creates a faction according to these settings in a given world.
        /// </summary>
        /// <param name="world">The world in which the faction is to be created.</param>
        /// <returns>The newly created faction.</returns>
        public Faction Create(World world)
        {
            Argument.EnsureNotNull(world, "world");

            Faction faction = world.CreateFaction(name, color);
            faction.AladdiumAmount = aladdiumAmount;
            faction.AlageneAmount = alageneAmount;

            return faction;
        }
        #endregion
    }
}
