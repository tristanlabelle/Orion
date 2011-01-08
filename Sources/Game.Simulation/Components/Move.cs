using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Game.Simulation.Components
{
    public class Move : Component
    {
        #region Fields
        public static readonly EntityStat<float> SpeedStat = new EntityStat<float>(typeof(Move), "Speed", "Vitesse");
        public static readonly EntityStat<float> AccelerationStat = new EntityStat<float>(typeof(Move), "Acceleration", "Accélération");

        private float speed;
        private float acceleration;
        #endregion

        #region Constructors
        public Move(Entity entity, float speed, float acceleration)
            : base(entity)
        {
            this.speed = speed;
            this.acceleration = acceleration;
        }
        #endregion

        #region Properties
        public float Speed
        {
            get { return speed; }
        }

        public float Acceleration
        {
            get { return acceleration; }
        }
        #endregion
    }
}
