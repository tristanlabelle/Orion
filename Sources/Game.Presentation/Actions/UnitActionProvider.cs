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
using Key = OpenTK.Input.Key;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Presentation.Actions
{
    public sealed class UnitActionProvider : IActionProvider
    {
        #region Fields
        private readonly ActionPanel actionPanel;
        private readonly UserInputManager userInputManager;
        private readonly GameGraphics graphics;
        private readonly Unit unitType;
        private readonly ActionDescriptor[,] actions = new ActionDescriptor[4, 4];
        #endregion

        #region Constructors
        public UnitActionProvider(ActionPanel actionPanel, UserInputManager userInputManager, GameGraphics graphics, Unit unitType)
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
        public ActionDescriptor GetActionAt(Point point)
        {
            return actions[point.X, point.Y];
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
            if (unitType.HasComponent<Attacker, AttackSkill>())
            {
                actions[2, 3] = new ActionDescriptor()
                {
                    Name = "Attaquer",
                    Texture = graphics.GetActionTexture("Attack"),
                    HotKey = Key.A,
                    Action = () =>
                    {
                        userInputManager.SelectedCommand = new AttackUserCommand(userInputManager, graphics);
                        actionPanel.Push(new CancelActionProvider(actionPanel, userInputManager, graphics));
                    }
                };
            }

            if (unitType.HasComponent<Builder, BuildSkill>())
            {
                actions[0, 0] = new ActionDescriptor()
                {
                    Name = "Construire",
                    Texture = graphics.GetActionTexture("Build"),
                    HotKey = Key.B,
                    Action = () => actionPanel.Push(new BuildActionProvider(actionPanel, userInputManager, graphics, unitType))
                };

                actions[1, 0] = new ActionDescriptor()
                {
                    Name = "Réparer",
                    Texture = graphics.GetActionTexture("Repair"),
                    HotKey = Key.R,
                    Action = () =>
                    {
                        userInputManager.SelectedCommand = new RepairUserCommand(userInputManager);
                        actionPanel.Push(new CancelActionProvider(actionPanel, userInputManager, graphics));
                    }
                };
            }

            if (unitType.HasComponent<Harvester, HarvestSkill>())
            {
                actions[1, 2] = new ActionDescriptor()
                {
                    Name = "Ramasser",
                    Texture = graphics.GetActionTexture("Harvest"),
                    HotKey = Key.H,
                    Action = () =>
                    {
                        userInputManager.SelectedCommand = new HarvestUserCommand(userInputManager);
                        actionPanel.Push(new CancelActionProvider(actionPanel, userInputManager, graphics));
                    }
                };
            }

            if (unitType.HasComponent<Healer, HealSkill>())
            {
                actions[3, 2] = new ActionDescriptor()
                {
                    Name = "Soigner",
                    Texture = graphics.GetActionTexture("Heal"),
                    HotKey = Key.H,
                    Action = () =>
                    {
                        userInputManager.SelectedCommand = new HealUserCommand(userInputManager);
                        actionPanel.Push(new CancelActionProvider(actionPanel, userInputManager, graphics));
                    }
                };
            }

            if (unitType.HasComponent<Move, MoveSkill>())
            {
                actions[0, 3] = new ActionDescriptor()
                {
                    Name = "Déplacer",
                    Texture = graphics.GetActionTexture("Move"),
                    HotKey = Key.M,
                    Action = () =>
                    {
                        userInputManager.SelectedCommand = new MoveUserCommand(userInputManager);
                        actionPanel.Push(new CancelActionProvider(actionPanel, userInputManager, graphics));
                    }
                };
            }

            if (unitType.HasComponent<Sellable, SellableSkill>())
            {
                actions[3, 0] = new ActionDescriptor()
                {
                    Name = "Vendre",
                    Texture = graphics.GetActionTexture("Sell"),
                    Action = () => userInputManager.LaunchSell()
                };
            }

            if (unitType.HasComponent<Attacker, AttackSkill>() && unitType.HasComponent<Move, MoveSkill>())
            {
                actions[3, 3] = new ActionDescriptor()
                {
                    Name = "Guarder",
                    Texture = graphics.GetActionTexture("Stand Guard"),
                    HotKey = Key.G,
                    Action = () => userInputManager.LaunchStandGuard()
                };
            }

            if (unitType.Upgrades.Any(u => !u.IsFree))
            {
                actions[2, 0] = new ActionDescriptor()
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
            Trainer trainer = unitType.Components.TryGet<Trainer>();
            if (trainer == null) return;

            var traineeTypes = userInputManager.Match.UnitTypes
                .Where(traineeType => trainer.Supports(traineeType))
                .OrderBy(traineeType => traineeType.GetStatValue(Identity.AladdiumCostStat, BasicSkill.AladdiumCostStat)
                    + traineeType.GetStatValue(Identity.AlageneCostStat, BasicSkill.AlageneCostStat));

            foreach (Unit traineeType in traineeTypes)
            {
                Point point = FindUnusedButton();

                int aladdiumCost = userInputManager.LocalFaction.GetStat(traineeType, BasicSkill.AladdiumCostStat);
                int alageneCost = userInputManager.LocalFaction.GetStat(traineeType, BasicSkill.AlageneCostStat);
                int foodCost = userInputManager.LocalFaction.GetStat(traineeType, BasicSkill.FoodCostStat);

                Unit traineeTypeForClosure = traineeType;
                actions[point.X, point.Y] = new ActionDescriptor()
                {
                    Name = traineeType.Name,
                    Cost = new ResourceAmount(aladdiumCost, alageneCost, foodCost),
                    Texture = graphics.GetUnitTexture(traineeType),
                    Action = () => userInputManager.LaunchTrain(traineeTypeForClosure)
                };
            }
        }

        private void CreateResearchButtons()
        {
            Researcher researcher = unitType.Components.TryGet<Researcher>();
            if (researcher == null) return;

            var technologies = userInputManager.Match.TechnologyTree.Technologies
                .Where(tech => userInputManager.LocalFaction.IsResearchable(tech) && researcher.Supports(tech));

            foreach (Technology technology in technologies)
            {
                Point point = FindUnusedButton();
                Technology technologyForClosure = technology;
                actions[point.X, point.Y] = new ActionDescriptor()
                {
                    Name = technology.Name,
                    Cost = new ResourceAmount(technology.AladdiumCost, technology.AlageneCost),
                    Texture = graphics.GetTechnologyTexture(technology),
                    Action = () => userInputManager.LaunchResearch(technologyForClosure)
                };
            }
        }

        private Point FindUnusedButton()
        {
            int x = 0;
            int y = 3;
            while (actions[x, y] != null)
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
            for (int y = 0; y < actions.GetLength(1); ++y)
                for (int x = 0; x < actions.GetLength(0); ++x)
                    actions[x, y] = null;
        }
        #endregion
    }
}
