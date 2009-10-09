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
        private readonly RessourceNode node;
        private const float secondsToHarvest = 5;
        private const int defaultAmountToHarvest = 5;
        private int amountToHarvest = 0;
        private float secondsSpentHarvesting = 0;
        private Move move;
        #endregion

        #region Constructors
        public Harvest(Unit harvester, RessourceNode node)
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
                    if(node.RessourcesLeft != 0)
                    {
                        //determines the amount of ressources to be harvested and substracts that amount to the node
                        if (node.RessourcesLeft >= amountToHarvest)
                            amountToHarvest = defaultAmountToHarvest;
                        else
                            amountToHarvest = node.RessourcesLeft;

                        node.Harvest(amountToHarvest);

                        //adds the ressources to the unit's faction
                        if (node.RessourceType == RessourceType.Alladium)
                            harvester.Faction.AladdiumAmount += amountToHarvest;
                        else if (node.RessourceType == RessourceType.Allagene)
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