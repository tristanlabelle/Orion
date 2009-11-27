using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Keys = System.Windows.Forms.Keys;
using Skills = Orion.GameLogic.Skills;
using Orion.Commandment;
using Orion.GameLogic;
using OpenTK.Math;

namespace Orion.UserInterface.Actions.UserCommands
{
    public sealed class BuildUserCommand : UserInputCommand
    {
        #region Fields
        private readonly UserInputManager inputManager;
        private readonly UnitType buildingType;
        #endregion

        #region Constructors
        public BuildUserCommand(UserInputManager inputManager, UnitType buildingType)
        {
            Argument.EnsureNotNull(inputManager, "inputManager");
            Argument.EnsureNotNull(buildingType, "buildingType");

            this.inputManager = inputManager;
            this.buildingType = buildingType;
        }
        #endregion

        #region Methods
        public override void Execute(Entity entity)
        {
            if (entity is ResourceNode && buildingType.HasSkill<Skills.ExtractAlagene>())
                if (((ResourceNode)entity).Type == ResourceType.Alagene)
                    inputManager.LaunchBuild(((ResourceNode)entity).Position, buildingType);
        }

        public override void Execute(Vector2 at)
        {
            inputManager.LaunchBuild(at, buildingType);
        }
        #endregion
    }
}
