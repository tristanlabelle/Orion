using Orion.Commandment;
using Orion.GameLogic;
using Orion.GameLogic.Skills;
using Orion.Graphics;
using Orion.UserInterface.Actions.UserCommands;
using Keys = System.Windows.Forms.Keys;
using Orion.UserInterface.Widgets;

namespace Orion.UserInterface.Actions.Enablers
{
    public sealed class HarvestEnabler : ActionEnabler
    {
        #region Fields
        private readonly HarvestUserCommand userCommand;
        #endregion

        #region Constructors
        public HarvestEnabler(UserInputManager inputManager, ActionFrame actionFrame, TextureManager textureManager)
            : base(inputManager, actionFrame, textureManager)
        {
            this.userCommand = new HarvestUserCommand(inputManager);
        }
        #endregion

        #region Methods
        public override void LetFill(UnitType type, ActionButton[,] buttonsArray)
        {
            if (!type.HasSkill<HarvestSkill>()) return;

            ActionButton button = new ActionButton(actionFrame, inputManager, "Harvest", Keys.H, textureManager);

            Texture texture = textureManager.GetAction("Harvest");
            button.Renderer = new TexturedFrameRenderer(texture);

            button.Triggered += OnButtonPressed;

            buttonsArray[1, 2] = button;
        }

        private void OnButtonPressed(Button button)
        {
            inputManager.SelectedCommand = userCommand;
            actionFrame.Push(new CancelActionProvider(actionFrame, inputManager, textureManager));
        }
        #endregion
    }
}
