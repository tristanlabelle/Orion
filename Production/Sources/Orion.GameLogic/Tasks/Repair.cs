﻿using System;


namespace Orion.GameLogic.Tasks
{
    /// <summary>
    /// A <see cref="Task"/> which make attack an enemy <see cref="Unit"/>
    /// </summary>
    [Serializable]
    public sealed class Repair : Task
    {
        #region Fields
        private readonly Unit unit;
        private readonly Unit building;
        private Follow follow;
        private float aladdiumCost;
        private float alageneCost;
        private float totalAladdiumCost = 0;
        private float totalAlageneCost = 0;
        #endregion

        #region Constructors
        public Repair(Unit unit, Unit building)
        {
            Argument.EnsureNotNull(unit, "unit");
            /*if(!unit.HasSkill<Skills.Repair>())
                throw new ArgumentException("Cannot repair without the repair skill.", "unit");*/
            Argument.EnsureNotNull(building, "building");
            if(!building.Type.IsBuilding)
                throw new ArgumentException("Can only repair buildings.", "building");
            if (building.Damage < 1)
                throw new ArgumentException("Cannot repair undamaged buildings.", "building");
            if(unit.Faction != building.Faction)
                throw new ArgumentException("Cannot repair enemy buildings.", "building");
            this.unit = unit;
            this.building = building;
            this.follow = new Follow(unit, building, unit.GetStat(UnitStat.AttackRange));
            this.aladdiumCost = 1 / building.MaxHealth * building.Type.AladdiumCost;
            this.alageneCost = 1 / building.MaxHealth * building.Type.AlageneCost;
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
            get { return building.Damage < 1; }
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
                if (unit.Faction.AladdiumAmount >= aladdiumCost && unit.Faction.AlageneAmount >= alageneCost)
                {
                    building.Damage--;
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
        #endregion
    }
}