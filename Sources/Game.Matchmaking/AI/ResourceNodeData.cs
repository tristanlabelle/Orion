using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Game.Simulation;
using Orion.Engine;

namespace Orion.Game.Matchmaking.AI
{
    /// <summary>
    /// Associates metadata used by the <see cref="HarvestingAICommander"/>
    /// to a resource node.
    /// </summary>
    internal sealed class ResourceNodeData
    {
        #region Fields
        private readonly ResourceNode node;
        private int harvesterCount;
        private Unit nearbyDepot;
        #endregion

        #region Constructors
        public ResourceNodeData(ResourceNode node)
        {
            Argument.EnsureNotNull(node, "node");
            this.node = node;
        }
        #endregion

        #region Properties
        public ResourceNode Node
        {
            get { return node; }
        }

        public int HarvesterCount
        {
            get { return harvesterCount; }
            set
            {
                Argument.EnsurePositive(value, "HarvesterCount");
                harvesterCount = value;
            }
        }

        public Unit NearbyDepot
        {
            get { return nearbyDepot; }
            set { nearbyDepot = value; }
        }
        #endregion
    }
}
