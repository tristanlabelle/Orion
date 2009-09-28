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
    public sealed class MockCommander : Commander
    {
        #region Constructors
        public MockCommander(Faction faction)
            : base(faction)
        {
        }
        #endregion

        #region Methods
        public override void Update(float timeDelta)
        {
            Command command = new Move(Faction, World.Units.Where(unit => unit.Faction == Faction), new Vector2(0, 0));
            GenerateCommand(command);
        }
        #endregion
    }
}
