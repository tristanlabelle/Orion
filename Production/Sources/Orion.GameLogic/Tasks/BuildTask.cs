using System;
using System.Linq;
using System.Diagnostics;
using OpenTK.Math;
using ExtractAlageneSkill = Orion.GameLogic.Skills.ExtractAlageneSkill;

namespace Orion.GameLogic.Tasks
{
    /// <summary>
    /// A <see cref="Task"/> which makes a <see cref="Unit"/> build a building of a given type.
    /// </summary>
    [Serializable]
    public sealed class BuildTask : Task
    {
        #region Fields
        private readonly BuildingPlan buildingPlan;
        private MoveTask move;
        private bool hasEnded = false;
        #endregion

        #region Constructors
        public BuildTask(Unit builder, BuildingPlan buildingPlan)
            : base(builder)
        {
            Argument.EnsureNotNull(builder, "builder");
            if (!builder.HasSkill<Skills.BuildSkill>())
                throw new ArgumentException("Cannot build without the build skill.", "builder");
            Argument.EnsureNotNull(buildingPlan, "buildingPlan");

            this.buildingPlan = buildingPlan;
            this.move = MoveTask.ToNearRegion(builder, buildingPlan.GridRegion);
        }
        #endregion

        #region Properties
        public override string Description
        {
            get { return "Building {0}".FormatInvariant(buildingPlan.BuildingType); }
        }

        public override bool HasEnded
        {
            get { return hasEnded; }
        }
        #endregion

        #region Methods
        protected override void DoUpdate(UpdateInfo info)
        {
            if (!move.HasEnded)
            {
                move.Update(info);
                return;
            }

            // Test if we're in the building's surrounding area
            if (!Region.AreAdjacentOrIntersecting(buildingPlan.GridRegion, Unit.GridRegion))
            {
                hasEnded = true;
                return;
            }

            if (buildingPlan.IsBuildingCreated)
            {
                if (buildingPlan.Building.Health < buildingPlan.Building.MaxHealth)
                    Unit.TaskQueue.OverrideWith(new RepairTask(Unit, buildingPlan.Building));
                hasEnded = true;
                return;
            }

            if (!Unit.World.IsFree(buildingPlan.GridRegion, Unit.CollisionLayer))
            {
                Debug.WriteLine("Cannot build {0}, spot occupied.".FormatInvariant(buildingPlan.BuildingType));
                hasEnded = true;
                return;
            }

            int aladdiumCost = Unit.Faction.GetStat(buildingPlan.BuildingType, UnitStat.AladdiumCost);
            int alageneCost = Unit.Faction.GetStat(buildingPlan.BuildingType, UnitStat.AlageneCost);
            bool hasEnoughResources = Unit.Faction.AladdiumAmount >= aladdiumCost
                && Unit.Faction.AlageneAmount >= alageneCost;

            if (!hasEnoughResources)
            {
                Debug.WriteLine("Not enough resources to build {0}.".FormatInvariant(buildingPlan.BuildingType));
                hasEnded = true;
                return;
            }

            Unit.Faction.AladdiumAmount -= aladdiumCost;
            Unit.Faction.AlageneAmount -= alageneCost;

            buildingPlan.CreateBuilding();

            if (buildingPlan.Building.HasSkill<ExtractAlageneSkill>())
            {
                ResourceNode node = Unit.World.Entities
                    .OfType<ResourceNode>()
                    .First(n => n.BoundingRectangle.ContainsPoint(buildingPlan.Location));
                node.Extractor = buildingPlan.Building;
            }

            Unit.TaskQueue.OverrideWith(new RepairTask(Unit, buildingPlan.Building));
            hasEnded = true;
        }
        #endregion
    }
}
