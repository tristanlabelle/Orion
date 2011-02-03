using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Graphics;
using Orion.Engine.Gui;
using Orion.Game.Simulation;
using Orion.Game.Presentation;
using Orion.Game.Presentation.Renderers;
using Orion.Game.Matchmaking;
using Orion.Game.Presentation.Actions.UserCommands;
using Keys = System.Windows.Forms.Keys;
using Orion.Game.Simulation.Skills;

namespace Orion.Game.Presentation.Actions
{
    /// <summary>
    /// Provides the buttons to build each <see cref="Unit"/> that is supported by a builder unit.
    /// </summary>
    public sealed class BuildActionProvider : IActionProvider
    {
        #region Fields
        private readonly ActionPanel actionPanel;
        private readonly UserInputManager inputManager;
        private readonly GameGraphics graphics;
        private readonly Unit unitType;
        private readonly ActionButton[,] buttons = new ActionButton[4, 4];
        #endregion

        #region Constructors
        public BuildActionProvider(ActionPanel actionPanel, UserInputManager inputManager,
            GameGraphics graphics, Unit unitType)
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
        public ActionButton GetButtonAt(Point point)
        {
            return buttons[point.X, point.Y];
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

            foreach (Unit buildingType in buildingTypes)
            {
                buttons[x, y] = CreateButton(buildingType);

                x++;
                if (x == 4)
                {
                    y--;
                    x = 0;
                }
            }

            buttons[3, 0] = actionPanel.CreateCancelButton(inputManager, graphics);
        }

        private ActionButton CreateButton(Unit buildingType)
        {
            ActionButton button = new ActionButton(actionPanel, inputManager, buildingType.Name, Keys.None, graphics);

            Texture texture = graphics.GetUnitTexture(buildingType);
            button.Renderer = new TexturedRenderer(texture);

            Faction faction = inputManager.LocalFaction;
            int aladdium = faction.GetStat(buildingType, BasicSkill.AladdiumCostStat);
            int alagene = faction.GetStat(buildingType, BasicSkill.AlageneCostStat);
            button.Name = "{0}\nAladdium: {1} / Alagene: {2}".FormatInvariant(buildingType.Name, aladdium, alagene);

            button.Triggered += delegate(Button sender)
            {
                inputManager.SelectedCommand = new BuildUserCommand(inputManager, graphics, buildingType);
                actionPanel.Push(new CancelActionProvider(actionPanel, inputManager, graphics));
            };

            return button;
        }

        private void DisposeButtons()
        {
            for (int y = 0; y < buttons.GetLength(1); ++y)
            {
                for (int x = 0; x < buttons.GetLength(0); ++x)
                {
                    if (buttons[x, y] != null)
                    {
                        buttons[x, y].Dispose();
                        buttons[x, y] = null;
                    }
                }
            }
        }
        #endregion
    }
}
