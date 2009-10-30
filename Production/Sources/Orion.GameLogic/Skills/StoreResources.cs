using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.GameLogic.Skills
{
    /// <summary>
    /// A <see cref="Skill"/> which permits a <see cref="UnitType"/>
    /// to receive resources that were harvested to store them.
    /// </summary>
    [Serializable]
    public sealed class StoreResources : Skill {}
}
