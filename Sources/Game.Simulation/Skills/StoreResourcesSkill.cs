using System;

namespace Orion.Game.Simulation.Skills
{
    /// <summary>
    /// A <see cref="UnitSkill"/> which permits a <see cref="UnitType"/>
    /// to receive resources that were harvested to store them.
    /// </summary>
    [Serializable]
    public sealed class StoreResourcesSkill : UnitSkill
    {
        protected override UnitSkill Clone()
        {
            return new StoreResourcesSkill();
        }
    }
}
