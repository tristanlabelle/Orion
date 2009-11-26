using System;


namespace Orion.GameLogic.Tasks
{
    [Serializable]
    public sealed class Repair : Task
    {
        #region Fields
        private readonly Unit unit;
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
        public Repair(Unit unit, Unit building)
        {
            Argument.EnsureNotNull(unit, "unit");
            Argument.EnsureNotNull(building, "building");
            if (!unit.HasSkill<Skills.Build>()) throw new ArgumentException("Cannot repair without the repair skill.", "unit");

            // TODO: check against repairability itself instead of the Building type, since otherwise mechanical units can be repaired too
            if (!building.Type.IsBuilding) throw new ArgumentException("Can only repair buildings.", "building");
            if (building.Damage < 1) throw new ArgumentException("Cannot repair undamaged buildings.", "building");
            if (unit.Faction != building.Faction) throw new ArgumentException("Cannot repair enemy buildings.", "building");

            this.unit = unit;
            this.building = building;
            this.buildingDiedEventHandler = OnBuildingDied;
            this.building.Died += buildingDiedEventHandler;
            this.follow = new Follow(unit, building);
            this.aladdiumCost = building.GetStat(UnitStat.AladdiumCost) / building.MaxHealth;
            this.alageneCost = building.GetStat(UnitStat.AlageneCost) / building.MaxHealth;
 
        }
        #endregion

        #region Properties
        public Unit Unit
        {
            get { return unit; }
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
        /// <summary>
        /// At each update it check if the unit is near enough to repair if not it reupdates the follow.update method.
        /// </summary>
        /// <param name="timeDelta"></param>
        public override void Update(float timeDelta)
        {
            if (HasEnded)
                return;

            if (follow.IsInRange)
            {
                if (building.Health >= building.MaxHealth)
                {
                    hasEnded = true;
                    building.Died -= buildingDiedEventHandler;
                    return;
                }
                if (unit.UnderConstruction)
                {
                    
                    building.Build(unit.GetStat(UnitStat.BuildingSpeed) * timeDelta);

                    if (!building.UnderConstruction)
                    {
                        hasEnded = true;
                        building.Died -= buildingDiedEventHandler;
                        return;
                    }
                        
                }
                else if (unit.Faction.AladdiumAmount >= aladdiumCost && unit.Faction.AlageneAmount >= alageneCost)
                {
                    building.Damage --;
                    totalAladdiumCost += aladdiumCost;
                    totalAlageneCost += alageneCost;
                    if (totalAladdiumCost > 1)
                    {
                        totalAladdiumCost--;
                        unit.Faction.AladdiumAmount--;
                    }
                    if (totalAlageneCost > 1)
                    {
                        totalAlageneCost--;
                        unit.Faction.AlageneAmount--;
                    }
                }
            }
            else
            {
                follow.Update(timeDelta);

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