using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Orion.Engine;

namespace Orion.Game.Simulation.Utilities
{
    /// <summary>
    /// Provides a memory of the game objects that have been seen in the fog of war.
    /// </summary>
    public sealed class FogOfWarMemory
    {
        #region Fields
        private readonly Faction faction;
        private readonly HashSet<RememberedBuilding> buildings = new HashSet<RememberedBuilding>();
        private bool hasVisibilityChanged = false;
        #endregion

        #region Constructors
        public FogOfWarMemory(Faction faction)
        {
            Argument.EnsureNotNull(faction, "faction");

            this.faction = faction;
            this.faction.VisibilityChanged += OnVisibilityChanged;
            this.faction.World.Updated += OnWorldUpdated;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the faction for which we're remembering objects.
        /// </summary>
        public Faction Faction
        {
            get { return faction; }
        }

        /// <summary>
        /// Accesses the buildings remembered by the faction.
        /// </summary>
        public IEnumerable<RememberedBuilding> Buildings
        {
            get { return buildings; }
        }
        #endregion

        #region Methods
        private void OnVisibilityChanged(Faction sender, Region region)
        {
            Debug.Assert(sender == faction);

            foreach (Entity entity in faction.World.Entities.Intersecting(region.ToRectangle()))
            {
                Unit unit = entity as Unit;
                if (unit == null || unit.Faction == faction || !unit.IsBuilding)
                    continue;

                RememberedBuilding building = new RememberedBuilding(unit);
                buildings.Add(building);
            }

            hasVisibilityChanged = true;
        }

        /// <summary>
        /// Cleans the memory from buildings which are visible and are not what we thought they were.
        /// </summary>
        private void RemoveDeprecatedBuildings()
        {
            buildings.RemoveWhere(rememberedBuilding =>
            {
                if (!faction.CanSee(rememberedBuilding.GridRegion)) return false;

                Unit building = faction.World.Entities.GetEntityAt(rememberedBuilding.Location, CollisionLayer.Ground) as Unit;
                return building == null || !rememberedBuilding.Matches(building);
            });
        }

        private void OnWorldUpdated(World world, SimulationStep step)
        {
            if (!hasVisibilityChanged) return;

            RemoveDeprecatedBuildings();
            hasVisibilityChanged = false;
        }
        #endregion
    }
}
