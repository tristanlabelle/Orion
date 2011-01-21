using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Game.Simulation.Components
{
    public class Sellable : Component
    {
        #region Fields
        public static readonly EntityStat AlageneValueStat = new EntityStat(typeof(Sellable), StatType.Integer, "AlageneValue", "Valeur d'alagène");
        public static readonly EntityStat AladdiumValueStat = new EntityStat(typeof(Sellable), StatType.Integer, "AladdiumValue", "Valeur d'aladdium");

        private float alageneValue;
        private float aladdiumValue;
        #endregion

        #region Constructors
        public Sellable(Entity entity, float alageneValue, float aladdiumValue)
            : base(entity)
        {
            this.aladdiumValue = aladdiumValue;
            this.alageneValue = alageneValue;
        }
        #endregion

        #region Properties
        public float AlageneValue
        {
            get { return alageneValue; }
        }

        public float AladdiumValue
        {
            get { return aladdiumValue; }
        }
        #endregion
    }
}
