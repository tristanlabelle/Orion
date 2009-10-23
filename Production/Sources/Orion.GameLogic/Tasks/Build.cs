using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Math;

namespace Orion.GameLogic.Tasks
{
    public sealed class Build : Task
    {
        #region Fields
        private readonly Unit builder;
        private readonly UnitType unitTypeToBuild;
        private readonly Vector2 buildPosition;
        private float secondsSpentBuilding = 0;
        private Move move;
        private bool buildHaveBegin = false;
        private bool unitConstructed = false;
        #endregion

        #region Constructors
        public Build(Unit builder, Vector2 buildPosition, UnitType unitToBuild)
        {
            this.builder = builder;
            this.unitTypeToBuild = unitToBuild;
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
                    int aladdiumCost = builder.Faction.GetStat(unitTypeToBuild, UnitStat.AladdiumCost);
                    int alageneCost = builder.Faction.GetStat(unitTypeToBuild, UnitStat.AlageneCost);

                    if (builder.Faction.AladdiumAmount >= aladdiumCost
                        && builder.Faction.AlageneAmount >= alageneCost)
                    {
                        builder.Faction.AladdiumAmount -= aladdiumCost;
                        builder.Faction.AlageneAmount -= alageneCost;
                        buildHaveBegin = true;
                    }
                    else
                    {
                        Console.WriteLine("Not Enough Ressources");
                        System.Diagnostics.Debug.Fail("Not Enough Ressources");
                        return;
                    }
                }

                if (buildHaveBegin)
                {
                    if (BuildingIsOver(timeDelta))
                    {
                        Unit unitBuilded = builder.faction.CreateUnit(unitTypeToBuild);
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
            float maxHealth = builder.Faction.GetStat(unitTypeToBuild, UnitStat.MaxHealth);
            float creationSpeed = builder.Faction.GetStat(unitTypeToBuild, UnitStat.CreationSpeed);
            if (secondsSpentBuilding >= maxHealth / creationSpeed)
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
