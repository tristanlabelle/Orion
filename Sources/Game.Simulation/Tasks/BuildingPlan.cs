using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Orion.Engine;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Simulation.Tasks
{
    /// <summary>
    /// Provides information about a building to be built.
    /// </summary>
    public sealed class BuildingPlan
    {
        #region Fields
        private readonly Faction faction;
        private readonly Unit buildingType;
        private readonly Point location;
        private Entity building;
        #endregion

        #region Constructors
        public BuildingPlan(Faction faction, Unit buildingType, Point location)
        {
            Argument.EnsureNotNull(faction, "faction");
            Argument.EnsureNotNull(buildingType, "buildingType");

            this.faction = faction;
            this.buildingType = buildingType;
            this.location = location;
        }
        #endregion

        #region Proprieties
        public Unit BuildingType
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

        /// <summary>
        /// Gets the <see cref="Entity"/> that was built,
        /// or <c>null</c> if it hasn't been built yet.
        /// </summary>
        public Entity Building
        {
            get { return building; }
        }
        #endregion

        #region Methods
        public Entity CreateBuilding()
        {
            if (IsBuildingCreated)
                throw new InvalidOperationException("Cannot create more than one building from a plan.");

            building = faction.CreateUnit(buildingType, location);

            // Set the building to an "under construction" state
            building.Components.Remove<TaskQueue>();

            BuildProgress buildProgress = new BuildProgress(building);
            building.Components.Add(buildProgress);

            Health buildingHealth = building.Components.TryGet<Health>();
            if (buildingHealth != null) buildingHealth.SetValue(1);
            
            buildProgress.RequiredTime = TimeSpan.FromSeconds((float)building.GetStatValue(Identity.SpawnTimeStat));

            return building;
        }
        #endregion
    }
}
