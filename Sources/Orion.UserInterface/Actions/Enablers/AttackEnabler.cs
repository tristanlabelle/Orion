using Orion.Commandment;
using Orion.GameLogic;
using Orion.GameLogic.Skills;
using Orion.Graphics;
using Orion.UserInterface.Actions.UserCommands;
using Keys = System.Windows.Forms.Keys;
using Orion.UserInterface.Widgets;

namespace Orion.UserInterface.Actions.Enablers
{
    public sealed class AttackEnabler : ActionEnabler
    {
        #region Fields
        private readonly AttackUserCommand userCommand;
        #endregion

        #region Constructors
        public AttackEnabler(UserInputManager manager, ActionFrame frame, TextureManager textureManager)
            : base(manager, frame, textureManager)
        {
            this.userCommand = new AttackUserCommand(inputManager);
        }
        #endregion

        #region Methods
        public override void LetFill(UnitType type, ActionButton[,] buttonsArray)
        {
            if (!type.HasSkill<AttackSkill>()) return;

            ActionButton button = new ActionButton(actionFrame, inputManager, "Attack", Keys.A, textureManager);

            Texture texture = textureManager.GetAction("Attack");
            button.Renderer = new TexturedFrameRenderer(texture);

            button.Triggered += OnButtonPressed;

            buttonsArray[2, 3] = button;
        }

        private void OnButtonPressed(Button button)
        {
            inputManager.SelectedCommand = userCommand;
            actionFrame.Push(new CancelActionProvider(actionFrame, inputManager, textureManager));
        }
        #endregion
    }
}
