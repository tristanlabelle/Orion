using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Game.Matchmaking.Commands;
using Orion.Game.Matchmaking.Commands.Pipeline;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Skills;

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
