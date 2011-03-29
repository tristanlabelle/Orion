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
        private readonly Entity buildingPrototype;
        private readonly Point location;
        private Entity building;
        #endregion

        #region Constructors
        public BuildingPlan(Faction faction, Entity buildingPrototype, Point location)
        {
            Argument.EnsureNotNull(faction, "faction");
            Argument.EnsureNotNull(buildingPrototype, "buildingPrototype");

            this.faction = faction;
            this.buildingPrototype = buildingPrototype;
            this.location = location;
        }
        #endregion

        #region Proprieties
        public Entity BuildingPrototype
        {
            get { return buildingPrototype; }
        }

        public Point Location
        {
            get { return location; }
        }

        public Region GridRegion
        {
            get { return new Region(Location, buildingPrototype.Size); }
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

            building = faction.CreateUnit(buildingPrototype, location);

            // Set the building to an "under construction" state
            building.Components.Remove<TaskQueue>();

            BuildProgress buildProgress = new BuildProgress(building);
            building.Components.Add(buildProgress);

            Health buildingHealth = building.Components.TryGet<Health>();
            if (buildingHealth != null) buildingHealth.SetValue(1);
            
            buildProgress.RequiredTime = TimeSpan.FromSeconds((float)building.GetStatValue(Cost.SpawnTimeStat));

            return building;
        }
        #endregion
    }
}
