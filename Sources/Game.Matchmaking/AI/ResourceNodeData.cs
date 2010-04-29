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
        public readonly ResourceNode Node;
        public int HarvesterCount;
        public Unit NearbyDepot;
        public Unit Extractor;
        #endregion

        #region Constructors
        public ResourceNodeData(ResourceNode node)
        {
            Argument.EnsureNotNull(node, "node");
            this.Node = node;
        }
        #endregion
    }
}
