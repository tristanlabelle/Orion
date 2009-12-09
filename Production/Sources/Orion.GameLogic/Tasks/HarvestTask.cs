﻿using System;
using System.Linq;

using OpenTK.Math;
using System.Diagnostics;

namespace Orion.GameLogic.Tasks
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
        private readonly GenericEventHandler<Entity> nodeDepletedEventHandler;
        private readonly GenericEventHandler<Entity> depotDestroyedEventHandler;
        private int amountCarrying;
        private float amountAccumulator;
        private float secondsGivingResource;
        private MoveTask move;
        private Unit depot;
        private Mode mode = Mode.Extracting;
        private bool hasEnded;
        private bool nodeIsDead = false;
        #endregion

        #region Constructors
        public HarvestTask(Unit harvester, ResourceNode node)
            : base(harvester)
        {
            if (!harvester.HasSkill<Skills.HarvestSkill>())
                throw new ArgumentException("Cannot harvest without the harvest skill.", "harvester");
            Argument.EnsureNotNull(node, "node");

            this.node = node;
            this.depotDestroyedEventHandler = OnDepotDestroyed;
            this.nodeDepletedEventHandler = OnNodeDepleted;
            this.move = MoveTask.ToNearRegion(harvester, node.GridRegion);
            node.Died += nodeDepletedEventHandler;
            depot = FindClosestDepot();
        }
        #endregion

        #region Properties
        public override string Description
        {
            get { return "harvesting " + node.Type; }
        }
        public override bool HasEnded
        {
            get { return hasEnded; }
        }
        #endregion

        #region Methods
        protected override void DoUpdate(SimulationStep step)
        {
            if (!node.IsHarvestableByFaction(Unit.Faction))
            {
                hasEnded = true;
                return;
            }

            if (!move.HasEnded)
            {
                move.Update(step);
                return;
            }

            if (!move.HasReachedDestination)
            {
                hasEnded = true;
                return;
            }

            if (mode == Mode.Extracting)
                UpdateExtracting(step);
            else
                UpdateDelivering(step);
        }

        public override void Dispose()
        {
            if (node != null) node.Died -= nodeDepletedEventHandler;
            if (depot != null) depot.Died -= depotDestroyedEventHandler;
        }

        private void UpdateExtracting(SimulationStep step)
        {
            Unit.LookAt(node.Center);

            float extractingSpeed = Unit.GetStat(UnitStat.ExtractingSpeed);
            amountAccumulator += extractingSpeed * step.TimeDeltaInSeconds;

            int maxCarryingAmount = Unit.GetSkill<Skills.HarvestSkill>().MaxCarryingAmount;
            while (amountAccumulator >= 1)
            {
                if (nodeIsDead)
                {
                    depot.Died += depotDestroyedEventHandler;
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
                    if (depot != null)
                        depot.Died -= depotDestroyedEventHandler;

                    depot = FindClosestDepot();
                    if (depot == null)
                    {
                        hasEnded = true;
                    }
                    else
                    {
                        depot.Died += depotDestroyedEventHandler;
                        move = MoveTask.ToNearRegion(Unit, depot.GridRegion);
                        mode = Mode.Delivering;
                    }
                    return;
                }
            }
        }

        private void UpdateDelivering(SimulationStep step)
        {
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

            if (nodeIsDead)
            {
                hasEnded = true;
                return;
            }

            move = MoveTask.ToNearRegion(Unit, node.GridRegion);
            mode = Mode.Extracting;
        }

        private Unit FindClosestDepot()
        {
            return Faction.Units
                .Where(other => other.HasSkill<Skills.StoreResourcesSkill>())
                .WithMinOrDefault(unit => Region.SquaredDistance(unit.GridRegion, node.GridRegion));
        }

        private void OnDepotDestroyed(Entity sender)
        {
            Debug.Assert(sender == depot);
            depot.Died -= depotDestroyedEventHandler;
            depot = null;

            if (mode == Mode.Delivering)
            {
                depot = FindClosestDepot();
                if (depot == null)
                {
                    hasEnded = true;
                    return;
                }
                else
                {
                    depot.Died += depotDestroyedEventHandler;
                    move = MoveTask.ToNearRegion(Unit, depot.GridRegion);
                }
            }
        }

        private void OnNodeDepleted(Entity sender)
        {
            Debug.Assert(sender == node);
            nodeIsDead = true;
            node.Died -= nodeDepletedEventHandler;
        }
        #endregion
        #endregion
    }
}