using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.GameLogic;
using OpenTK.Math;

namespace Orion.Commandment.Commands
{
    class Build: Command
    {
        #region Fields
        private readonly Unit constructor;
        private readonly Vector2 position;
        private readonly UnitType unitTobuild;
        #endregion

        #region Constructors
        /// <summary>
        /// Command implemented to build.
        /// </summary>
        /// <param name="selectedUnit">The Builder</param>
        /// <param name="position">Where To build</param>
        /// <param name="unitTobuild">What to build</param>
        public Build(Unit selectedUnit, Vector2 position,UnitType unitTobuild)
            : base(selectedUnit.Faction)
        {
            this.constructor = selectedUnit;
            this.unitTobuild = unitTobuild;
            Argument.EnsureNotNull(position, "position");
            this.position = position;
        }
        #endregion

        #region Methods
        public override void Execute()
        {
           
                constructor.Task = new Orion.GameLogic.Tasks.Build(constructor,position,unitTobuild);
        }
        #endregion
    

}
}
