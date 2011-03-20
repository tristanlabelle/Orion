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
        private const float harvestDuration = 5;
        private const float depositingDuration = 0;

        private readonly Entity node;
        private int amountCarrying;
        private float amountAccumulator;
        private float secondsGivingResource;
        private MoveTask move;
        private Entity depot;
        private Mode mode = Mode.Extracting;
        #endregion

        #region Constructors
        public HarvestTask(Entity harvester, Entity node)
            : base(harvester)
        {
            if (!harvester.Components.Has<Harvester>())
                throw new ArgumentException("Cannot harvest without the harvest skill.", "harvester");
            Argument.EnsureNotNull(node, "node");

            this.node = node;
            this.move = MoveTask.ToNearRegion(harvester, node.GridRegion);
        }
        #endregion

        #region Properties
        public Entity ResourceNode
        {
            get { return node; }
        }

        public override string Description
        {
            get { return "harvesting " + node.Components.Get<Harvestable>().Type; }
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

            if (!move.HasEnded)
            {
                move.Update(step);
                return;
            }

            if (!move.HasReachedDestination)
            {
                MarkAsEnded();
                return;
            }

            if (mode == Mode.Extracting)
                UpdateExtracting(step);
            else
                UpdateDelivering(step);
        }

        private void UpdateExtracting(SimulationStep step)
        {
            Faction faction = FactionMembership.GetFaction(Entity);
            if (!node.IsAliveInWorld || !faction.CanHarvest(node))
            {
                if (amountCarrying == 0)
                {
                    MarkAsEnded();
                    return;
                }

                mode = Mode.Delivering;
                return;
            }

            Entity.Spatial.LookAt(node.Center);

            float extractingSpeed = (float)Entity.GetStatValue(Harvester.SpeedStat);
            amountAccumulator += extractingSpeed * step.TimeDeltaInSeconds;

            int maxCarryingAmount = (int)Entity.GetStatValue(Harvester.MaxCarryingAmountStat);
            while (amountAccumulator >= 1)
            {
                Harvestable harvest = node.Components.Get<Harvestable>();
                if (!node.IsAliveInWorld)
                {
                    faction.RaiseWarning("Mine d'{0} vidée!".FormatInvariant(harvest.Type));
                    move = MoveTask.ToNearRegion(Entity, depot.GridRegion);
                    mode = Mode.Delivering;
                    return;
                }

                if (!harvest.IsEmpty)
                {
                    harvest.Harvest(1);
                    --amountAccumulator;
                    ++amountCarrying;
                }

                if (amountCarrying >= maxCarryingAmount)
                {
                    depot = FindClosestDepot();
                    if (depot == null)
                    {
                        MarkAsEnded();
                    }
                    else
                    {
                        move = MoveTask.ToNearRegion(Entity, depot.GridRegion);
                        mode = Mode.Delivering;
                    }

                    return;
                }
            }
        }

        private void UpdateDelivering(SimulationStep step)
        {
            if (depot == null || !depot.IsAliveInWorld)
            {
                depot = FindClosestDepot();
                if (depot == null)
                {
                    MarkAsEnded();
                    return;
                }

                move = MoveTask.ToNearRegion(Entity, depot.GridRegion);
                return;
            }

            Entity.Spatial.LookAt(depot.Center);

            secondsGivingResource += step.TimeDeltaInSeconds;
            if (secondsGivingResource < depositingDuration)
                return;

            //adds the resources to the unit's faction
            Faction faction = FactionMembership.GetFaction(Entity);
            Harvestable harvest = node.Components.Get<Harvestable>();
            if (harvest.Type == ResourceType.Aladdium)
                faction.AladdiumAmount += amountCarrying;
            else if (harvest.Type == ResourceType.Alagene)
                faction.AlageneAmount += amountCarrying;
            amountCarrying = 0;

            if (!node.IsAliveInWorld || !faction.CanHarvest(node))
            {
                if (TaskQueue.Count == 1)
                    TaskQueue.OverrideWith(new MoveTask(Entity, (Point)node.Center));
                MarkAsEnded();
                return;
            }

            // if the unit was enqueued other tasks, stop harvesting
            if (TaskQueue.Count > 1) MarkAsEnded();

            move = MoveTask.ToNearRegion(Entity, node.GridRegion);
            mode = Mode.Extracting;
        }

        private Entity FindClosestDepot()
        {
#warning The resource depot component should not be present while the build is in progress
            return FactionMembership.GetFaction(Entity).Entities
                .Where(other => !other.Components.Has<BuildProgress>() && other.Components.Has<ResourceDepot>())
                .WithMinOrDefault(storage => Region.SquaredDistance(storage.GridRegion, Entity.Spatial.GridRegion));
        }
        #endregion
        #endregion
    }
}