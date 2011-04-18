using Orion.Engine;
using Orion.Game.Simulation;

namespace Orion.Game.Matchmaking
{
    /// <summary>
    /// A commander which never issues any command.
    /// </summary>
    public sealed class NoopCommander : Commander
    {
        #region Contructors
        public NoopCommander(Match match, Faction faction)
            : base(match, faction)
        {}
        #endregion

        #region Methods
        public override void Update(SimulationStep step) { }
        #endregion
    }
}
