using System;

namespace Orion.Game.Simulation.Skills
{
    /// <summary>
    /// A <see cref="UnitSkill"/> which permits a <see cref="Unit"/>
    /// to be built on an Alagene node and extract its gas.
    /// </summary>
    [Serializable]
    public sealed class ExtractAlageneSkill : UnitSkill
    {
        #region Methods
        protected override UnitSkill Clone()
        {
            return new ExtractAlageneSkill();
        }
        #endregion
    }
}
