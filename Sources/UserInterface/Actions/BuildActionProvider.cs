using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Graphics;
using Orion.GameLogic;
using Orion.Graphics;
using Orion.Graphics.Renderers;
using Orion.Matchmaking;
using Orion.UserInterface.Actions.UserCommands;
using Orion.UserInterface.Widgets;
using Keys = System.Windows.Forms.Keys;

namespace Orion.UserInterface.Actions
{
    /// <summary>
    /// Provides the buttons to build each <see cref="UnitType"/> that is supported by a builder unit.
    /// </summary>
    public sealed class BuildActionProvider : IActionProvider
    {
        #region Fields
        private readonly ActionFrame actionFrame;
        private readonly UserInputManager inputManager;
        private readonly UnitType unitType;
        private readonly UnitTypeRegistry unitTypeRegistry;
        private readonly GameGraphics gameGraphics;
        private readonly ActionButton[,] buttons = new ActionButton[4, 4];
        #endregion

        #region Constructors
        public BuildActionProvider(ActionFrame actionFrame, UserInputManager inputManager, UnitType unitType,
            UnitTypeRegistry unitTypeRegistry, GameGraphics gameGraphics)
        {
            Argument.EnsureNotNull(actionFrame, "actionFrame");
            Argument.EnsureNotNull(inputManager, "inputManager");
            Argument.EnsureNotNull(unitType, "unitType");
            Argument.EnsureNotNull(unitTypeRegistry, "unitTypeRegistry");
            Argument.EnsureNotNull(gameGraphics, "gameGraphics");

            this.actionFrame = actionFrame;
            this.inputManager = inputManager;
            this.unitType = unitType;
            this.unitTypeRegistry = unitTypeRegistry;
            this.gameGraphics = gameGraphics;

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
            Debug.Assert(unitType.HasSkill(UnitSkill.Build));

            int x = 0;
            int y = 3;
            foreach (UnitType buildingType in unitTypeRegistry.Where(u => unitType.CanBuild(u)))
            {
                buttons[x, y] = CreateButton(buildingType);

                x++;
                if (x == 4)
                {
                    y--;
                    x = 0;
                }
            }

            buttons[3, 0] = actionFrame.CreateCancelButton(inputManager, gameGraphics);
        }

        private ActionButton CreateButton(UnitType buildingType)
        {
            ActionButton button = new ActionButton(actionFrame, inputManager, buildingType.Name, Keys.None, gameGraphics);

            Texture texture = gameGraphics.GetUnitTexture(buildingType);
            button.Renderer = new TexturedFrameRenderer(texture);

            Faction faction = inputManager.LocalFaction;
            int aladdium = faction.GetStat(buildingType, UnitStat.AladdiumCost);
            int alagene = faction.GetStat(buildingType, UnitStat.AlageneCost);
            button.Name = "{0}\nAladdium: {1} / Alagene: {2}".FormatInvariant(buildingType.Name, aladdium, alagene);

            button.Triggered += delegate(Button sender)
            {
                inputManager.SelectedCommand = new BuildUserCommand(inputManager, gameGraphics, buildingType);
                actionFrame.Push(new CancelActionProvider(actionFrame, inputManager, gameGraphics));
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
