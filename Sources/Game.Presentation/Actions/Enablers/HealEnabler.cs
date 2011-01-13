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
    public sealed class HealEnabler : ActionEnabler
    {
        #region Fields
        private readonly HealUserCommand userCommand;
        #endregion

        #region Constructors
        public HealEnabler(UserInputManager inputManager, ActionPanel actionPanel, GameGraphics gameGraphics)
            : base(inputManager, actionPanel, gameGraphics)
        {
            this.userCommand = new HealUserCommand(inputManager);
        }
        #endregion

        #region Methods
        public override void LetFill(UnitType type, ActionButton[,] buttonsArray)
        {
            if (!type.HasSkill<HealSkill>()) return;

            buttonsArray[3, 2] = new ActionButton()
            {
            	Name = "Soigner",
            	Texture = graphics.GetActionTexture("Heal"),
            	HotKey = Keys.H,
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