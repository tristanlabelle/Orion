using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Commandment;
using Orion.Commandment.Commands;
using Orion.GameLogic;
using OpenTK.Math;

namespace Orion.Commandment
{
    class MockCommander : Commander
    {

        #region Constructors

        public MockCommander(Faction sourceFaction, World world)
            : base(sourceFaction, world)
        {
        }

        #endregion

        #region Methods

        public override IEnumerable<Command> CreateCommands()
        {
            return new Command[] { new MockCommand(base.Faction), new Move(base.Faction,base.World.Units, new Vector2(0f,0f)) };
        }
        #endregion
    }
}
