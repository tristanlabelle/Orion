using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Math;

namespace Orion.GameLogic.Tasks
{
    public class Build : Task
    {
          #region Fields
        private readonly Unit builder;
        private readonly UnitType unitToBuild;
        private readonly Vector2 buildPosition;
        private const float secondsToBuild = 1;
        private float secondsSpentBuilding = 0;
        private Move move;
        private bool buildHaveBegin = false;
        private bool unitConstructed = false;
        #endregion

        #region Constructors
        public Build(Unit builder, Vector2 buildPosition, UnitType unitToBuild)
        {
            this.builder = builder;
            this.unitToBuild = unitToBuild;
            this.buildPosition = buildPosition;
            this.move = new Move(builder, this.buildPosition);
        }
        #endregion

        #region Properties
        public override string Description
        {
            get { return "Building"; }
        }

        public override bool HasEnded
        {
            get
            {
                return unitConstructed;
            }
        }
        #endregion

        #region Methods
        public override void Update(float timeDelta)
        {
            if (move.HasEnded)
            {
                if (!buildHaveBegin)
                {
                    if (builder.faction.AladdiumAmount >= unitToBuild.AladdiumPrice
                    && builder.faction.AlageneAmount >= unitToBuild.AlagenePrice)
                    {
                        builder.faction.AladdiumAmount -= unitToBuild.AladdiumPrice;
                        builder.faction.AlageneAmount -= unitToBuild.AlagenePrice;
                        buildHaveBegin = true;
                    }
                    else
                    {
                        Console.WriteLine("Not Enought Ressources");
                        return;
                    }
                }

                if (buildHaveBegin)
                {
                    if (BuildingIsOver(timeDelta))
                    {
                        Unit unitBuilded = builder.faction.CreateUnit(unitToBuild);
                        unitBuilded.Position = buildPosition;
                        unitConstructed = true;
                        
                    }
                }
            }
            else
            {
                move.Update(timeDelta);
            }
        }

        private bool BuildingIsOver(float timeDelta)
        {
            if (secondsSpentBuilding >= secondsToBuild)
            {
                return true;
            }
            else
            {
                secondsSpentBuilding += timeDelta;
                return false;
            }
        }
        #endregion
    }
}
