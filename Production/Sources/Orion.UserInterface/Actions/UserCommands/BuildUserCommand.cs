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
            if (buildingType.HasSkill<Skills.ExtractAlagene>())
            {
                if (!(entity is ResourceNode)) return;
                ResourceNode node = (ResourceNode)entity;
                if (node.Type != ResourceType.Alagene) return;
                inputManager.LaunchBuild(node.Position, buildingType);
            }
        }

        public override void Execute(Vector2 at)
        {
            if (buildingType.HasSkill<Skills.ExtractAlagene>()) return;
            inputManager.LaunchBuild((Point)(at - (Vector2)buildingType.Size * 0.5f), buildingType);
        }
        #endregion
    }
}
