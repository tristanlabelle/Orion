using System;
using Orion.Engine.Graphics;
using Orion.Engine.Gui;
using Orion.Game.Matchmaking;
using Orion.Game.Presentation;
using Orion.Game.Presentation.Actions.UserCommands;
using Orion.Game.Presentation.Renderers;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Skills;
using Keys = System.Windows.Forms.Keys;

namespace Orion.Game.Presentation.Actions.Enablers
{
    public sealed class AttackEnabler : ActionEnabler
    {
        #region Fields
        private readonly AttackUserCommand userCommand;
        #endregion

        #region Constructors
        public AttackEnabler(UserInputManager manager, ActionPanel actionPanel, GameGraphics gameGraphics)
            : base(manager, actionPanel, gameGraphics)
        {
            this.userCommand = new AttackUserCommand(userInputManager, gameGraphics);
        }
        #endregion

        #region Methods
        public override void LetFill(UnitType type, ActionButton[,] buttonsArray)
        {
            if (!type.HasSkill<AttackSkill>()) return;

            buttonsArray[2, 3] = new ActionButton()
            {
            	Name = "Attaquer",
            	Texture = graphics.GetActionTexture("Attack"),
            	HotKey = Keys.A,
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
