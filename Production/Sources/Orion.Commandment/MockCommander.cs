using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Orion.Commandment;
using Orion.Commandment.Commands;
using Orion.GameLogic;
using OpenTK.Math;


namespace Orion.Commandment
{
    class MockCommander : Commander
    {

        #region Constructors

        public MockCommander(Faction faction, World world)
            : base(faction, world)
        {
        }

        #endregion

        #region Methods

        public override IEnumerable<Command> CreateCommands()
        {
            ICollection<Unit> allUnits = World.Units;
            IEnumerable<Unit> myUnits = allUnits.Where(unit => unit.Faction == Faction);

            return new Command[] { new Move(Faction, myUnits, new Vector2(0f,0f)) };
        }
        #endregion
    }
}
