using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Game.Simulation.Components.Serialization;

namespace Orion.Game.Simulation.Components
{
    /// <summary>
    /// Marks an <see cref="Entity"/> as being able to be sold for resources.
    /// </summary>
    public sealed class Sellable : Component
    {
        #region Fields
        public static readonly Stat AlageneValueStat = new Stat(typeof(Sellable), StatType.Integer, "AlageneValue");
        public static readonly Stat AladdiumValueStat = new Stat(typeof(Sellable), StatType.Integer, "AladdiumValue");

        private float alageneValue;
        private float aladdiumValue;
        #endregion

        #region Constructors
        public Sellable(Entity entity) : base(entity) { }
        #endregion

        #region Properties
        [Persistent(true)]
        public float AlageneValue
        {
            get { return alageneValue; }
            set { alageneValue = value; }
        }

        [Persistent(true)]
        public float AladdiumValue
        {
            get { return aladdiumValue; }
            set { aladdiumValue = value; }
        }
        #endregion
    }
}
