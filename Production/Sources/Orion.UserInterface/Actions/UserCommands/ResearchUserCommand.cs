using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Math;
using Orion.Commandment;
using Orion.GameLogic;
using Orion.GameLogic.Technologies;
using Keys = System.Windows.Forms.Keys;
using Skills = Orion.GameLogic.Skills;

namespace Orion.UserInterface.Actions.UserCommands
{
    public sealed class ResearchUserCommand : ImmediateUserCommand
    {
        #region Fields
        private readonly UserInputManager inputManager;
        private readonly Technology technology;
        #endregion

        #region Constructors
        public ResearchUserCommand(UserInputManager inputManager, Technology technology)
        {
            Argument.EnsureNotNull(inputManager, "inputManager");
            Argument.EnsureNotNull(technology, "technology");

            this.inputManager = inputManager;
            this.technology = technology;
        }
        #endregion

        #region Methods
        public override void Execute()
        {
            return;
            //inputManager.LaunchResearch(technology);
        }
        #endregion
    }
}
