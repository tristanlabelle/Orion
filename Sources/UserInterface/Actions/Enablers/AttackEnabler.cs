using System;
using Orion.Engine.Graphics;
using Orion.Matchmaking;
using Orion.GameLogic;
using Orion.GameLogic.Skills;
using Orion.Graphics;
using Orion.UserInterface.Actions.UserCommands;
using Orion.UserInterface.Widgets;
using Keys = System.Windows.Forms.Keys;

namespace Orion.UserInterface.Actions.Enablers
{
    public sealed class AttackEnabler : ActionEnabler
    {
        #region Fields
        private readonly AttackUserCommand userCommand;
        #endregion

        #region Constructors
        public AttackEnabler(UICommander manager, ActionFrame frame, GameGraphics gameGraphics)
            : base(manager, frame, gameGraphics)
        {
            this.userCommand = new AttackUserCommand(uiCommander, gameGraphics);
        }
        #endregion

        #region Methods
        public override void LetFill(UnitType type, ActionButton[,] buttonsArray)
        {
            if (!type.HasSkill<AttackSkill>()) return;

            ActionButton button = new ActionButton(actionFrame, uiCommander, "Attack", Keys.A, gameGraphics);

            Texture texture = gameGraphics.GetActionTexture("Attack");
            button.Renderer = new TexturedFrameRenderer(texture);

            button.Triggered += OnButtonPressed;

            buttonsArray[2, 3] = button;
        }

        private void OnButtonPressed(Button button)
        {
            uiCommander.SelectedCommand = userCommand;
            actionFrame.Push(new CancelActionProvider(actionFrame, uiCommander, gameGraphics));
        }
        #endregion
    }
}
