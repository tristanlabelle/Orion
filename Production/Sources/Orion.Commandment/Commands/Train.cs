using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.GameLogic;

namespace Orion.Commandment.Commands
{
    public class Train:Command
    {
          #region Fields
        private readonly List<Unit> identicalsBuildings;
        private readonly UnitType unitToCreate;
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
            this.identicalsBuildings = selectedsSameBuilding;
            this.unitToCreate = unitTobuild;
        }
        #endregion

        #region Methods
        public override void Execute()
        {
            int aladiumCost = base.SourceFaction.GetStat(unitToCreate, UnitStat.AladdiumCost);
            int alageneCost = base.SourceFaction.GetStat(unitToCreate, UnitStat.AlageneCost);
            for (int i = 0; i < identicalsBuildings.Count;i++ )
            {
                // If we don't have enought money to continue the production we stop.
                if (!(base.SourceFaction.AladdiumAmount >= (aladiumCost + aladiumCost * i)
                       && base.SourceFaction.AlageneAmount >= (alageneCost + alageneCost * i)))
                    break;
                identicalsBuildings[i].Task = new Orion.GameLogic.Tasks.Train(identicalsBuildings[i], unitToCreate);
            }

        }
        #endregion

        #region Proprieties
        public override IEnumerable<Unit> UnitsInvolved
        {
            get
            {
                foreach (Unit unit in identicalsBuildings)
                    yield return unit;
            }
        }
        #endregion

    }
}
