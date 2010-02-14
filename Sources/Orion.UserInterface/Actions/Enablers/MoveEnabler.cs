using Orion.Commandment;
using Orion.GameLogic;
using Orion.GameLogic.Skills;
using Orion.Graphics;
using Orion.UserInterface.Actions.UserCommands;
using Keys = System.Windows.Forms.Keys;
using Orion.UserInterface.Widgets;

namespace Orion.UserInterface.Actions.Enablers
{
    public sealed class MoveEnabler : ActionEnabler
    {
        #region Constructors
        public MoveEnabler(UserInputManager inputManager, ActionFrame actionFrame, TextureManager textureManager)
            : base(inputManager, actionFrame, textureManager)
        { }
        #endregion

        #region Methods
        public override void LetFill(UnitType type, ActionButton[,] buttonsArray)
        {
            if (!type.HasSkill<MoveSkill>()) return;

            ActionButton button = new ActionButton(actionFrame, inputManager, "Move", Keys.M, textureManager);
            Texture texture = textureManager.GetAction("Move");
            button.Renderer = new TexturedFrameRenderer(texture);
            button.Triggered += OnButtonPressed;

            buttonsArray[0, 3] = button;
        }

        private void OnButtonPressed(Button sender)
        {
            inputManager.SelectedCommand = new MoveUserCommand(inputManager);
            actionFrame.Push(new CancelActionProvider(actionFrame, inputManager, textureManager));
        }
        #endregion
    }
}
