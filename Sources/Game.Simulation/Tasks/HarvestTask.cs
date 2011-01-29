using System;
using System.Diagnostics;
using System.Linq;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Game.Simulation.Skills;
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
        private Unit depot;
        private Mode mode = Mode.Extracting;
        #endregion

        #region Constructors
        public HarvestTask(Unit harvester, Entity node)
            : base(harvester)
        {
            if (!harvester.HasSkill<HarvestSkill>())
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
            get { return "harvesting " + node.GetComponent<Harvestable>().Type; }
        }
        #endregion

        #region Methods
        protected override void DoUpdate(SimulationStep step)
        {
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
            if (!node.IsAliveInWorld || !Unit.Faction.CanHarvest(node))
            {
                if (amountCarrying == 0)
                {
                    MarkAsEnded();
                    return;
                }

                mode = Mode.Delivering;
                return;
            }

            Unit.LookAt(node.Center);

            float extractingSpeed = Unit.GetStat(HarvestSkill.SpeedStat);
            amountAccumulator += extractingSpeed * step.TimeDeltaInSeconds;

            int maxCarryingAmount = Unit.GetStat(HarvestSkill.MaxCarryingAmountStat);
            while (amountAccumulator >= 1)
            {
                Harvestable harvest = node.GetComponent<Harvestable>();
                if (!node.IsAliveInWorld)
                {
                    Faction.RaiseWarning("Mine d'{0} vidée!".FormatInvariant(harvest.Type));
                    move = MoveTask.ToNearRegion(Unit, depot.GridRegion);
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
                        move = MoveTask.ToNearRegion(Unit, depot.GridRegion);
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

                move = MoveTask.ToNearRegion(Unit, depot.GridRegion);
                return;
            }

            Unit.LookAt(depot.Center);

            secondsGivingResource += step.TimeDeltaInSeconds;
            if (secondsGivingResource < depositingDuration)
                return;
            
            //adds the resources to the unit's faction
            Harvestable harvest = node.GetComponent<Harvestable>();
            if (harvest.Type == ResourceType.Aladdium)
                Unit.Faction.AladdiumAmount += amountCarrying;
            else if (harvest.Type == ResourceType.Alagene)
                Unit.Faction.AlageneAmount += amountCarrying;
            amountCarrying = 0;

            if (!node.IsAliveInWorld || !Unit.Faction.CanHarvest(node))
            {
                if (Unit.TaskQueue.Count == 1)
                    Unit.TaskQueue.OverrideWith(new MoveTask(Unit, (Point)node.Center));
                MarkAsEnded();
                return;
            }

            // if the unit was enqueued other tasks, stop harvesting
            if (Unit.TaskQueue.Count > 1) MarkAsEnded();

            move = MoveTask.ToNearRegion(Unit, node.GridRegion);
            mode = Mode.Extracting;
        }

        private Unit FindClosestDepot()
        {
            return Faction.Units
                .Where(other => !other.IsUnderConstruction && other.HasSkill<StoreResourcesSkill>())
                .WithMinOrDefault(storage => Region.SquaredDistance(storage.GridRegion, Unit.GridRegion));
        }
        #endregion
        #endregion
    }
}