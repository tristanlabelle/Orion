﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Game.Simulation.Components.Serialization;

namespace Orion.Game.Simulation.Components
{
    public class Build : Component
    {
        #region Fields
        public static readonly EntityStat BuildSpeedStat = new EntityStat(typeof(Build), StatType.Real, "BuildSpeed", "Vitesse de construction");

        private readonly HashSet<UnitType> buildableTypes = new HashSet<UnitType>();
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
        public ICollection<UnitType> BuildableTypes
        {
            get { return buildableTypes; }
        }
        #endregion

        #region Methods
        public bool Supports(UnitType type)
        {
            return buildableTypes.Contains(type);
        }

        public void AddBuildableType(UnitType type)
        {
            buildableTypes.Add(type);
        }
        #endregion
    }
}
