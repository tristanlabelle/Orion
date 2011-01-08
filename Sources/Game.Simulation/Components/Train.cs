using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Game.Simulation.Components
{
    public class Train : Component
    {
        #region Properties
        public static readonly EntityStat<float> SpeedMultiplierStat = new EntityStat<float>(typeof(Train), "Speed Multiplier", "Multiplicateur de vitesse");

        #endregion
    }
}
