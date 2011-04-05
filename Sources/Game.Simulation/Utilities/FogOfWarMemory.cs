using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Orion.Engine;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Simulation.Utilities
{
    /// <summary>
    /// Provides a memory of the game objects that have been seen in the fog of war.
    /// </summary>
    public sealed class FogOfWarMemory
    {
        #region Fields
        private readonly Faction faction;
        private readonly HashSet<RememberedEntity> entities = new HashSet<RememberedEntity>();
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
        /// Accesses the <see cref="Faction"/> for which we're remembering <see cref="Entity">entities</see>.
        /// </summary>
        public Faction Faction
        {
            get { return faction; }
        }

        /// <summary>
        /// Accesses the <see cref="Entity">entities</see> remembered by the faction.
        /// </summary>
        public IEnumerable<RememberedEntity> Entities
        {
            get { return entities; }
        }
        #endregion

        #region Methods
        private void OnVisibilityChanged(Faction sender, Region region)
        {
            Debug.Assert(sender == faction);

            foreach (Entity entity in faction.World.Entities.Intersecting(region.ToRectangle()))
            {
                Faction entityFaction = FactionMembership.GetFaction(entity);
                Entity prototype = Identity.GetPrototype(entity);
                if (entityFaction == faction
                    || prototype == null
                    || (!entity.Identity.IsBuilding && !entity.Components.Has<Harvestable>())
                    || faction.CanSee(entity))
                {
                    continue;
                }

                entities.Add(new RememberedEntity(entity.Spatial.GridRegion.Min, prototype, entityFaction));
            }

            hasVisibilityChanged = true;
        }

        /// <summary>
        /// Clears the memory of entities which are visible and are not what we thought they were.
        /// </summary>
        private void RemoveDeprecatedEntities()
        {
            entities.RemoveWhere(rememberedEntity =>
            {
                if (!faction.CanSee(rememberedEntity.GridRegion)) return false;

                CollisionLayer collisionLayer = rememberedEntity.Prototype.Spatial.CollisionLayer;
                Entity entity = faction.World.Entities.GetEntityAt(rememberedEntity.Location, collisionLayer);
                return entity == null || !rememberedEntity.Matches(entity);
            });
        }

        private void OnWorldUpdated(World world, SimulationStep step)
        {
            if (!hasVisibilityChanged) return;

            RemoveDeprecatedEntities();
            hasVisibilityChanged = false;
        }
        #endregion
    }
}
