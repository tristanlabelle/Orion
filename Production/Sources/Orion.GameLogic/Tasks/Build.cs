using System;
using System.Linq;
using System.Diagnostics;
using OpenTK.Math;
using ExtractAlageneSkill = Orion.GameLogic.Skills.ExtractAlagene;

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
        private readonly BuildingPlan buildingPlan;
        private Move move;
        private bool hasEnded = false;
        #endregion

        #region Constructors
        public Build(Unit builder, BuildingPlan buildingPlan)
        {
            Argument.EnsureNotNull(builder, "builder");
            if (!builder.HasSkill<Skills.Build>())
                throw new ArgumentException("Cannot build without the build skill.", "builder");
            Argument.EnsureNotNull(buildingPlan, "buildingPlan");

            this.buildingPlan = buildingPlan;
            this.builder = builder;
            this.move = new Move(builder, buildingPlan.Location);
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
        protected override void DoUpdate(float timeDelta)
        {
            if (hasEnded) return;

            if (!move.HasEnded)
            {
                move.Update(timeDelta);
                return;
            }

            if (!buildingPlan.GridRegion.Contains(builder.GridRegion.Min))
            {
                //Unable To reach Destination
                hasEnded = true;
                return;
            }

            if (buildingPlan.IsBuildingCreated)
            {
                if (buildingPlan.Building.Health < buildingPlan.Building.MaxHealth)
                    builder.CurrentTask = new Repair(builder, buildingPlan.Building);
                hasEnded = true;
                return;
            }

            if (!builder.World.IsFree(buildingPlan.GridRegion))
            {
                Debug.WriteLine("Cannot build {0}, spot occupied.".FormatInvariant(buildingPlan.BuildingType));
                hasEnded = true;
                return;
            }

            int aladdiumCost = builder.Faction.GetStat(buildingPlan.BuildingType, UnitStat.AladdiumCost);
            int alageneCost = builder.Faction.GetStat(buildingPlan.BuildingType, UnitStat.AlageneCost);
            bool hasEnoughResources = builder.Faction.AladdiumAmount >= aladdiumCost
                && builder.Faction.AlageneAmount >= alageneCost;

            if (!hasEnoughResources)
            {
                Debug.WriteLine("Not enough resources to build {0}.".FormatInvariant(buildingPlan.BuildingType));
                hasEnded = true;
                return;
            }

            builder.Faction.AladdiumAmount -= aladdiumCost;
            builder.Faction.AlageneAmount -= alageneCost;

            buildingPlan.CreateBuilding();

            if (buildingPlan.Building.HasSkill<ExtractAlageneSkill>())
            {
                ResourceNode node = builder.World.Entities
                    .OfType<ResourceNode>()
                    .First(n => n.BoundingRectangle.ContainsPoint(buildingPlan.Location));
                node.Extractor = buildingPlan.Building;
            }

            builder.CurrentTask = new Repair(builder, buildingPlan.Building);
        }
        #endregion
    }
}
