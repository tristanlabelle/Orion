using System;
using System.Linq;

namespace Orion.GameLogic.Tasks
{
    [Serializable]
    public sealed class Repair : Task
    {
        #region Fields
        private readonly Unit target;
        private readonly GenericEventHandler<Entity> buildingDiedEventHandler;
        private readonly Move move;
        private float aladdiumCost;
        private float alageneCost;
        private float totalAladdiumCost;
        private float totalAlageneCost;
        private bool hasEnded;
        #endregion

        #region Constructors
        public Repair(Unit repairer, Unit target)
            : base(repairer)
        {
            Argument.EnsureNotNull(repairer, "unit");
            Argument.EnsureNotNull(target, "target");
            if (!repairer.HasSkill<Skills.Build>())
                throw new ArgumentException("Cannot repair without the repair skill.", "unit");
            if (target == repairer)
                throw new ArgumentException("A unit cannot repair itself.");

            // TODO: check against repairability itself instead of the Building type,
            // since otherwise mechanical units could be repaired too
            if (!target.Type.IsBuilding)
                throw new ArgumentException("Can only repair buildings.", "target");
            if (repairer.Faction != target.Faction)
                throw new ArgumentException("Cannot repair other faction buildings.", "target");

            this.target = target;
            this.buildingDiedEventHandler = OnBuildingDied;
            this.target.Died += buildingDiedEventHandler;
            this.move = Move.ToNearRegion(repairer, target.GridRegion);
            this.aladdiumCost = target.GetStat(UnitStat.AladdiumCost) / target.MaxHealth;
            this.alageneCost = target.GetStat(UnitStat.AlageneCost) / target.MaxHealth;
        }
        #endregion

        #region Properties
        public Unit Building
        {
            get { return target; }
        }

        public override string Description
        {
            get { return "repairing"; }
        }

        public override bool HasEnded
        {
            get { return hasEnded; }
        }
        #endregion

        #region Methods
        protected override void DoUpdate(float timeDelta)
        {
            if (!move.HasEnded)
            {
                move.Update(timeDelta);
                return;
            }

            if (target.Health >= target.MaxHealth)
            {
                hasEnded = true;
                target.Died -= buildingDiedEventHandler;
                return;
            }

            if (target.IsUnderConstruction)
            {
                target.Build(Unit.GetStat(UnitStat.BuildingSpeed) * timeDelta);

                if (!target.IsUnderConstruction)
                {
                    hasEnded = true;
                    target.Died -= buildingDiedEventHandler;
                    
                    // If we just built an alagene extractor, start harvesting.
                    if (target.HasSkill<Skills.ExtractAlagene>())
                    {
                        // Smells like a hack!
                        ResourceNode node = Unit.World.Entities.OfType<ResourceNode>()
                            .First(n => n.Position == target.Position);
                        Unit.CurrentTask = new Harvest(Unit, node);
                    }

                    return;
                } 
            }
            else if (Unit.Faction.AladdiumAmount >= aladdiumCost && Unit.Faction.AlageneAmount >= alageneCost)
            {
                target.Damage--;
                totalAladdiumCost += aladdiumCost;
                totalAlageneCost += alageneCost;

                if (totalAladdiumCost > 1)
                {
                    totalAladdiumCost--;
                    Unit.Faction.AladdiumAmount--;
                }

                if (totalAlageneCost > 1)
                {
                    totalAlageneCost--;
                    Unit.Faction.AlageneAmount--;
                }
            }
        }

        private void OnBuildingDied(Entity entity)
        {
            hasEnded = true;
            entity.Died -= buildingDiedEventHandler;
        }
        #endregion
    }
}