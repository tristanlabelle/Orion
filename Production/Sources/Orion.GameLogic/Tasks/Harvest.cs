using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        private const int defaultAmountToHarvest = 5;
        private int amountToHarvest = 0;
        private float secondsSpentHarvesting = 0;
        private Move move;
        #endregion

        #region Constructors
        public Harvest(Unit harvester, ResourceNode node)
        {
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
                    if(node.ResourcesLeft != 0)
                    {
                        //determines the amount of resources to be harvested and substracts that amount to the node
                        if (node.ResourcesLeft >= amountToHarvest)
                            amountToHarvest = defaultAmountToHarvest;
                        else
                            amountToHarvest = node.ResourcesLeft;

                        node.Harvest(amountToHarvest);

                        //adds the resources to the unit's faction
                        if (node.ResourceType == ResourceType.Alladium)
                            harvester.Faction.AladdiumAmount += amountToHarvest;
                        else if (node.ResourceType == ResourceType.Allagene)
                            harvester.Faction.AllageneAmount += amountToHarvest;

                        //System.Console.Write("\nAlladium: " + harvester.Faction.AladdiumAmount + "\tAllagene: " + harvester.Faction.AllageneAmount);
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