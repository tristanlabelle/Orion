using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Math;

namespace Orion.GameLogic.Tasks
{
    /// <summary>
    /// A <see cref="Task"/> which makes a <see cref="Unit"/> build a building of a given type.
    /// </summary>
    [Serializable]
    public sealed class Build : Task
    {
        #region Fields
        private readonly Unit builder;
        private readonly UnitType buildingType;
        private readonly Vector2 buildPosition;
        private Move move;
        private float secondsSpentBuilding = 0;
        private bool hasBegunBuilding = false;
        private bool hasEnded = false;
        #endregion

        #region Constructors
        public Build(Unit builder, Vector2 buildPosition, UnitType unitToBuild)
        {
            Argument.EnsureNotNull(builder, "builder");
            if (!builder.HasSkill<Skills.Build>())
                throw new ArgumentException("Cannot build without the build skill.", "builder");

            this.builder = builder;
            this.buildingType = unitToBuild;
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
            get { return hasEnded; }
        }
        #endregion

        #region Methods
        public override void Update(float timeDelta)
        {
            if (hasEnded) return;

            if (!move.HasEnded)
            {
                move.Update(timeDelta);
                return;
            }

            if (!hasBegunBuilding)
            {
                int aladdiumCost = builder.Faction.GetStat(buildingType, UnitStat.AladdiumCost);
                int alageneCost = builder.Faction.GetStat(buildingType, UnitStat.AlageneCost);

                if (builder.Faction.AladdiumAmount >= aladdiumCost
                    && builder.Faction.AlageneAmount >= alageneCost)
                {
                    builder.Faction.AladdiumAmount -= aladdiumCost;
                    builder.Faction.AlageneAmount -= alageneCost;
                    hasBegunBuilding = true;
                }
                else
                {
                    Console.WriteLine("Not Enough Ressources");
                    System.Diagnostics.Debug.Fail("Not Enough Ressources");
                    return;
                }
            }

            if (hasBegunBuilding)
            {
                secondsSpentBuilding += timeDelta;
                float maxHealth = builder.Faction.GetStat(buildingType, UnitStat.MaxHealth);
                float buildingSpeed = builder.GetStat(UnitStat.BuildingSpeed);
                if (secondsSpentBuilding * buildingSpeed > maxHealth)
                {
                    Unit building = builder.faction.CreateUnit(buildingType);
                    building.Position = buildPosition;
                    hasEnded = true;
                }
            }
        }
        #endregion
    }
}
