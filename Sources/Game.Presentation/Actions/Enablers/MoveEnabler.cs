using System;
using Orion.Engine.Gui;
using Orion.Engine.Graphics;
using Orion.Game.Matchmaking;
using Orion.Game.Presentation;
using Orion.Game.Presentation.Actions.UserCommands;
using Orion.Game.Presentation.Renderers;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Skills;
using Keys = System.Windows.Forms.Keys;

namespace Orion.Game.Presentation.Actions.Enablers
{
    public sealed class MoveEnabler : ActionEnabler
    {
        #region Fields
        private readonly MoveUserCommand userCommand;
        #endregion

        #region Constructors
        public MoveEnabler(UserInputManager inputManager, ActionPanel actionPanel, GameGraphics gameGraphics)
            : base(inputManager, actionPanel, gameGraphics)
        {
            userCommand = new MoveUserCommand(inputManager);
        }
        #endregion

        #region Methods
        public override void LetFill(UnitType type, ActionButton[,] buttonsArray)
        {
            if (!type.HasSkill<MoveSkill>()) return;

            buttonsArray[0, 3] = new ActionButton()
            {
            	Name = "Déplacer",
            	Texture = graphics.GetActionTexture("Move"),
            	HotKey = Keys.M,
            	Action = () =>
	            {
	                userInputManager.SelectedCommand = userCommand;
	                actionPanel.Push(new CancelActionProvider(actionPanel, userInputManager, graphics));
	            }
            };
        }
        #endregion
    }
}
