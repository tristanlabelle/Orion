using System;
using System.Linq;
using System.Diagnostics;
using Orion.Engine;
using Orion.Game.Simulation.Skills;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Simulation.Tasks
{
    /// <summary>
    /// A <see cref="Task"/> which causes a <see cref="Unit"/> to repair a target to its full health
    /// or to complete its construction.
    /// </summary>
    [Serializable]
    public sealed class RepairTask : Task
    {
        #region Fields
        private const float repairSpeedRatio = 0.25f;

        private readonly Unit target;
        private readonly MoveTask move;
        private readonly bool building;
        
        /// <summary>
        /// Remaining amount of aladdium that has been taken from the <see cref="Faction"/>'s coffers
        /// and is to be used to repair.
        /// </summary>
        private float aladdiumCreditRemaining;
        private float alageneCreditRemaining;
        #endregion

        #region Constructors
        public RepairTask(Entity repairer, Unit target)
            : base(repairer)
        {
            Argument.EnsureNotNull(repairer, "unit");
            Argument.EnsureNotNull(target, "target");
            if (!repairer.Components.Has<Builder>())
                throw new ArgumentException("Cannot repair without the build skill.", "unit");
            if (target == repairer)
                throw new ArgumentException("A unit cannot repair itself.");
            if (!target.Type.IsBuilding)
                throw new ArgumentException("Can only repair buildings.", "target");

            this.target = target;
            this.move = MoveTask.ToNearRegion(repairer, target.GridRegion);
            this.building = target.IsUnderConstruction;
        }
        #endregion

        #region Properties
        public Unit Target
        {
            get { return target; }
        }

        public override string Description
        {
            get { return "repairing {0}".FormatInvariant(target); }
        }

        public override float Progress
        {
            get { return target.IsAliveInWorld ? target.Health / target.MaxHealth : float.NaN; }
        }
        #endregion

        #region Methods
        protected override void DoUpdate(SimulationStep step)
        {
            Spatial spatial = Entity.Spatial;
            Faction faction = FactionMembership.GetFaction(Entity);
            if (spatial == null
                || !Entity.Components.Has<Builder>()
                || faction == null
                || !target.IsAliveInWorld)
            {
                Debug.Assert(faction != null, "Repairing without a faction is unimplemented.");
                MarkAsEnded();
                return;
            }

            if (!move.HasEnded)
            {
                move.Update(step);
                if (move.HasEnded && !move.HasReachedDestination)
                    MarkAsEnded();
                return;
            }

            spatial.LookAt(target.Center);
            if (building) UpdateBuild(step);
            else UpdateRepair(step);
        }

        private void UpdateBuild(SimulationStep step)
        {
            if (target.IsUnderConstruction)
            {
                target.Build((float)Entity.GetStatValue(Builder.SpeedStat) * step.TimeDeltaInSeconds);
            }

            if (!target.IsUnderConstruction)
            {
                // If we just built an alagene extractor, start harvesting.
                if (Entity.Components.Has<Harvester>() && target.Components.Has<AlageneExtractor>())
                {
                    // Smells like a hack!
                    Entity node = World.Entities
                        .Intersecting(target.BoundingRectangle)
                        .Where(e => e.Components.Has<Harvestable>())
                        .Where(e => !e.Components.Get<Harvestable>().IsEmpty)
                        .First(n => Region.Intersects(n.GridRegion, target.GridRegion));

                    if (TaskQueue.Count == 1)
                        TaskQueue.OverrideWith(new HarvestTask(Entity, node));
                }

                MarkAsEnded();
            }
        }

        private void UpdateRepair(SimulationStep step)
        {
            if (!TryGetCredit()) return;

            int aladdiumCost = (int)Target.GetStatValue(Identity.AladdiumCostStat, BasicSkill.AladdiumCostStat);
            int alageneCost = (int)Target.GetStatValue(Identity.AlageneCostStat, BasicSkill.AlageneCostStat);

            float healthToRepair = (float)Entity.GetStatValue(Builder.SpeedStat)
                * repairSpeedRatio * step.TimeDeltaInSeconds;
            if (healthToRepair > target.Damage) healthToRepair = target.Damage;

            float frameAladdiumCost = healthToRepair / Target.MaxHealth * aladdiumCost;
            float frameAlageneCost = healthToRepair / Target.MaxHealth * alageneCost;

            if (frameAladdiumCost > aladdiumCreditRemaining)
            {
                frameAladdiumCost = aladdiumCreditRemaining;
                healthToRepair = aladdiumCreditRemaining / aladdiumCost * Target.MaxHealth;
            }

            if (frameAlageneCost > alageneCreditRemaining)
            {
                frameAlageneCost = alageneCreditRemaining;
                healthToRepair = alageneCreditRemaining / alageneCost * Target.MaxHealth;
            }

            target.Health += healthToRepair;
            aladdiumCreditRemaining -= frameAladdiumCost;
            alageneCreditRemaining -= frameAlageneCost;

            if (target.Damage < 0.001f) MarkAsEnded();
        }

        private bool TryGetCredit()
        {
            int aladdiumCost = (int)Target.GetStatValue(Identity.AladdiumCostStat, BasicSkill.AladdiumCostStat);
            int alageneCost = (int)Target.GetStatValue(Identity.AlageneCostStat, BasicSkill.AlageneCostStat);

            bool needsAladdiumCredit = aladdiumCost > 0 && aladdiumCreditRemaining <= 0;
            bool needsAlageneCredit = alageneCost > 0 && alageneCreditRemaining <= 0;
            if (!needsAladdiumCredit && !needsAlageneCredit) return true;

            Faction faction = FactionMembership.GetFaction(Entity);

            if ((needsAladdiumCredit && faction.AladdiumAmount == 0)
                || (needsAlageneCredit && faction.AlageneAmount == 0))
            {
                string warning = "Pas assez de ressources pour réparer le bâtiment {0}".FormatInvariant(Target.Type.Name);
                faction.RaiseWarning(warning);
                return false;
            }

            if (needsAladdiumCredit)
            {
                --faction.AladdiumAmount;
                ++aladdiumCreditRemaining;
            }

            if (needsAlageneCredit)
            {
                --faction.AlageneAmount;
                ++alageneCreditRemaining;
            }

            return true;
        }
        #endregion
    }
}