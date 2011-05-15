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
        public static readonly Stat SpeedStat = new Stat(typeof(Trainer), StatType.Real, "Speed");

        private float speed = 1;
        private Vector2? rallyPoint;
        private readonly HashSet<string> trainableTypes = new HashSet<string>();
        #endregion

        #region Constructors
        public Trainer(Entity entity) : base(entity) { }
        #endregion

        #region Properties
        [Persistent(true)]
        public float Speed
        {
            get { return speed; }
            set
            {
                Argument.EnsureNotNaN(value, "Speed");
                speed = value;
            }
        }

        [Persistent(true)]
        public ICollection<string> TrainableTypes
        {
            get { return trainableTypes; }
        }

        /// <summary>
        /// Accesses the rally point of the trained <see cref="Entity">entities</see>.
        /// This is <c>null</c> if no rally point is set.
        /// </summary>
        public Vector2? RallyPoint
        {
            get { return rallyPoint; }
            set { rallyPoint = value; }
        }

        /// <summary>
        /// Gets a value indicating if this <see cref="Entity"/> has a rally point.
        /// </summary>
        public bool HasRallyPoint
        {
            get { return rallyPoint.HasValue; }
        }
        #endregion

        #region Methods
        public bool Supports(Entity prototype)
        {
            Argument.EnsureNotNull(prototype, "prototype");
            return trainableTypes.Contains(prototype.Identity.Name);
        }
        #endregion
    }
}
