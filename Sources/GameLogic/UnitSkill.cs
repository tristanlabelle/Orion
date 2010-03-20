using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.GameLogic
{
    /// <summary>
    /// Identifies a capability of a unit.
    /// </summary>
    [Serializable]
    public enum UnitSkill
    {
        /// <summary>
        /// Allows units perform either melee or ranged attacks.
        /// </summary>
        Attack,

        /// <summary>
        /// Allows units to build buildings.
        /// </summary>
        Build,

        /// <summary>
        /// Allows buildings to extract alagene from resource nodes and provide it to harvesters.
        /// </summary>
        ExtractAlagene,

        /// <summary>
        /// Allows units to harvest resources.
        /// </summary>
        Harvest,

        /// <summary>
        /// Allows units to heal nearby allies.
        /// </summary>
        Heal,

        /// <summary>
        /// Allows units to move and pathfind.
        /// </summary>
        Move,

        /// <summary>
        /// Allows units to research technologies.
        /// </summary>
        Research,

        /// <summary>
        /// Allows buildings to increase the maximum available food amount.
        /// </summary>
        StoreFood,

        /// <summary>
        /// Allows buildings to serve as resource depots for harvesters.
        /// </summary>
        StoreResources,

        /// <summary>
        /// Allows units to explode when touching another kind of unit.
        /// </summary>
        SuicideBomb,

        /// <summary>
        /// Allows buildings to train new units.
        /// </summary>
        Train
    }
}
