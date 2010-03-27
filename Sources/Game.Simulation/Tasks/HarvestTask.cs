using System;
using System.Diagnostics;
using System.Linq;
using OpenTK.Math;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Game.Simulation.Skills;

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

        private readonly ResourceNode node;
        private int amountCarrying;
        private float amountAccumulator;
        private float secondsGivingResource;
        private MoveTask move;
        private Unit depot;
        private Mode mode = Mode.Extracting;
        private bool depotAvailable = true;
        #endregion

        #region Constructors
        public HarvestTask(Unit harvester, ResourceNode node)
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
        public ResourceNode ResourceNode
        {
            get { return node; }
        }

        public override string Description
        {
            get { return "harvesting " + node.Type; }
        }

        public override bool HasEnded
        {
            get
            {
                return (move.HasEnded && !move.HasReachedDestination)
                    || !node.IsAlive
                    || !node.IsHarvestableByFaction(Unit.Faction)
                    || !depotAvailable;
            }
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

            if (mode == Mode.Extracting)
                UpdateExtracting(step);
            else
                UpdateDelivering(step);
        }

        private void UpdateExtracting(SimulationStep step)
        {
            Unit.LookAt(node.Center);

            float extractingSpeed = Unit.GetStat(HarvestSkill.SpeedStat);
            amountAccumulator += extractingSpeed * step.TimeDeltaInSeconds;

            int maxCarryingAmount = Unit.GetStat(HarvestSkill.MaxCarryingAmountStat);
            while (amountAccumulator >= 1)
            {
                if (!node.IsAlive)
                {
                    Faction.RaiseWarning("Mine d'{0} vidée!".FormatInvariant(node.Type));
                    move = MoveTask.ToNearRegion(Unit, depot.GridRegion);
                    mode = Mode.Delivering;
                    return;
                }

                if (node.AmountRemaining > 0)
                {
                    node.Harvest(1);
                    --amountAccumulator;
                    ++amountCarrying;
                }

                if (amountCarrying >= maxCarryingAmount)
                {
                    depot = FindClosestDepot();
                    if (depot == null)
                    {
                        depotAvailable = false;
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
            if (!depot.IsAlive)
            {
                depot = FindClosestDepot();
                if (depot == null) depotAvailable = false;
                else move = MoveTask.ToNearRegion(Unit, depot.GridRegion);
                return;
            }

            Unit.LookAt(depot.Center);

            secondsGivingResource += step.TimeDeltaInSeconds;
            if (secondsGivingResource < depositingDuration)
                return;
            
            //adds the resources to the unit's faction
            if (node.Type == ResourceType.Aladdium)
                Unit.Faction.AladdiumAmount += amountCarrying;
            else if (node.Type == ResourceType.Alagene)
                Unit.Faction.AlageneAmount += amountCarrying;
            amountCarrying = 0;

            if (!node.IsAlive) return;

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