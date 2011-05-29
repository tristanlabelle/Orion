using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Game.Simulation.Components.Serialization;
using Orion.Engine;

namespace Orion.Game.Simulation.Components
{
    public sealed class TimedExistence : Component
    {
        #region Fields
        public static readonly Stat LifeSpanStat = new Stat(typeof(TimedExistence), StatType.Real, "LifeSpan");

        private float lifeSpan = 1;
        private float elapsedTime;
        #endregion

        #region Constructors
        public TimedExistence(Entity entity)
            : base(entity)
        { }
        #endregion

        #region Properties
        [Persistent]
        public float LifeSpan
        {
            get { return lifeSpan; }
            set { lifeSpan = value; }
        }

        public float ElapsedTime
        {
            get { return elapsedTime; }
            set { elapsedTime = value; }
        }

        public float TimeLeft
        {
            get { return lifeSpan - elapsedTime; }
            set
            {
                Argument.EnsurePositive(value, "value");
                Argument.EnsureLowerOrEqual(value, lifeSpan, "value");
                elapsedTime = lifeSpan - value;
            }
        }
        #endregion

        #region Methods
        public override int GetStateHashCode()
        {
            return lifeSpan.GetHashCode() ^ elapsedTime.GetHashCode();
        }

        protected override void Update(SimulationStep step)
        {
            elapsedTime += step.TimeDeltaInSeconds;
            if (elapsedTime >= lifeSpan)
                Entity.World.Entities.Remove(Entity);
        }
        #endregion
    }
}
