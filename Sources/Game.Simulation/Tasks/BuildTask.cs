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
    /// A <see cref="Task"/> which makes a <see cref="Entity"/> build a building of a given type.
    /// </summary>
    [Serializable]
    public sealed class BuildTask : Task
    {
        #region Fields
        private readonly BuildingPlan plan;
        private MoveTask move;
        #endregion

        #region Constructors
        public BuildTask(Entity entity, BuildingPlan plan)
            : base(entity)
        {
            Argument.EnsureNotNull(entity, "entity");
            Argument.EnsureNotNull(plan, "plan");

            Builder builder = entity.Components.TryGet<Builder>();
            if (builder == null || !builder.Supports(plan.BuildingType))
            {
                throw new ArgumentException("{0} cannot build {1}."
                    .FormatInvariant(entity, plan.BuildingType));
            }

            this.plan = plan;
            this.move = MoveTask.ToNearRegion(entity, plan.GridRegion);
        }
        #endregion

        #region Properties
        public override string Description
        {
            get { return "Building {0}".FormatInvariant(plan.BuildingType); }
        }

        public BuildingPlan Plan
        {
            get { return plan; }
        }
        #endregion

        #region Methods
        protected override void DoUpdate(SimulationStep step)
        {
            Spatial spatial = Entity.Spatial;
            Builder builder = Entity.Components.TryGet<Builder>();
            Faction faction = FactionMembership.GetFaction(Entity);
            if (spatial == null || builder == null || faction == null)
            {
                Debug.Assert(faction != null, "Building without a faction is unimplemented.");
                MarkAsEnded();
                return;
            }

            if (!move.HasEnded)
            {
                move.Update(step);
                return;
            }

            // Test if we're in the building's surrounding area
            if (!Region.AreAdjacentOrIntersecting(plan.GridRegion, spatial.GridRegion))
            {
                MarkAsEnded();
                return;
            }

            if (plan.IsBuildingCreated)
            {
                Health buildingHealth = plan.Building.Components.TryGet<Health>();
                if (TaskQueue.Count == 1
                    && (plan.Building.Components.Has<BuildProgress>()
                    || (buildingHealth != null && buildingHealth.Damage > 0)))
                {
                    TaskQueue.OverrideWith(new RepairTask(Entity, plan.Building));
                }

                MarkAsEnded();
                return;
            }

            CollisionLayer layer = plan.BuildingType.Spatial.CollisionLayer;
            if (!World.IsFree(plan.GridRegion, layer))
            {
                string warning = "Pas de place pour construire le bâtiment {0}"
                    .FormatInvariant(plan.BuildingType.Identity.Name);
                faction.RaiseWarning(warning);
                MarkAsEnded();
                return;
            }

            int aladdiumCost = faction.GetStat(plan.BuildingType, BasicSkill.AladdiumCostStat);
            int alageneCost = faction.GetStat(plan.BuildingType, BasicSkill.AlageneCostStat);
            bool hasEnoughResources = faction.AladdiumAmount >= aladdiumCost
                && faction.AlageneAmount >= alageneCost;

            if (!hasEnoughResources)
            {
                string warning = "Pas assez de ressources pour construire le bâtiment {0}"
                    .FormatInvariant(plan.BuildingType.Identity.Name);
                faction.RaiseWarning(warning);
                MarkAsEnded();
                return;
            }

            faction.AladdiumAmount -= aladdiumCost;
            faction.AlageneAmount -= alageneCost;

            Entity building = plan.CreateBuilding();
            TaskQueue.ReplaceWith(new RepairTask(Entity, building));
            MarkAsEnded();
        }
        #endregion
    }
}
