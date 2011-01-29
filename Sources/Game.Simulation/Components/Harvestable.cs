using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Game.Simulation.Components.Serialization;

namespace Orion.Game.Simulation.Components
{
    public class Harvestable : Component
    {
        #region Fields
        public static readonly EntityStat HarvestRateStat = new EntityStat(typeof(Harvestable), StatType.Real, "HarvestRate", "Vitesse de récolte");

        private int aladdiumLeft;
        private int alageneLeft;
        private float harvestRate;
        #endregion

        #region Constructors
        public Harvestable(Entity e)
            : base(e)
        { }
        #endregion

        #region Properties
        [Persistent]
        public int AladdiumLeft
        {
            get { return aladdiumLeft; }
            set { aladdiumLeft = value; }
        }

        [Persistent]
        public int AlageneLeft
        {
            get { return alageneLeft; }
            set { alageneLeft = value; }
        }

        [Mandatory]
        public float HarvestRate
        {
            get { return harvestRate; }
            set { harvestRate = value; }
        }
        #endregion
    }
}
