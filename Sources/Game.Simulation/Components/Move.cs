using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Game.Simulation.Components.Serialization;

namespace Orion.Game.Simulation.Components
{
    public class Move : Component
    {
        #region Fields
        public static readonly EntityStat SpeedStat = new EntityStat(typeof(Move), StatType.Real, "Speed", "Vitesse");
        public static readonly EntityStat AccelerationStat = new EntityStat(typeof(Move), StatType.Real, "Acceleration", "Accélération");

        [Mandatory] private float speed;
        [Mandatory] private float acceleration;
        #endregion

        #region Constructors
        public Move(Entity entity) : base(entity) { }
        #endregion

        #region Properties
        public float Speed
        {
            get { return speed; }
            set { speed = value; }
        }

        public float Acceleration
        {
            get { return acceleration; }
            set { acceleration = value; }
        }
        #endregion
    }
}
