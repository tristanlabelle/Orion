using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Graphics;
using Orion.Engine.Gui;
using Orion.Game.Matchmaking;
using Orion.Game.Presentation;
using Orion.Game.Presentation.Actions.UserCommands;
using Orion.Game.Presentation.Renderers;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Skills;
using Keys = System.Windows.Forms.Keys;

namespace Orion.Game.Presentation.Actions
{
    /// <summary>
    /// Provides the buttons to build each <see cref="UnitType"/> that is supported by a builder unit.
    /// </summary>
    public sealed class BuildActionProvider : IActionProvider
    {
        #region Fields
        private readonly ActionPanel actionPanel;
        private readonly UserInputManager inputManager;
        private readonly GameGraphics graphics;
        private readonly UnitType unitType;
        private readonly ActionDescriptor[,] actions = new ActionDescriptor[4, 4];
        #endregion

        #region Constructors
        public BuildActionProvider(ActionPanel actionPanel, UserInputManager inputManager,
            GameGraphics graphics, UnitType unitType)
        {
            Argument.EnsureNotNull(actionPanel, "actionPanel");
            Argument.EnsureNotNull(inputManager, "inputManager");
            Argument.EnsureNotNull(graphics, "graphics");
            Argument.EnsureNotNull(unitType, "unitType");

            this.actionPanel = actionPanel;
            this.inputManager = inputManager;
            this.graphics = graphics;
            this.unitType = unitType;

            CreateButtons();
        }
        #endregion

        #region Methods
        public ActionDescriptor GetActionAt(Point point)
        {
            return actions[point.X, point.Y];
        }

        public void Refresh()
        {
            DisposeButtons();
            CreateButtons();
        }

        public void Dispose()
        {
            DisposeButtons();
        }

        private void CreateButtons()
        {
            BuildSkill buildSkill = unitType.TryGetSkill<BuildSkill>();
            Debug.Assert(buildSkill != null);

            int x = 0;
            int y = 3;

            var buildingTypes = inputManager.Match.UnitTypes
                .Where(buildingType => buildSkill.Supports(buildingType))
                .OrderByDescending(buildingType => buildingType.HasSkill<TrainSkill>())
                .ThenBy(buildingType => buildingType.GetBaseStat(BasicSkill.AladdiumCostStat) + buildingType.GetBaseStat(BasicSkill.AlageneCostStat));

            foreach (UnitType buildingType in buildingTypes)
            {
                actions[x, y] = CreateButton(buildingType);

                x++;
                if (x == 4)
                {
                    y--;
                    x = 0;
                }
            }

            actions[3, 0] = actionPanel.CreateCancelAction(inputManager, graphics);
        }

        private ActionDescriptor CreateButton(UnitType buildingType)
        {
        	Faction faction = inputManager.LocalFaction;
            int aladdiumCost = faction.GetStat(buildingType, BasicSkill.AladdiumCostStat);
            int alageneCost = faction.GetStat(buildingType, BasicSkill.AlageneCostStat);
        	
            return new ActionDescriptor()
            {
            	Name = buildingType.Name,
            	Description = "Aladdium: {0} / Alagene: {1}".FormatInvariant(aladdiumCost, alageneCost),
            	Texture = graphics.GetUnitTexture(buildingType),
            	Action = () =>
	            {
	                inputManager.SelectedCommand = new BuildUserCommand(inputManager, graphics, buildingType);
	                actionPanel.Push(new CancelActionProvider(actionPanel, inputManager, graphics));
	            }
            };
        }

        private void DisposeButtons()
        {
            for (int y = 0; y < actions.GetLength(1); ++y)
                for (int x = 0; x < actions.GetLength(0); ++x)
                    actions[x, y] = null;
        }
        #endregion
    }
}
