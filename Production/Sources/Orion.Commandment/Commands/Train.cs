using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.GameLogic;

namespace Orion.Commandment.Commands
{
    class Train:Command
    {
          #region Fields
        private readonly List<Unit> BuldingsIdentical;
        private readonly UnitType unitTobuild;
        #endregion

        #region Constructors
        /// <summary>
        /// Command implemented to build.
        /// </summary>
        /// <param name="selectedUnit">The Builder</param>
        /// <param name="position">Where To build</param>
        /// <param name="unitTobuild">What to build</param>
        public Train(List<Unit> selectedsSameBuilding,UnitType unitTobuild, Faction faction)
            : base(faction)
        {
            this.BuldingsIdentical = selectedsSameBuilding;
            this.unitTobuild = unitTobuild;
        }
        #endregion

        #region Methods
        public override void Execute()
        {
           
               // BuldingsIdentical.Task = new Orion.GameLogic.Tasks.Build(BuldingsIdentical,position,unitTobuild);
        }
        #endregion
    
    }
}
