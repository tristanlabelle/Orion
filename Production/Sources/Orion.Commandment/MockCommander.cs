using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Commandment;
using Orion.Commandment.Commands;
using Orion.GameLogic;

namespace Orion.Commandment
{
    class MockCommander : Commander
    {

        #region Constructors

        public MockCommander(Faction sourceFaction)
            : base(sourceFaction)
        {
        }

        #endregion

        #region Methods

        public override IEnumerable<Command> CreateCommands()
        {
            return new Command[] { new MockCommand(base.Faction) };
        }

        #endregion
    }
}
