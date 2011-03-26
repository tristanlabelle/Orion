﻿using System;
using System.Diagnostics;
using System.Linq;
using Orion.Engine;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Simulation.Tasks
{
    /// <summary>
    /// A <see cref="Task"/> which causes a <see cref="Entity"/> to repair a target to its full health
    /// or to complete its construction.
    /// </summary>
    [Serializable]
    public sealed class RepairTask : Task
    {
        #region Fields
        private const float repairSpeedRatio = 0.25f;

        private readonly Entity target;
        private readonly MoveTask move;
        
        /// <summary>
        /// Remaining amount of aladdium that has been taken from the <see cref="Faction"/>'s coffers
        /// and is to be used to repair.
        /// </summary>
        private float aladdiumCreditRemaining;
        private float alageneCreditRemaining;
        #endregion

        #region Constructors
        public RepairTask(Entity entity, Entity target)
            : base(entity)
        {
            Argument.EnsureNotNull(entity, "entity");
            Argument.EnsureNotNull(target, "target");
            if (!entity.Components.Has<Builder>())
                throw new ArgumentException("Cannot repair without the build skill.", "entity");
            if (target == entity)
                throw new ArgumentException("An entity cannot repair itself.", "entity");

            Health targetHealth = target.Components.TryGet<Health>();
            if (targetHealth == null) throw new ArgumentException("Cannot heal an entity without a health component.", "target");
            if (targetHealth.Constitution != Constitution.Mechanical)
                throw new ArgumentException("Cannot repair a non-mechanical entity.", "target");

            this.target = target;
            this.move = MoveTask.ToNearRegion(entity, target.GridRegion);
        }
        #endregion

        #region Properties
        public Entity Target
        {
            get { return target; }
        }

        public override string Description
        {
            get { return "repairing {0}".FormatInvariant(target); }
        }

        public override float Progress
        {
            get
            {
                Health targetHealth = target.Components.TryGet<Health>();
                return targetHealth == null
                    ? float.NaN
                    : targetHealth.Value / (float)target.GetStatValue(Health.MaxValueStat);
            }
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

            BuildProgress buildProgress = target.Components.TryGet<BuildProgress>();
            if (buildProgress == null)
            {
                UpdateRepair(step);
            }
            else
            {
                float timeSpentInSeconds = (float)Entity.GetStatValue(Builder.SpeedStat) * step.TimeDeltaInSeconds;
                buildProgress.SpendTime(TimeSpan.FromSeconds(timeSpentInSeconds));
            }
        }

        private void UpdateRepair(SimulationStep step)
        {
            Spatial targetSpatial = target.Spatial;
            Health targetHealth = target.Components.TryGet<Health>();
            if (targetSpatial == null || targetHealth == null) return;

            if (!TryGetCredit()) return;

            int aladdiumCost = (int)target.GetStatValue(Identity.AladdiumCostStat);
            int alageneCost = (int)target.GetStatValue(Identity.AlageneCostStat);

            float healthToRepair = (float)Entity.GetStatValue(Builder.SpeedStat)
                * repairSpeedRatio * step.TimeDeltaInSeconds;
            if (healthToRepair > targetHealth.Damage) healthToRepair = targetHealth.Damage;

            float maxHealth = (float)target.GetStatValue(Health.MaxValueStat);
            float frameAladdiumCost = healthToRepair / maxHealth * aladdiumCost;
            float frameAlageneCost = healthToRepair / maxHealth * alageneCost;

            if (frameAladdiumCost > aladdiumCreditRemaining)
            {
                frameAladdiumCost = aladdiumCreditRemaining;
                healthToRepair = aladdiumCreditRemaining / aladdiumCost * maxHealth;
            }

            if (frameAlageneCost > alageneCreditRemaining)
            {
                frameAlageneCost = alageneCreditRemaining;
                healthToRepair = alageneCreditRemaining / alageneCost * maxHealth;
            }

            targetHealth.Damage -= healthToRepair;
            aladdiumCreditRemaining -= frameAladdiumCost;
            alageneCreditRemaining -= frameAlageneCost;

            if (targetHealth.Damage >= 0.001f) return;

            // If we just built an alagene extractor, start harvesting.
            if (Entity.Components.Has<Harvester>() && target.Components.Has<AlageneExtractor>())
            {
                // Smells like a hack!
                Entity node = World.Entities
                    .Intersecting(targetSpatial.BoundingRectangle)
                    .Where(e => e.Components.Has<Harvestable>())
                    .Where(e => !e.Components.Get<Harvestable>().IsEmpty)
                    .First(n => Region.Intersects(n.GridRegion, targetSpatial.GridRegion));

                if (TaskQueue.Count == 1)
                    TaskQueue.OverrideWith(new HarvestTask(Entity, node));
            }

            MarkAsEnded();
        }

        private bool TryGetCredit()
        {
            int aladdiumCost = (int)Target.GetStatValue(Identity.AladdiumCostStat);
            int alageneCost = (int)Target.GetStatValue(Identity.AlageneCostStat);

            bool needsAladdiumCredit = aladdiumCost > 0 && aladdiumCreditRemaining <= 0;
            bool needsAlageneCredit = alageneCost > 0 && alageneCreditRemaining <= 0;
            if (!needsAladdiumCredit && !needsAlageneCredit) return true;

            Faction faction = FactionMembership.GetFaction(Entity);

            if ((needsAladdiumCredit && faction.AladdiumAmount == 0)
                || (needsAlageneCredit && faction.AlageneAmount == 0))
            {
                string warning = "Pas assez de ressources pour réparer le bâtiment {0}"
                    .FormatInvariant(Target.Identity.Name);
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