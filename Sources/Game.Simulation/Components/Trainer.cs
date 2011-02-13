using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Game.Simulation.Components.Serialization;
using OpenTK;
using Orion.Engine;

namespace Orion.Game.Simulation.Components
{
    /// <summary>
    /// Allows an <see cref="Entity"/> to train other <see cref="Entity">entities</see>.
    /// </summary>
    public sealed class Trainer : Component
    {
        #region Fields
        public static readonly Stat SpeedMultiplierStat = new Stat(typeof(Trainer), StatType.Real, "SpeedMultiplier");

        private float speedMultiplier = 1;
        private Vector2 rallyPoint;
        private HashSet<string> trainableTypes = new HashSet<string>();
        #endregion

        #region Constructors
        public Trainer(Entity entity) : base(entity) { }
        #endregion

        #region Properties
        [Mandatory]
        public float SpeedMultiplier
        {
            get { return speedMultiplier; }
            set
            {
                Argument.EnsureNotNaN(value, "SpeedMultiplier");
                speedMultiplier = value;
            }
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
        public bool Supports(Entity prototype)
        {
            return trainableTypes.Contains(prototype.Components.Get<Identity>().Name);
        }
        #endregion
    }
}
