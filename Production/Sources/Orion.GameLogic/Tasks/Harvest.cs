using System;
using System.Linq;

using OpenTK.Math;
using System.Diagnostics;

namespace Orion.GameLogic.Tasks
{
    [Serializable]
    public sealed class Harvest : Task
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
        private Move move;
        private Unit depot;
        private Mode mode = Mode.Extracting;
        private bool hasEnded;
        private bool nodeIsDead = false;
        #endregion

        #region Constructors
        public Harvest(Unit harvester, ResourceNode node)
            : base(harvester)
        {
            if (!harvester.HasSkill<Skills.Harvest>())
                throw new ArgumentException("Cannot harvest without the harvest skill.", "harvester");
            Argument.EnsureNotNull(node, "node");

            this.node = node;
            this.depotDestroyedEventHandler = OnDepotDestroyed;
            this.nodeDepletedEventHandler = OnNodeDepleted;
            this.move = Move.ToNearRegion(harvester, node.GridRegion);
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
        protected override void DoUpdate(UpdateInfo info)
        {
            if (!node.IsHarvestableByFaction(Unit.Faction))
            {
                hasEnded = true;
                return;
            }

            if (!move.HasEnded)
            {
                move.Update(info);
                return;
            }

            if (mode == Mode.Extracting)
                UpdateExtracting(info);
            else
                UpdateDelivering(info);
        }

        public override void Dispose()
        {
            if (node != null) node.Died -= nodeDepletedEventHandler;
            if (depot != null) depot.Died -= depotDestroyedEventHandler;
        }

        private void UpdateExtracting(UpdateInfo info)
        {
            Unit.LookAt(node.Center);

            float extractingSpeed = Unit.GetStat(UnitStat.ExtractingSpeed);
            amountAccumulator += extractingSpeed * info.TimeDeltaInSeconds;

            int maxCarryingAmount = Unit.GetSkill<Skills.Harvest>().MaxCarryingAmount;
            while (amountAccumulator >= 1)
            {
                if (nodeIsDead)
                {
                    depot.Died += depotDestroyedEventHandler;
                    move = Move.ToNearRegion(Unit, depot.GridRegion);
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
                        move = Move.ToNearRegion(Unit, depot.GridRegion);
                        mode = Mode.Delivering;
                    }
                    return;
                }
            }
        }

        private void UpdateDelivering(UpdateInfo info)
        {
            Unit.LookAt(depot.Center);

            secondsGivingResource += info.TimeDeltaInSeconds;
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

            move = Move.ToNearRegion(Unit, node.GridRegion);
            mode = Mode.Extracting;
        }

        private Unit FindClosestDepot()
        {
            return Faction.Units
                .Where(other => other.HasSkill<Skills.StoreResources>())
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
                    move = Move.ToNearRegion(Unit, depot.GridRegion);
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