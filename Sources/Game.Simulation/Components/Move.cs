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
        public static readonly Stat SpeedStat = new Stat(typeof(Move), StatType.Real, "Speed");
        public static readonly Stat AccelerationStat = new Stat(typeof(Move), StatType.Real, "Acceleration");

        private float speed;
        private float acceleration;
        #endregion

        #region Constructors
        public Move(Entity entity) : base(entity) { }
        #endregion

        #region Properties
        [Mandatory]
        public float Speed
        {
            get { return speed; }
            set { speed = value; }
        }

        [Mandatory]
        public float Acceleration
        {
            get { return acceleration; }
            set { acceleration = value; }
        }
        #endregion
    }
}
