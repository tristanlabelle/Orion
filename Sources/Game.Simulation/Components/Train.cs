using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Game.Simulation.Components
{
    public class Train : Component
    {
        #region Properties
        public static readonly EntityStat SpeedMultiplierStat = new EntityStat(typeof(Train), StatType.Real, "Speed Multiplier", "Multiplicateur de vitesse");

        private float speedMultiplier;
        private IEnumerable<UnitType> buildableTypes;
        #endregion

        #region Constructors
        public Train(Entity entity, float speedMultiplier, IEnumerable<UnitType> buildableTypes)
            : base(entity)
        {
            this.speedMultiplier = speedMultiplier;
            this.buildableTypes = buildableTypes;
        }
        #endregion

        #region Properties
        public float SpeedMultiplier
        {
            get { return speedMultiplier; }
        }
        #endregion
    }
}
