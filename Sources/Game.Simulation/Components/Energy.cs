using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Game.Simulation.Components.Serialization;
using Orion.Engine;

namespace Orion.Game.Simulation.Components
{
    /// <summary>
    /// A <see cref="Component"/> which provides an <see cref="Entity"/> with magic energy,
    /// making it possible for it to use magic skills.
    /// </summary>
    public sealed class Energy : Component
    {
        #region Fields
        public static readonly Stat MaximumEnergyStat = new Stat(typeof(Energy), StatType.Integer, "MaximumEnergy");
        public static readonly Stat RegenerationRateStat = new Stat(typeof(Energy), StatType.Real, "RegenerationRate");

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

        public float RegenerationRate
        {
            get { return regenerationRate; }
            set { regenerationRate = value; }
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
