using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Matchmaking;
using Orion.GameLogic;
using Orion.Graphics;
using Orion.GameLogic.Skills;
using System.Diagnostics;
using Keys = System.Windows.Forms.Keys;
using Orion.UserInterface.Widgets;
using Orion.UserInterface.Actions.UserCommands;

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
        private readonly TextureManager textureManager;
        private readonly ActionButton[,] buttons = new ActionButton[4, 4];
        #endregion

        #region Constructors
        public BuildActionProvider(ActionFrame actionFrame, UserInputManager inputManager, UnitType unitType,
            UnitTypeRegistry unitTypeRegistry, TextureManager textureManager)
        {
            Argument.EnsureNotNull(actionFrame, "actionFrame");
            Argument.EnsureNotNull(inputManager, "inputManager");
            Argument.EnsureNotNull(unitType, "unitType");
            Argument.EnsureNotNull(unitTypeRegistry, "unitTypeRegistry");
            Argument.EnsureNotNull(textureManager, "textureManager");

            this.actionFrame = actionFrame;
            this.inputManager = inputManager;
            this.unitType = unitType;
            this.unitTypeRegistry = unitTypeRegistry;
            this.textureManager = textureManager;

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
            BuildSkill buildSkill = unitType.GetSkill<BuildSkill>();
            Debug.Assert(buildSkill != null);

            int x = 0;
            int y = 3;
            foreach (UnitType buildingType in unitTypeRegistry.Where(u => buildSkill.Supports(u)))
            {
                buttons[x, y] = CreateButton(buildingType);

                x++;
                if (x == 4)
                {
                    y--;
                    x = 0;
                }
            }

            buttons[3, 0] = actionFrame.CreateCancelButton(inputManager, textureManager);
        }

        private ActionButton CreateButton(UnitType buildingType)
        {
            ActionButton button = new ActionButton(actionFrame, inputManager, buildingType.Name, Keys.None, textureManager);

            Texture texture = textureManager.GetUnit(buildingType.Name);
            button.Renderer = new TexturedFrameRenderer(texture);

            Faction faction = inputManager.LocalCommander.Faction;
            int aladdium = faction.GetStat(buildingType, UnitStat.AladdiumCost);
            int alagene = faction.GetStat(buildingType, UnitStat.AlageneCost);
            button.Name = "{0}\nAladdium: {1} / Alagene: {2}".FormatInvariant(buildingType.Name, aladdium, alagene);

            button.Triggered += delegate(Button sender)
            {
                inputManager.SelectedCommand = new BuildUserCommand(inputManager, textureManager, buildingType);
                actionFrame.Push(new CancelActionProvider(actionFrame, inputManager, textureManager));
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
