using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenTK.Math;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Game.Matchmaking.Commands;
using Orion.Game.Matchmaking.Commands.Pipeline;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Skills;

namespace Orion.Game.Matchmaking
{
    public class AICommander : Commander
    {
        #region Fields
        #endregion

        #region Contructors
        public AICommander(Match match, Faction faction)
            : base(match, faction)
        {}
        #endregion

        #region Properties
        #endregion

        #region Methods
        public override void Update(float timeDeltaInSeconds)
        {}
        #endregion
    }
}
