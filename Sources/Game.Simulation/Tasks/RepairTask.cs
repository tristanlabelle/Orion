using System;
using System.Linq;
using System.Diagnostics;
using Orion.Engine;

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
        private readonly Action<Entity> targetDiedEventHandler;
        private readonly MoveTask move;
        private readonly bool building;
        private bool hasEnded;
        
        /// <summary>
        /// Remaining amount of aladdium that has been taken from the <see cref="Faction"/>'s coffers
        /// and is to be used to repair.
        /// </summary>
        private float aladdiumCreditRemaining;
        private float alageneCreditRemaining;
        #endregion

        #region Constructors
        public RepairTask(Unit repairer, Unit target)
            : base(repairer)
        {
            Argument.EnsureNotNull(repairer, "unit");
            Argument.EnsureNotNull(target, "target");
            if (!repairer.HasSkill(UnitSkill.Build))
                throw new ArgumentException("Cannot repair without the repair skill.", "unit");
            if (target == repairer)
                throw new ArgumentException("A unit cannot repair itself.");

            if (!target.Type.IsBuilding)
                throw new ArgumentException("Can only repair buildings.", "target");
            if (repairer.Faction != target.Faction)
                throw new ArgumentException("Cannot repair other faction buildings.", "target");

            this.target = target;
            this.targetDiedEventHandler = OnBuildingDied;
            this.target.Died += targetDiedEventHandler;
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

        public override bool HasEnded
        {
            get
            {
                return hasEnded
                    || !target.IsAlive
                    || (move.HasEnded && !move.HasReachedDestination)
                    || (!building && target.Health >= target.MaxHealth);
            }
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

            Unit.LookAt(target.Center);
            if (building) UpdateBuild(step);
            else UpdateRepair(step);
        }

        private void UpdateBuild(SimulationStep step)
        {
            if (target.IsUnderConstruction)
            {
                target.Build(Unit.GetStat(UnitStat.BuildSpeed) * step.TimeDeltaInSeconds);
            }

            if (!target.IsUnderConstruction)
            {
                // If we just built an alagene extractor, start harvesting.
                if (Unit.HasSkill(UnitSkill.Harvest) && target.HasSkill(UnitSkill.ExtractAlagene))
                {
                    // Smells like a hack!
                    ResourceNode node = Unit.World.Entities.OfType<ResourceNode>()
                        .First(n => Region.Intersects(n.GridRegion, target.GridRegion));
                    Unit.TaskQueue.OverrideWith(new HarvestTask(Unit, node));
                }

                hasEnded = true;
            }
        }

        private void UpdateRepair(SimulationStep step)
        {
            if (!TryGetCredit()) return;

            int aladdiumCost = Target.GetStat(UnitStat.AladdiumCost);
            int alageneCost = Target.GetStat(UnitStat.AlageneCost);

            float healthToRepair = Unit.GetStat(UnitStat.BuildSpeed) * repairSpeedRatio * step.TimeDeltaInSeconds;
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
        }

        private bool TryGetCredit()
        {
            int aladdiumCost = Target.GetStat(UnitStat.AladdiumCost);
            int alageneCost = Target.GetStat(UnitStat.AlageneCost);

            bool needsAladdiumCredit = aladdiumCost > 0 && aladdiumCreditRemaining <= 0;
            bool needsAlageneCredit = alageneCost > 0 && alageneCreditRemaining <= 0;
            if (!needsAladdiumCredit && !needsAlageneCredit) return true;

            if ((needsAladdiumCredit && Faction.AladdiumAmount == 0)
                || (needsAlageneCredit && Faction.AlageneAmount == 0))
            {
                string warning = "Pas assez de ressources pour réparer le bâtiment {0}".FormatInvariant(Target.Type.Name);
                Faction.RaiseWarning(warning);
                return false;
            }

            if (needsAladdiumCredit)
            {
                --Faction.AladdiumAmount;
                ++aladdiumCreditRemaining;
            }

            if (needsAlageneCredit)
            {
                --Faction.AlageneAmount;
                ++alageneCreditRemaining;
            }

            return true;
        }

        public override void Dispose()
        {
            target.Died -= targetDiedEventHandler;
        }

        private void OnBuildingDied(Entity entity)
        {
            Debug.Assert(entity == target);
            target.Died -= targetDiedEventHandler;
        }
        #endregion
    }
}