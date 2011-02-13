using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Game.Simulation.Components.Serialization;
using OpenTK;

namespace Orion.Game.Simulation.Components
{
    public class Train : Component
    {
        #region Fields
        public static readonly Stat SpeedMultiplierStat = new Stat(typeof(Train), StatType.Real, "SpeedMultiplier");

        private float speedMultiplier;
        private Vector2 rallyPoint;
        private HashSet<string> trainableTypes = new HashSet<string>();
        #endregion

        #region Constructors
        public Train(Entity entity) : base(entity) { }
        #endregion

        #region Properties
        [Mandatory]
        public float SpeedMultiplier
        {
            get { return speedMultiplier; }
            set { speedMultiplier = value; }
        }

        [Mandatory]
        public ICollection<string> TrainableTypes
        {
            get { return trainableTypes; }
        }

        public Vector2 RallyPoint
        {
            get { return rallyPoint; }
            set { rallyPoint = value; }
        }
        #endregion

        #region Methods
        public bool Supports(Entity type)
        {
            return trainableTypes.Contains(type.Components.Get<Identity>().Name);
        }
        #endregion
    }
}
