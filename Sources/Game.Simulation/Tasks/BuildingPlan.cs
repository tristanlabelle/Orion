using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Orion.Engine;

namespace Orion.Game.Simulation.Tasks
{
    /// <summary>
    /// Provides information about a building to be built.
    /// </summary>
    public sealed class BuildingPlan
    {
        #region Fields
        private readonly Faction faction;
        private readonly UnitType buildingType;
        private readonly Point location;
        private Unit building;
        #endregion

        #region Constructors
        public BuildingPlan(Faction faction, UnitType buildingType, Point location)
        {
            Argument.EnsureNotNull(faction, "faction");
            Argument.EnsureNotNull(buildingType, "buildingType");

            this.faction = faction;
            this.buildingType = buildingType;
            this.location = location;
        }
        #endregion

        #region Proprieties
        public UnitType BuildingType
        {
            get { return buildingType; }
        }

        public Point Location
        {
            get { return location; }
        }

        public Region GridRegion
        {
            get { return new Region(Location, buildingType.Size); }
        }

        public bool IsBuildingCreated
        {
            get { return building != null; }
        }

        public Unit Building
        {
            get { return building; }
        }
        #endregion

        #region Methods
        public void CreateBuilding()
        {
            if (IsBuildingCreated)
                throw new InvalidOperationException("Cannot create more than one building from a plan.");

            building = faction.CreateUnit(buildingType, location);
        }
        #endregion
    }
}
