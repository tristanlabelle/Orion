using System;
using System.Linq;
using System.Diagnostics;
using OpenTK;
using Orion.Engine;
using Orion.Game.Simulation.Skills;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Simulation.Tasks
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
        #endregion

        #region Constructors
        public BuildTask(Unit buildingUnit, BuildingPlan buildingPlan)
            : base(buildingUnit)
        {
            Argument.EnsureNotNull(buildingUnit, "builder");
            Argument.EnsureNotNull(buildingPlan, "buildingPlan");

            Builder builder = buildingUnit.Components.TryGet<Builder>();
            if (builder == null || !builder.Supports(buildingPlan.BuildingType))
            {
                throw new ArgumentException("Builder {0} cannot build {1}."
                    .FormatInvariant(buildingUnit, buildingPlan.BuildingType));
            }

            this.buildingPlan = buildingPlan;
            this.move = MoveTask.ToNearRegion(buildingUnit, buildingPlan.GridRegion);
        }
        #endregion

        #region Properties
        public override string Description
        {
            get { return "Building {0}".FormatInvariant(buildingPlan.BuildingType); }
        }

        public BuildingPlan BuildingPlan
        {
            get { return buildingPlan; }
        }
        #endregion

        #region Methods
        protected override void DoUpdate(SimulationStep step)
        {
            if (!move.HasEnded)
            {
                move.Update(step);
                return;
            }

            // Test if we're in the building's surrounding area
            if (!Region.AreAdjacentOrIntersecting(buildingPlan.GridRegion, Unit.GridRegion))
            {
                MarkAsEnded();
                return;
            }

            if (buildingPlan.IsBuildingCreated)
            {
                if (buildingPlan.Building.Health < buildingPlan.Building.MaxHealth && Unit.TaskQueue.Count == 1)
                    Unit.TaskQueue.OverrideWith(new RepairTask(Unit, buildingPlan.Building));
                MarkAsEnded();
                return;
            }

            CollisionLayer layer = buildingPlan.BuildingType.Spatial.CollisionLayer;
            if (!Unit.World.IsFree(buildingPlan.GridRegion, layer))
            {
                string warning = "Pas de place pour construire le bâtiment {0}".FormatInvariant(buildingPlan.BuildingType.Name);
                Faction.RaiseWarning(warning);
                MarkAsEnded();
                return;
            }

            int aladdiumCost = Unit.Faction.GetStat(buildingPlan.BuildingType, BasicSkill.AladdiumCostStat);
            int alageneCost = Unit.Faction.GetStat(buildingPlan.BuildingType, BasicSkill.AlageneCostStat);
            bool hasEnoughResources = Unit.Faction.AladdiumAmount >= aladdiumCost
                && Unit.Faction.AlageneAmount >= alageneCost;

            if (!hasEnoughResources)
            {
                string warning = "Pas assez de ressources pour construire le bâtiment {0}"
                    .FormatInvariant(buildingPlan.BuildingType.Name);
                Faction.RaiseWarning(warning);
                MarkAsEnded();
                return;
            }

            Unit.Faction.AladdiumAmount -= aladdiumCost;
            Unit.Faction.AlageneAmount -= alageneCost;

            buildingPlan.CreateBuilding();

            Unit.TaskQueue.ReplaceWith(new RepairTask(Unit, buildingPlan.Building));
            MarkAsEnded();
        }
        #endregion
    }
}
