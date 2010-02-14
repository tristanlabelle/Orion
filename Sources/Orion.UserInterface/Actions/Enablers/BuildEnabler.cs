using Orion.Commandment;
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
        public override void LetFill(UnitType type, ActionButton[,] buttonsArray)
        {
            if (!type.HasSkill<BuildSkill>()) return;
            
            buttonsArray[0, 0] = new BuildActionButton(actionFrame, inputManager, type, World.UnitTypes, base.textureManager);
            buttonsArray[1, 0] = CreateRepairButton();
        }

        private ActionButton CreateRepairButton()
        {
            ActionButton button = new ActionButton(actionFrame, inputManager, "Repair", Keys.R, textureManager);

            Texture texture = textureManager.GetAction("Repair");
            button.Renderer = new TexturedFrameRenderer(texture);

            button.Triggered += OnRepairButtonPressed;

            return button;
        }

        private void OnRepairButtonPressed(Button button)
        {
            inputManager.SelectedCommand = repairUserCommand;
            actionFrame.Push(new CancelActionProvider(actionFrame, inputManager, textureManager));
        }
        #endregion
    }
}
