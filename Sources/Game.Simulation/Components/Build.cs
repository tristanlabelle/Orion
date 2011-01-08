using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Game.Simulation.Components
{
    public class Build : Component
    {
        #region Fields
        public static readonly EntityStat<float> BuildSpeedStat = new EntityStat<float>(typeof(Build), "BuildSpeed", "Vitesse de construction");

        private readonly IEnumerable<UnitType> buildableTypes;
        private float buildSpeed;
        #endregion

        #region Constructors
        public Build(Entity entity, float buildSpeed, IEnumerable<UnitType> buildableTypes)
            : base(entity)
        {
            this.buildableTypes = buildableTypes;
            this.buildSpeed = buildSpeed;
        }
        #endregion

        #region Properties
        public float BuildSpeed
        {
            get { return buildSpeed; }
        }

        public IEnumerable<UnitType> BuildableTypes
        {
            get { return buildableTypes; }
        }
        #endregion

        #region Methods
        public bool Supports(UnitType type)
        {
            return buildableTypes.Contains(type);
        }
        #endregion
    }
}
