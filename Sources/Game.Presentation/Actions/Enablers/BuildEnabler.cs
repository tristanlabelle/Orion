using System;
using Orion.Engine.Graphics;
using Orion.Engine.Gui;
using Orion.Game.Presentation;
using Orion.Game.Presentation.Actions.UserCommands;
using Orion.Game.Presentation.Renderers;
using Orion.Game.Matchmaking;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Skills;
using Keys = System.Windows.Forms.Keys;

namespace Orion.Game.Presentation.Actions.Enablers
{
    public sealed class BuildEnabler : ActionEnabler
    {
        #region Fields
        private readonly RepairUserCommand repairUserCommand;
        #endregion

        #region Constructors
        public BuildEnabler(UserInputManager inputManager, ActionPanel actionPanel, GameGraphics graphics)
            : base(inputManager, actionPanel, graphics)
        {
            this.repairUserCommand = new RepairUserCommand(inputManager);
        }
        #endregion

        #region Methods
        public override void LetFill(UnitType unitType, ActionButton[,] buttonsArray)
        {
            if (!unitType.HasSkill<BuildSkill>()) return;
            
            buttonsArray[0, 0] = new ActionButton()
            {
            	Name = "Construire",
            	Texture = graphics.GetActionTexture("Build"),
            	HotKey = Keys.B,
            	Action = () => actionPanel.Push(new BuildActionProvider(actionPanel, userInputManager, graphics, unitType))
            };
            
            buttonsArray[1, 0] = new ActionButton()
            {
            	Name = "Réparer",
            	Texture = graphics.GetActionTexture("Repair"),
            	HotKey = Keys.R,
            	Action = () =>
	            {
	                userInputManager.SelectedCommand = repairUserCommand;
	                actionPanel.Push(new CancelActionProvider(actionPanel, userInputManager, graphics));
	            }
            };
        }
        #endregion
    }
}
