using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Orion.GameLogic
{
    public sealed class BuildingPlan
    {
        #region Fields
        private readonly UnitType buildingType;
        private Unit createdUnit;
        private Point position;
        private bool constructionBegan;
        #endregion

        #region Constructor
        public BuildingPlan(UnitType buildingType, Point position)
        {
            Argument.EnsureNotNull(buildingType, "buildingType");
            this.buildingType = buildingType;
            this.position = position;
        }
        #endregion

        #region Proprieties
        public bool ConstructionBegan
        {
            get { return constructionBegan; }
        }
        public UnitType BuildingType
        {
            get { return buildingType; }
        }
        public Point Position
        {
            get { return position; }
            set { position = value; }
        }

        public Unit CreatedUnit
        {
            get { return createdUnit; }
        }

        #endregion

        #region Methods
        public void lauchCreationOfThisUnit(Unit unit)
        {
            Argument.EnsureNotNull(unit, "unit");
            createdUnit = unit;
            unit.Health = 1;
            constructionBegan = true;
        }
        #endregion
    }
}
