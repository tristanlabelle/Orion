using System;
using Orion.Engine.Graphics;
using Orion.GameLogic;
using Orion.GameLogic.Skills;
using Orion.Graphics;
using Orion.Matchmaking;
using Orion.UserInterface.Actions.UserCommands;
using Orion.UserInterface.Widgets;
using Keys = System.Windows.Forms.Keys;

namespace Orion.UserInterface.Actions.Enablers
{
    public sealed class MoveEnabler : ActionEnabler
    {
        #region Fields
        private readonly MoveUserCommand userCommand;
        #endregion

        #region Constructors
        public MoveEnabler(UserInputManager inputManager, ActionFrame actionFrame, TextureManager textureManager)
            : base(inputManager, actionFrame, textureManager)
        {
            userCommand = new MoveUserCommand(inputManager);
        }
        #endregion

        #region Methods
        public override void LetFill(UnitType type, ActionButton[,] buttonsArray)
        {
            if (!type.HasSkill<MoveSkill>()) return;

            buttonsArray[0, 3] = CreateMoveButton();
            buttonsArray[3, 3] = CreateStandGuardButton();
        }

        private ActionButton CreateMoveButton()
        {
            ActionButton button = new ActionButton(actionFrame, inputManager, "Move", Keys.M, textureManager);

            Texture texture = textureManager.GetAction("Move");
            button.Renderer = new TexturedFrameRenderer(texture);

            button.Triggered += delegate(Button sender)
            {
                inputManager.SelectedCommand = userCommand;
                actionFrame.Push(new CancelActionProvider(actionFrame, inputManager, textureManager));
            };

            return button;
        }

        private ActionButton CreateStandGuardButton()
        {
            ActionButton button = new ActionButton(actionFrame, inputManager, "Stand Guard", Keys.G, textureManager);

            Texture texture = textureManager.GetAction("Stand Guard");
            button.Renderer = new TexturedFrameRenderer(texture);

            button.Triggered += delegate(Button sender)
            {
                inputManager.LaunchStandGuard();
            };

            return button;
        }
        #endregion
    }
}
