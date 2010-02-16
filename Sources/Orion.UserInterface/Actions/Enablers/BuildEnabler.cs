using Orion.Matchmaking;
using Orion.GameLogic;
using Orion.GameLogic.Skills;
using Orion.Graphics;
using Orion.UserInterface.Actions.UserCommands;
using Keys = System.Windows.Forms.Keys;
using Orion.UserInterface.Widgets;

namespace Orion.UserInterface.Actions.Enablers
{
    public sealed class BuildEnabler : ActionEnabler
    {
        #region Fields
        private readonly RepairUserCommand repairUserCommand;
        #endregion

        #region Constructors
        public BuildEnabler(UserInputManager inputManager, ActionFrame actionFrame, TextureManager textureManager)
            : base(inputManager, actionFrame, textureManager)
        {
            this.repairUserCommand = new RepairUserCommand(inputManager);
        }
        #endregion

        #region Methods
        public override void LetFill(UnitType unitType, ActionButton[,] buttonsArray)
        {
            if (!unitType.HasSkill<BuildSkill>()) return;
            
            buttonsArray[0, 0] = CreateBuildButton(unitType);
            buttonsArray[1, 0] = CreateRepairButton();
        }

        private ActionButton CreateBuildButton(UnitType unitType)
        {
            ActionButton button = new ActionButton(actionFrame, inputManager, "Build", Keys.B, textureManager);

            Texture texture = textureManager.GetAction("Build");
            button.Renderer = new TexturedFrameRenderer(texture);

            button.Triggered += delegate(Button sender)
            {
                actionFrame.Push(new BuildActionProvider(actionFrame, inputManager, unitType, World.UnitTypes, textureManager));
            };

            return button;
        }

        private ActionButton CreateRepairButton()
        {
            ActionButton button = new ActionButton(actionFrame, inputManager, "Repair", Keys.R, textureManager);

            Texture texture = textureManager.GetAction("Repair");
            button.Renderer = new TexturedFrameRenderer(texture);

            button.Triggered += delegate(Button sender)
            {
                inputManager.SelectedCommand = repairUserCommand;
                actionFrame.Push(new CancelActionProvider(actionFrame, inputManager, textureManager));
            };

            return button;
        }
        #endregion
    }
}
