﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Game.Simulation.Components.Serialization;
using Orion.Engine;

namespace Orion.Game.Simulation.Components
{
    public class TimedExistence : Component
    {
        #region Fields
        public static readonly EntityStat LifeSpanStat = new EntityStat(typeof(TimedExistence), StatType.Real, "LifeSpan", "Durée de vie");

        private float lifeSpan;
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

        [Transient]
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
        public override void Update(SimulationStep step)
        {
            elapsedTime += step.TimeDeltaInSeconds;
            if (elapsedTime >= lifeSpan)
                Entity.World.Entities.Remove(Entity);
        }
        #endregion
    }
}
