using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Orion.GameLogic;
using HarvestTask = Orion.GameLogic.Tasks.Harvest;

namespace Orion.Commandment.Commands
{
    public sealed class Harvest : Command
    {
        #region Fields
        private readonly List<Unit> harvesters;
        private readonly ResourceNode node;
        #endregion

        #region Contructors
        public Harvest(Faction faction, IEnumerable<Unit> harvesters, ResourceNode node)
            :base(faction)
        {
            this.harvesters = harvesters.Distinct().ToList();
            this.node = node;
        }
        #endregion

        #region Methods
        public override void Execute()
        {
            foreach (Unit harvester in harvesters)
            {
                harvester.Task = new HarvestTask(harvester, node);
            }
        }
        #endregion
    }
}
