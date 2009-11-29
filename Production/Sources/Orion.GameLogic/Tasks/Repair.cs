using System;
using System.Linq;

namespace Orion.GameLogic.Tasks
{
    [Serializable]
    public sealed class Repair : Task
    {
        #region Fields
        private readonly Unit repairer;
        private readonly Unit building;
        private readonly GenericEventHandler<Entity> buildingDiedEventHandler;
        private Follow follow;
        private float aladdiumCost;
        private float alageneCost;
        private float totalAladdiumCost;
        private float totalAlageneCost;
        private bool hasEnded;
        #endregion

        #region Constructors
        public Repair(Unit repairer, Unit building)
        {
            Argument.EnsureNotNull(repairer, "unit");
            Argument.EnsureNotNull(building, "building");
            if (!repairer.HasSkill<Skills.Build>())
                throw new ArgumentException("Cannot repair without the repair skill.", "unit");

            // TODO: check against repairability itself instead of the Building type, since otherwise mechanical units can be repaired too
            if (!building.Type.IsBuilding) throw new ArgumentException("Can only repair buildings.", "building");
            if (building.Health >= building.MaxHealth) throw new ArgumentException("Cannot repair undamaged buildings.", "building");
            if (repairer.Faction != building.Faction) throw new ArgumentException("Cannot repair enemy buildings.", "building");

            this.repairer = repairer;
            this.building = building;
            this.buildingDiedEventHandler = OnBuildingDied;
            this.building.Died += buildingDiedEventHandler;
            this.follow = new Follow(repairer, building);
            this.aladdiumCost = building.GetStat(UnitStat.AladdiumCost) / building.MaxHealth;
            this.alageneCost = building.GetStat(UnitStat.AlageneCost) / building.MaxHealth;
        }
        #endregion

        #region Properties
        public Unit Unit
        {
            get { return repairer; }
        }

        public Unit Building
        {
            get { return building; }
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
            if (!follow.IsInRange)
            {
                follow.Update(timeDelta);
                return;
            }

            if (building.Health >= building.MaxHealth)
            {
                hasEnded = true;
                building.Died -= buildingDiedEventHandler;
                return;
            }

            if (building.IsUnderConstruction)
            {
                building.Build(repairer.GetStat(UnitStat.BuildingSpeed) * timeDelta);

                if (!building.IsUnderConstruction)
                {
                    hasEnded = true;
                    building.Died -= buildingDiedEventHandler;
                    
                    // If we just built an alagene extractor, start harvesting.
                    if (building.HasSkill<Skills.ExtractAlagene>())
                    {
                        // Smells like a hack!
                        ResourceNode node = repairer.World.Entities.OfType<ResourceNode>()
                            .First(n => n.Position == building.Position);
                        repairer.CurrentTask = new Harvest(repairer, node);
                    }

                    return;
                } 
            }
            else if (repairer.Faction.AladdiumAmount >= aladdiumCost && repairer.Faction.AlageneAmount >= alageneCost)
            {
                building.Damage--;
                totalAladdiumCost += aladdiumCost;
                totalAlageneCost += alageneCost;

                if (totalAladdiumCost > 1)
                {
                    totalAladdiumCost--;
                    repairer.Faction.AladdiumAmount--;
                }

                if (totalAlageneCost > 1)
                {
                    totalAlageneCost--;
                    repairer.Faction.AlageneAmount--;
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