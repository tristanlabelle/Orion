using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Game.Simulation.Components.Serialization;

namespace Orion.Game.Simulation.Components
{
    public class Train : Component
    {
        #region Properties
        public static readonly EntityStat SpeedMultiplierStat = new EntityStat(typeof(Train), StatType.Real, "Speed Multiplier", "Multiplicateur de vitesse");

        [Mandatory] private float speedMultiplier;
        [Mandatory] private HashSet<string> trainableTypes = new HashSet<string>();
        #endregion

        #region Constructors
        public Train(Entity entity) : base(entity) { }
        #endregion

        #region Properties
        public float SpeedMultiplier
        {
            get { return speedMultiplier; }
            set { speedMultiplier = value; }
        }

        public ICollection<string> TrainableTypes
        {
            get { return trainableTypes; }
        }
        #endregion

        #region Methods
        public bool Supports(Entity type)
        {
            return trainableTypes.Contains(type.GetComponent<Identity>().Name);
        }
        #endregion
    }
}
