using System;
using System.Linq;

using OpenTK.Math;

namespace Orion.GameLogic.Tasks
{
    [Serializable]
    public sealed class Harvest : Task
    {
        #region Fields
        private readonly Unit harvester;
        private readonly ResourceNode node;
        private const float secondsToHarvest = 5;
        private const float secondsToGiveRessource = 0;
        private const int defaultAmountToHarvest = 5;
        private int amountToHarvest = 0;
        private float secondsSpentHarvesting = 0;
        private float secondsGivingRessource = 0;
        private Move move;
        private Unit commandCenter;
        private bool extractingOrDelivering = true; //true = extracting, false = delivering
        private bool hasEnded = false;
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
            this.move = new Move(harvester, node.Position);
            this.commandCenter = FindClosestCommandCenter(node.Position);
            
        }
        #endregion

        #region Properties
        public override string Description
        {
            get { return "harvesting"; }
        }
        public override bool HasEnded
        {
            get { return hasEnded; }
        }
        #endregion

        #region Methods
        public override void Update(float timeDelta)
        {
            if (move.HasEnded)
            {
                if (extractingOrDelivering) //true = extracting, false = delivering
                {
                    if (harvestingIsOver(timeDelta))
                    {
                    
                        if (node.AmountRemaining != 0)
                        {
                            //determines the amount of resources to be harvested and substracts that amount to the node
                            if (node.AmountRemaining >= amountToHarvest)
                                amountToHarvest = defaultAmountToHarvest;
                            else
                                amountToHarvest = node.AmountRemaining;

                            node.Harvest(amountToHarvest);
                            if (commandCenter != null)
                                move = new Move(harvester, commandCenter.Position);
                            else
                            {
                                hasEnded = true;
                            }
                            extractingOrDelivering = false;

                            

                            //System.Console.Write("\nAlladium: " + harvester.Faction.AladdiumAmount + "\tAlagene: " + harvester.Faction.AlageneAmount);
                            secondsSpentHarvesting = 0;
                        }
                    }
                }
                else
                {
                    if (visitingCommandCenterIsOver(timeDelta))
                    {
                        move = new Move(harvester, node.Position);
                        extractingOrDelivering = true;
                        //adds the resources to the unit's faction
                        if (node.Type == ResourceType.Aladdium)
                            harvester.Faction.AladdiumAmount += amountToHarvest;
                        else if (node.Type == ResourceType.Alagene)
                            harvester.Faction.AlageneAmount += amountToHarvest;
                    }
                }
            }
            else
            {
                move.Update(timeDelta);
            }
        }

        private Unit FindClosestCommandCenter(Vector2 nodePosition)
        {
            Unit closestCommandCenter = null;
            float shortestDistance = -1;
            foreach (Unit unit in harvester.Faction.World.Entities.OfType<Unit>())
            {
                if (unit.Faction == harvester.Faction && unit.HasSkill<Skills.StoreResources>())
                {
                    float distance = (unit.Position - nodePosition).LengthSquared;
                    if (distance < shortestDistance || shortestDistance == -1)
                    {
                        shortestDistance = distance;
                        closestCommandCenter = unit;
                    }
                }
            }

            if (closestCommandCenter == null)
            {
                return null;
            }
            else
                closestCommandCenter.Died += new GenericEventHandler<Entity>(CommandCenterDestroyed);

            return closestCommandCenter;
        }

        void CommandCenterDestroyed(Entity sender)
        {
            commandCenter = FindClosestCommandCenter(node.Position);
        }

        private bool harvestingIsOver(float timeDelta)
        {
            if (secondsSpentHarvesting >= secondsToHarvest)
            {
                return true;
            }
            else
            {
                secondsSpentHarvesting += timeDelta;
                return false;
            }
        }

        private bool visitingCommandCenterIsOver(float timeDelta)
        {
            if (secondsGivingRessource >= secondsToGiveRessource)
            {
                return true;
            }
            else
            {
                secondsGivingRessource += timeDelta;
                return false;
            }
        }

        #endregion
    }
}