﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Game.Simulation.Components.Serialization;

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
        public Sellable(Entity entity) : base(entity) { }
        #endregion

        #region Properties
        [Mandatory]
        public float AlageneValue
        {
            get { return alageneValue; }
            set { alageneValue = value; }
        }

        [Mandatory]
        public float AladdiumValue
        {
            get { return aladdiumValue; }
            set { aladdiumValue = value; }
        }
        #endregion
    }
}
