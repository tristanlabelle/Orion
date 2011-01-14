using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Game.Presentation;
using Orion.Game.Presentation.Actions.UserCommands;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Skills;
using Orion.Game.Simulation.Technologies;
using Keys = System.Windows.Forms.Keys;

namespace Orion.Game.Presentation.Actions
{
    public sealed class UnitActionProvider : IActionProvider
    {
        #region Fields
        private readonly ActionPanel actionPanel;
        private readonly UserInputManager userInputManager;
        private readonly GameGraphics graphics;
        private readonly UnitType unitType;
        private readonly ActionButton[,] buttons = new ActionButton[4, 4];
        #endregion

        #region Constructors
        public UnitActionProvider(ActionPanel actionPanel, UserInputManager userInputManager, GameGraphics graphics, UnitType unitType)
        {
            Argument.EnsureNotNull(unitType, "unitType");
            Argument.EnsureNotNull(actionPanel, "actionPanel");
            Argument.EnsureNotNull(userInputManager, "userInputManager");
            Argument.EnsureNotNull(graphics, "graphics");

            this.actionPanel = actionPanel;
            this.userInputManager = userInputManager;
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
            ClearButtons();
            CreateButtons();
        }

        public void Dispose()
        {
            ClearButtons();
        }

        private void CreateButtons()
        {
            if (unitType.HasSkill<AttackSkill>())
            {
                buttons[2, 3] = new ActionButton()
                {
                    Name = "Attaquer",
                    Texture = graphics.GetActionTexture("Attack"),
                    HotKey = Keys.A,
                    Action = () =>
                    {
                        userInputManager.SelectedCommand = new AttackUserCommand(userInputManager, graphics);
                        actionPanel.Push(new CancelActionProvider(actionPanel, userInputManager, graphics));
                    }
                };
            }

            if (unitType.HasSkill<BuildSkill>())
            {
                buttons[0, 0] = new ActionButton()
                {
                    Name = "Construire",
                    Texture = graphics.GetActionTexture("Build"),
                    HotKey = Keys.B,
                    Action = () => actionPanel.Push(new BuildActionProvider(actionPanel, userInputManager, graphics, unitType))
                };

                buttons[1, 0] = new ActionButton()
                {
                    Name = "Réparer",
                    Texture = graphics.GetActionTexture("Repair"),
                    HotKey = Keys.R,
                    Action = () =>
                    {
                        userInputManager.SelectedCommand = new RepairUserCommand(userInputManager);
                        actionPanel.Push(new CancelActionProvider(actionPanel, userInputManager, graphics));
                    }
                };
            }

            if (unitType.HasSkill<TransportSkill>())
            {
                buttons[3, 0] = new ActionButton()
                {
                    Name = "Débarquer",
                    Texture = graphics.GetActionTexture("Disembark"),
                    HotKey = Keys.D,
                    Action = () => userInputManager.LaunchDisembark()
                };
            }

            if (unitType.HasSkill<HarvestSkill>())
            {
                buttons[1, 2] = new ActionButton()
                {
                    Name = "Ramasser",
                    Texture = graphics.GetActionTexture("Harvest"),
                    HotKey = Keys.H,
                    Action = () =>
                    {
                        userInputManager.SelectedCommand = new HarvestUserCommand(userInputManager);
                        actionPanel.Push(new CancelActionProvider(actionPanel, userInputManager, graphics));
                    }
                };
            }

            if (unitType.HasSkill<HealSkill>())
            {
                buttons[3, 2] = new ActionButton()
                {
                    Name = "Soigner",
                    Texture = graphics.GetActionTexture("Heal"),
                    HotKey = Keys.H,
                    Action = () =>
                    {
                        userInputManager.SelectedCommand = new HealUserCommand(userInputManager);
                        actionPanel.Push(new CancelActionProvider(actionPanel, userInputManager, graphics));
                    }
                };
            }

            if (unitType.HasSkill<MoveSkill>())
            {
                buttons[0, 3] = new ActionButton()
                {
                    Name = "Déplacer",
                    Texture = graphics.GetActionTexture("Move"),
                    HotKey = Keys.M,
                    Action = () =>
                    {
                        userInputManager.SelectedCommand = new MoveUserCommand(userInputManager);
                        actionPanel.Push(new CancelActionProvider(actionPanel, userInputManager, graphics));
                    }
                };
            }

            if (unitType.HasSkill<SellableSkill>())
            {
                buttons[3, 0] = new ActionButton()
                {
                    Name = "Vendre",
                    Texture = graphics.GetActionTexture("Sell"),
                    Action = () => userInputManager.LaunchSell()
                };
            }

            if (unitType.HasSkill<AttackSkill>() && !unitType.HasSkill<MoveSkill>())
            {
                buttons[3, 3] = new ActionButton()
                {
                    Name = "Guarder",
                    Texture = graphics.GetActionTexture("Stand Guard"),
                    HotKey = Keys.G,
                    Action = () => userInputManager.LaunchStandGuard()
                };
            }

            if (unitType.Upgrades.Any(u => !u.IsFree))
            {
                buttons[2, 0] = new ActionButton()
                {
                    Name = "Améliorer",
                    Texture = graphics.GetActionTexture("Upgrade"),
                    Action = () => actionPanel.Push(new UpgradeActionProvider(actionPanel, userInputManager, graphics, unitType))
                };
            }

            CreateTrainButtons();
            CreateResearchButtons();
        }

        private void CreateTrainButtons()
        {
            TrainSkill trainSkill = unitType.TryGetSkill<TrainSkill>();
            if (trainSkill == null) return;

            var traineeTypes = userInputManager.Match.UnitTypes
                .Where(traineeType => trainSkill.Supports(traineeType))
                .OrderBy(traineeType => traineeType.GetBaseStat(BasicSkill.AladdiumCostStat) + traineeType.GetBaseStat(BasicSkill.AlageneCostStat));

            foreach (UnitType traineeType in traineeTypes)
            {
                Point point = FindUnusedButton();

                int aladdium = userInputManager.LocalFaction.GetStat(traineeType, BasicSkill.AladdiumCostStat);
                int alagene = userInputManager.LocalFaction.GetStat(traineeType, BasicSkill.AlageneCostStat);

                UnitType traineeTypeForClosure = traineeType;
                buttons[point.X, point.Y] = new ActionButton()
                {
                    Name = traineeType.Name,
                    Description = "Aladdium: {0} Alagene: {1}".FormatInvariant(aladdium, alagene),
                    Texture = graphics.GetUnitTexture(traineeType),
                    Action = () => userInputManager.LaunchTrain(traineeTypeForClosure)
                };
            }
        }

        private void CreateResearchButtons()
        {
            ResearchSkill researchSkill = unitType.TryGetSkill<ResearchSkill>();
            if (researchSkill == null) return;

            var technologies = userInputManager.Match.TechnologyTree.Technologies
                .Where(tech => userInputManager.LocalFaction.IsResearchable(tech) && researchSkill.Supports(tech));

            foreach (Technology technology in technologies)
            {
                Point point = FindUnusedButton();
                Technology technologyForClosure = technology;
                buttons[point.X, point.Y] = new ActionButton()
                {
                    Name = technology.Name,
                    Description = "Aladdium: {0} Alagene: {1}".FormatInvariant(technology.AladdiumCost, technology.AlageneCost),
                    Texture = graphics.GetTechnologyTexture(technology),
                    Action = () => userInputManager.LaunchResearch(technologyForClosure)
                };
            }
        }

        private Point FindUnusedButton()
        {
            int x = 0;
            int y = 3;
            while (buttons[x, y] != null)
            {
                x++;
                if (x == 4)
                {
                    x = 0;
                    y--;
                }
            }

            return new Point(x, y);
        }

        private void ClearButtons()
        {
            for (int y = 0; y < buttons.GetLength(1); ++y)
                for (int x = 0; x < buttons.GetLength(0); ++x)
                        buttons[x, y] = null;
        }
        #endregion
    }
}
