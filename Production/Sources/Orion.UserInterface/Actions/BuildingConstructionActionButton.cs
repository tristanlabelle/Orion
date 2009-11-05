using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Keys = System.Windows.Forms.Keys;

using Skills = Orion.GameLogic.Skills;
using Orion.Commandment;
using Orion.GameLogic;
using OpenTK.Math;

namespace Orion.UserInterface.Actions
{
    public class BuildingConstructionActionButton : ActionButton
    {
        #region Nested Types
        private class BuildUserCommand : UserInputCommand
        {
            private UnitType buildingType;
            private UserInputManager inputManager;

            public BuildUserCommand(UserInputManager manager, UnitType type)
            {
                buildingType = type;
                inputManager = manager;
            }

            public override void Target(Entity entity)
            {
                // todo: don't silently fail
            }

            public override void Target(Vector2 at)
            {
                inputManager.LaunchBuild(at, buildingType);
            }
        }
        #endregion

        #region Fields
        private UnitType builtType;
        #endregion

        #region Constructor
        public BuildingConstructionActionButton(ActionFrame frame, UserInputManager manager, UnitType builtType)
            : base(frame, manager, builtType.Name, Keys.None)
        {
            this.builtType = builtType;
        }
        #endregion

        #region Methods
        protected override void OnPress()
        {
            inputManager.SelectedCommand = new BuildUserCommand(inputManager, builtType);
            base.OnPress();
        }
        #endregion
    }
}
