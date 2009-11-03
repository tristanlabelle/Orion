﻿using System;


namespace Orion.GameLogic.Tasks
{
    [Serializable]
    public sealed class Harvest : Task
    {
        #region Fields
        private readonly Unit harvester;
        private readonly ResourceNode node;
        private const float secondsToHarvest = 5;
        private const int defaultAmountToHarvest = 5;
        private int amountToHarvest = 0;
        private float secondsSpentHarvesting = 0;
        private Move move;
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
        }
        #endregion

        #region Properties
        public override string Description
        {
            get { return "harvesting"; }
        }
        #endregion

        #region Methods
        public override void Update(float timeDelta)
        {
            if (move.HasEnded)
            {
                if (harvestingIsOver(timeDelta))
                {
                    if(node.AmountRemaining != 0)
                    {
                        //determines the amount of resources to be harvested and substracts that amount to the node
                        if (node.AmountRemaining >= amountToHarvest)
                            amountToHarvest = defaultAmountToHarvest;
                        else
                            amountToHarvest = node.AmountRemaining;

                        node.Harvest(amountToHarvest);

                        //adds the resources to the unit's faction
                        if (node.Type == ResourceType.Aladdium)
                            harvester.Faction.AladdiumAmount += amountToHarvest;
                        else if (node.Type == ResourceType.Alagene)
                            harvester.Faction.AlageneAmount += amountToHarvest;

                        //System.Console.Write("\nAlladium: " + harvester.Faction.AladdiumAmount + "\tAlagene: " + harvester.Faction.AlageneAmount);
                        secondsSpentHarvesting = 0;
                    }
                }
            }
            else
            {
                move.Update(timeDelta);
            }
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
        #endregion
    }
}