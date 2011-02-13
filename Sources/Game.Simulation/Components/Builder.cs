using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Game.Simulation.Components.Serialization;

namespace Orion.Game.Simulation.Components
{
    /// <summary>
    /// Provides an <see cref="Entity"/> with the capability to build structures.
    /// </summary>
    public sealed class Builder : Component
    {
        #region Fields
        public static readonly Stat BuildSpeedStat = new Stat(typeof(Builder), StatType.Real, "Speed");

        private readonly HashSet<string> buildableTypes = new HashSet<string>();
        private float speed;
        #endregion

        #region Constructors
        public Builder(Entity entity) : base(entity) { }
        #endregion

        #region Properties
        [Mandatory]
        public float Speed
        {
            get { return speed; }
            set { speed = value; }
        }

        [Persistent]
        public ICollection<string> BuildableTypes
        {
            get { return buildableTypes; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Tests if an <see cref="Entity"/> described by a given prototype can be built by the host <see cref="Entity"/>.
        /// </summary>
        /// <param name="prototype">The <see cref="Entity"/> prototype.</param>
        /// <returns>A value indicating if such <see cref="Entity"/> instances can be built.</returns>
        public bool Supports(Entity prototype)
        {
            return buildableTypes.Contains(prototype.Components.Get<Identity>().Name);
        }
        #endregion
    }
}
