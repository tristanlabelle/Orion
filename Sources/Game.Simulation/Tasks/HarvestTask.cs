using System;
using System.Diagnostics;
using System.Linq;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Simulation.Tasks
{
    [Serializable]
    public sealed class HarvestTask : Task
    {
        #region Nested Types
        private enum Mode
        {
            Extracting,
            Delivering
        }
        #endregion

        #region Instance
        #region Fields
        private const float extractingDuration = 5;

        private readonly Entity resourceNode;
        private readonly ResourceType resourceType;
        private int amountCarrying;
        private float amountAccumulator;
        private MoveTask move;
        private Entity depot;
        private Mode mode = Mode.Extracting;
        #endregion

        #region Constructors
        public HarvestTask(Entity entity, Entity resourceNode)
            : base(entity)
        {
            if (!entity.Components.Has<Harvester>())
                throw new ArgumentException("Cannot harvest without the harvester component.", "entity");
            Argument.EnsureNotNull(resourceNode, "node");

            this.resourceNode = resourceNode;
            this.resourceType = resourceNode.Components.Get<Harvestable>().Type;
            this.move = MoveTask.ToNearRegion(entity, resourceNode.Spatial.GridRegion);
        }
        #endregion

        #region Properties
        public Entity ResourceNode
        {
            get { return resourceNode; }
        }

        /// <summary>
        /// Gets a value indicating if the <see cref="Entity"/> is currently extracting
        /// resources from the resource node.
        /// </summary>
        public bool IsExtracting
        {
            get { return move == null && mode == Mode.Extracting; }
        }

        public override string Description
        {
            get { return "harvesting " + resourceType; }
        }

        public override Type PublicType
        {
            get { return move == null && mode == Mode.Extracting ? typeof(Harvester) : typeof(MoveTask); }
        }

        private bool IsResourceNodeValid
        {
            get
            {
                Faction faction = FactionMembership.GetFaction(Entity);
                return resourceNode.IsAlive
                    && resourceNode.Spatial != null
                    && faction != null
                    && faction.CanHarvest(resourceNode);
            }
        }
        #endregion

        #region Methods
        protected override void DoUpdate(SimulationStep step)
        {
            Faction faction = FactionMembership.GetFaction(Entity);
            if (Entity.Spatial == null
                || !Entity.Components.Has<Harvester>()
                || faction == null)
            {
                Debug.Assert(faction != null, "Harvesting without a faction is unimplemented.");
                MarkAsEnded();
                return;
            }

            if (move != null)
                UpdateMove(step);
            else if (mode == Mode.Extracting)
                UpdateExtracting(step);
            else
                UpdateDelivering(step);
        }

        private void UpdateMove(SimulationStep step)
        {
            if (!move.HasEnded)
            {
                move.Update(step);
                return;
            }

            if (!move.HasReachedDestination)
            {
                // TODO: Resources are lost here
                MarkAsEnded();
                return;
            }

            // Setup so next update will extract or deposit
            move = null;
        }

        private void UpdateExtracting(SimulationStep step)
        {
            Faction faction = FactionMembership.GetFaction(Entity);
            if (!IsResourceNodeValid)
            {
                if (amountCarrying == 0)
                {
                    MarkAsEnded();
                    return;
                }

                TransitionToDelivering();
                return;
            }

            Entity.Spatial.LookAt(resourceNode.Spatial.Center);

            float extractingSpeed = (float)Entity.GetStatValue(Harvester.SpeedStat);
            amountAccumulator += extractingSpeed * step.TimeDeltaInSeconds;

            int maxCarryingAmount = (int)Entity.GetStatValue(Harvester.MaxCarryingAmountStat);
            while (amountAccumulator >= 1)
            {
                Harvestable harvestable = resourceNode.Components.Get<Harvestable>();
                if (!resourceNode.IsAlive)
                {
                    faction.RaiseWarning("Mine d'{0} vidée!".FormatInvariant(harvestable.Type));
                    TransitionToDelivering();
                    return;
                }

                if (!harvestable.IsEmpty)
                {
                    harvestable.Harvest(1);
                    --amountAccumulator;
                    ++amountCarrying;
                }

                if (amountCarrying >= maxCarryingAmount)
                {
                    TransitionToDelivering();
                    return;
                }
            }
        }

        private void TransitionToDelivering()
        {
            depot = FindNearestDepot();
            if (depot == null)
            {
                MarkAsEnded();
            }
            else
            {
                mode = Mode.Delivering;
                move = MoveTask.ToNearRegion(Entity, depot.Spatial.GridRegion);
            }
        }

        private void UpdateDelivering(SimulationStep step)
        {
            if (!IsValidResourceDepot(depot))
            {
                TransitionToDelivering();
                return;
            }

            Entity.Spatial.LookAt(depot.Spatial.Center);

            // Add resources to the entity's faction
            Faction faction = FactionMembership.GetFaction(Entity);
            faction.AddResources(resourceType, amountCarrying);
            amountCarrying = 0;

            if (!IsResourceNodeValid)
            {
                if (TaskQueue.Count == 1) TaskQueue.OverrideWith(new MoveTask(Entity, (Point)resourceNode.Center));
                MarkAsEnded();
                return;
            }

            // If the entity was enqueued other tasks, stop harvesting
            if (TaskQueue.Count > 1) MarkAsEnded();

            mode = Mode.Extracting;
            move = MoveTask.ToNearRegion(Entity, resourceNode.Spatial.GridRegion);
        }

        private static bool IsValidResourceDepot(Entity entity)
        {
#warning The resource depot component should not be present while the build is in progress
            return entity.Components.Has<ResourceDepot>()
                && entity.IsAlive
                && entity.Spatial != null
                && !entity.Components.Has<BuildProgress>();
        }

        private Entity FindNearestDepot()
        {
            float nearestDepotSquaredDistance = float.PositiveInfinity;
            Entity nearestDepot = null;

            foreach (Entity entity in FactionMembership.GetFaction(Entity).Entities)
            {
                if (!IsValidResourceDepot(entity)) continue;

                float squaredDistance = Region.SquaredDistance(entity.Spatial.GridRegion, Entity.Spatial.GridRegion);
                if (squaredDistance < nearestDepotSquaredDistance)
                {
                    nearestDepot = entity;
                    nearestDepotSquaredDistance = squaredDistance;
                }
            }

            return nearestDepot;
        }
        #endregion
        #endregion
    }
}