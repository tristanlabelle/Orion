﻿using System;
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

        private readonly Unit harvester;
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
        {
            Argument.EnsureNotNull(harvester, "harvester");
            if (!harvester.HasSkill<Skills.Harvest>())
                throw new ArgumentException("Cannot harvest without the harvest skill.", "harvester");
            Argument.EnsureNotNull(node, "node");

            this.harvester = harvester;
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
        public override void Update(float timeDelta)
        {
            if (!node.IsHarvestableByFaction(harvester.Faction))
            {
                hasEnded = true;
                return;
            }

            if (move.HasEnded)
            {
                if (mode == Mode.Extracting)
                {
                    UpdateExtracting(timeDelta);
                }
                else
                {
                    if (VisitingCommandCenterIsOver(timeDelta))
                    {
                        //adds the resources to the unit's faction
                        if (node.Type == ResourceType.Aladdium)
                            harvester.Faction.AladdiumAmount += amountCarrying;
                        else if (node.Type == ResourceType.Alagene)
                            harvester.Faction.AlageneAmount += amountCarrying;
                        amountCarrying = 0;

                        if (nodeIsDead)
                        {
                            hasEnded = true;
                            return;
                        }
                        move = new Move(harvester, node.Position);
                        mode = Mode.Extracting;
                    }
                }
            }
            else
            {
                move.Update(timeDelta);
            }
        }

        private void UpdateExtracting(float timeDelta)
        {
            float extractingSpeed = harvester.GetStat(UnitStat.ExtractingSpeed);
            amountAccumulator += extractingSpeed * timeDelta;
            
            int maxCarryingAmount = harvester.GetSkill<Skills.Harvest>().MaxCarryingAmount;
            while (amountAccumulator >= 1)
            {
                if (nodeIsDead)
                {
                    commandCenter.Died += commandCenterDestroyedEventHandler;
                    move = new Move(harvester, commandCenter.Position);
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
                        move = new Move(harvester, commandCenter.Position);
                        mode = Mode.Delivering;
                    }
                    return;
                }
            }
        }

        private Unit FindClosestCommandCenter()
        {
            return harvester.World.Entities
                .OfType<Unit>()
                .Where(unit => unit.Faction == harvester.Faction && unit.HasSkill<Skills.StoreResources>())
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
                    move = new Move(harvester, commandCenter.Position);
                }
            }
        }

        private bool VisitingCommandCenterIsOver(float timeDelta)
        {
            if (secondsGivingResource >= depositingDuration)
            {
                return true;
            }
            else
            {
                secondsGivingResource += timeDelta;
                return false;
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