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
    public sealed class Build : Component
    {
        #region Fields
        public static readonly Stat BuildSpeedStat = new Stat(typeof(Build), StatType.Real, "Speed");

        private readonly HashSet<string> buildableTypes = new HashSet<string>();
        private float speed;
        #endregion

        #region Constructors
        public Build(Entity entity) : base(entity) { }
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
        public bool Supports(Entity type)
        {
            return buildableTypes.Contains(type.Components.Get<Identity>().Name);
        }
        #endregion
    }
}
