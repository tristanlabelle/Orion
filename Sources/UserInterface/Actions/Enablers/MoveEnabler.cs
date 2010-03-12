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
        public MoveEnabler(UICommander uiCommander, ActionFrame actionFrame, GameGraphics gameGraphics)
            : base(uiCommander, actionFrame, gameGraphics)
        {
            userCommand = new MoveUserCommand(uiCommander);
        }
        #endregion

        #region Methods
        public override void LetFill(UnitType type, ActionButton[,] buttonsArray)
        {
            if (!type.HasSkill<MoveSkill>()) return;

            ActionButton button = new ActionButton(actionFrame, uiCommander, "Move", Keys.M, gameGraphics);

            Texture texture = gameGraphics.GetActionTexture("Move");
            button.Renderer = new TexturedFrameRenderer(texture);

            button.Triggered += delegate(Button sender)
            {
                uiCommander.SelectedCommand = userCommand;
                actionFrame.Push(new CancelActionProvider(actionFrame, uiCommander, gameGraphics));
            };

            buttonsArray[0, 3] = button;
        }
        #endregion
    }
}
