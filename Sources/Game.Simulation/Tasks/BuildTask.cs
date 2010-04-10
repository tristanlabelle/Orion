﻿using System;
using System.Linq;
using System.Diagnostics;
using OpenTK.Math;
using Orion.Engine;
using Orion.Game.Simulation.Skills;

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
        public BuildTask(Unit builder, BuildingPlan buildingPlan)
            : base(builder)
        {
            Argument.EnsureNotNull(builder, "builder");
            Argument.EnsureNotNull(buildingPlan, "buildingPlan");

            BuildSkill buildSkill = builder.Type.TryGetSkill<BuildSkill>();
            if (buildSkill == null)
                throw new ArgumentException("Cannot build without the build skill.", "builder");
            if (!buildSkill.Supports(buildingPlan.BuildingType))
            {
                throw new ArgumentException("Builder {0} cannot build {1}."
                    .FormatInvariant(builder, buildingPlan.BuildingType));
            }

            this.buildingPlan = buildingPlan;
            this.move = MoveTask.ToNearRegion(builder, buildingPlan.GridRegion);
        }
        #endregion

        #region Properties
        public override string Description
        {
            get { return "Building {0}".FormatInvariant(buildingPlan.BuildingType); }
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
                if (buildingPlan.Building.Health < buildingPlan.Building.MaxHealth)
                    Unit.TaskQueue.OverrideWith(new RepairTask(Unit, buildingPlan.Building));
                MarkAsEnded();
                return;
            }

            if (!Unit.World.IsFree(buildingPlan.GridRegion, buildingPlan.BuildingType.CollisionLayer))
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

            if (buildingPlan.Building.HasSkill<ExtractAlageneSkill>())
            {
                ResourceNode node = Unit.World.Entities
                    .OfType<ResourceNode>()
                    .First(n => n.BoundingRectangle.ContainsPoint(buildingPlan.Location));
                node.Extractor = buildingPlan.Building;
            }

            Unit.TaskQueue.OverrideWith(new RepairTask(Unit, buildingPlan.Building));
            MarkAsEnded();
        }
        #endregion
    }
}
