using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Math;

namespace Orion.GameLogic.Tasks
{
    public class Train : Task
    {
   #region Fields
        private readonly Unit building;
        private readonly UnitType unitTypeToBuild;
        private float secondsSpentTraining = 0;
        private bool trainHaveBegin = false;
        private bool trainingCompleted = false;
        #endregion

        #region Constructors
        public Train(Unit building, UnitType unitToBuild)
        {
            this.building = building;
            this.unitTypeToBuild = unitToBuild;
        }
        #endregion

        #region Properties
        public override string Description
        {
            get { return "Training"; }
        }

        public override bool HasEnded
        {
            get
            {
                return trainingCompleted;
            }
        }
        #endregion

        #region Methods
        public override void Update(float timeDelta)
        {
            if (!trainHaveBegin)
            {
                int aladdiumCost = building.Faction.GetStat(unitTypeToBuild, UnitStat.AladdiumCost);
                int alageneCost = building.Faction.GetStat(unitTypeToBuild, UnitStat.AlageneCost);

                if (building.Faction.AladdiumAmount >= aladdiumCost
                    && building.Faction.AlageneAmount >= alageneCost)
                {
                    building.Faction.AladdiumAmount -= aladdiumCost;
                    building.Faction.AlageneAmount -= alageneCost;
                    trainHaveBegin = true;
                }
                else
                {
                    Console.WriteLine("Not Enough Ressources");
                    System.Diagnostics.Debug.Fail("Not Enough Ressources");
                    return;
                }
            }

            if (trainHaveBegin)
            {
                if (BuildingIsOver(timeDelta))
                {
                    Unit unitBuilded = building.faction.CreateUnit(unitTypeToBuild);
                    Vector2 newPosition = new Vector2(building.Position.X + 2, building.Position.Y + 2);
                    // If the new assigned position is unavalible put it over the building
                    if (!building.World.Terrain.IsWalkable(newPosition))
                        newPosition = building.Position;

                    unitBuilded.Position = newPosition;
                    trainingCompleted = true;

                }
            }
        }
           
        

        private bool BuildingIsOver(float timeDelta)
        {
            float maxHealth = building.Faction.GetStat(unitTypeToBuild, UnitStat.MaxHealth);
            float creationSpeed = building.Faction.GetStat(unitTypeToBuild, UnitStat.CreationSpeed);
            if (secondsSpentTraining >= maxHealth / creationSpeed)
            {
                return true;
            }
            else
            {
                secondsSpentTraining += timeDelta;
                return false;
            }
        }
        #endregion
    }
}
