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
        private readonly GenericEventHandler<Entity> commandCenterDestroyedEventHandler;
        private int amountCarrying;
        private float amountAccumulator;
        private float secondsGivingResource;
        private Move move;
        private Unit commandCenter;
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
            this.commandCenterDestroyedEventHandler = OnCommandCenterDestroyed;
            this.move = new Move(harvester, node.Position);
            node.Died += new GenericEventHandler<Entity>(nodeDied);
            commandCenter = FindClosestCommandCenter();
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
        protected override void DoUpdate(float timeDelta)
        {
            if (!node.IsHarvestableByFaction(Unit.Faction))
            {
                hasEnded = true;
                return;
            }

            if (!move.HasEnded)
            {
                move.Update(timeDelta);
                return;
            }


            if (mode == Mode.Extracting)
                UpdateExtracting(timeDelta);
            else
                UpdateDelivering(timeDelta);
        }

        private void UpdateExtracting(float timeDelta)
        {
            float extractingSpeed = Unit.GetStat(UnitStat.ExtractingSpeed);
            amountAccumulator += extractingSpeed * timeDelta;

            int maxCarryingAmount = Unit.GetSkill<Skills.Harvest>().MaxCarryingAmount;
            while (amountAccumulator >= 1)
            {
                if (nodeIsDead)
                {
                    commandCenter.Died += commandCenterDestroyedEventHandler;
                    move = new Move(Unit, commandCenter.Position);
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
                    if (commandCenter != null)
                        commandCenter.Died -= commandCenterDestroyedEventHandler;

                    commandCenter = FindClosestCommandCenter();
                    if (commandCenter == null)
                    {
                        hasEnded = true;
                    }
                    else
                    {
                        commandCenter.Died += commandCenterDestroyedEventHandler;
                        move = new Move(Unit, commandCenter.Position);
                        mode = Mode.Delivering;
                    }
                    return;
                }
            }
        }

        private void UpdateDelivering(float timeDelta)
        {
            secondsGivingResource += timeDelta;
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

            move = new Move(Unit, node.Position);
            mode = Mode.Extracting;
        }

        private Unit FindClosestCommandCenter()
        {
            return Unit.World.Entities
                .OfType<Unit>()
                .Where(other => other.Faction == Unit.Faction && other.HasSkill<Skills.StoreResources>())
                .WithMinOrDefault(unit => (unit.Position - node.Position).LengthSquared);
        }

        private void OnCommandCenterDestroyed(Entity sender)
        {
            Debug.Assert(sender == commandCenter);
            commandCenter.Died -= commandCenterDestroyedEventHandler;
            commandCenter = null;

            if (mode == Mode.Delivering)
            {
                commandCenter = FindClosestCommandCenter();
                if (commandCenter == null)
                {
                    hasEnded = true;
                    return;
                }
                else
                {
                    commandCenter.Died += commandCenterDestroyedEventHandler;
                    move = new Move(Unit, commandCenter.Position);
                }
            }
        }

        void nodeDied(Entity sender)
        {
            nodeIsDead = true;
        }
        #endregion
        #endregion
    }
}