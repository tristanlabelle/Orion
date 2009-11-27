using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Keys = System.Windows.Forms.Keys;
using Skills = Orion.GameLogic.Skills;
using Orion.GameLogic;
using Orion.Graphics;
using Orion.Geometry;
using Orion.Commandment;

namespace Orion.UserInterface.Actions.UserCommands
{
    public sealed class TrainUserCommand : ImmediateUserCommand
    {
        private UnitType type;
        private UserInputManager inputManager;

        public TrainUserCommand(UserInputManager manager, UnitType type)
        {
            this.type = type;
            inputManager = manager;
        }

        public override void Execute()
        {
            inputManager.LaunchTrain(type);
        }
    }
}
