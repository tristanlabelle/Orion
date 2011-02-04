﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Game.Simulation.Components.Serialization;
using Orion.Engine;

namespace Orion.Game.Simulation.Components
{
    public class Energy : Component
    {
        #region Fields
        public static readonly EntityStat MaximumEnergyStat = new EntityStat(typeof(Energy), StatType.Integer, "Energy", "Énergie");
        public static readonly EntityStat RegenerationRateStat = new EntityStat(typeof(Energy), StatType.Real, "RegenerationRate", "Vitesse de régénération");

        private int maximumEnergy;
        private float spentEnergy;
        private float regenerationRate;
        #endregion

        #region Constructors
        public Energy(Entity e)
            : base(e)
        { }
        #endregion

        #region Properties
        [Mandatory]
        public int MaximumEnergy
        {
            get { return maximumEnergy; }
            set { maximumEnergy = value; }
        }

        [Persistent]
        public int CurrentEnergy
        {
            get { return (int)(maximumEnergy - spentEnergy); }
            set { spentEnergy = maximumEnergy - value; }
        }
        #endregion

        #region Methods
        public override void Update(SimulationStep step)
        {
            spentEnergy -= regenerationRate * step.TimeDeltaInSeconds;
            if (spentEnergy < 0)
                spentEnergy = 0;
        }

        public bool TrySpendEnergy(int amount)
        {
            if (amount > CurrentEnergy)
                return false;
            spentEnergy += amount;
            return true;
        }
        #endregion
    }
}
