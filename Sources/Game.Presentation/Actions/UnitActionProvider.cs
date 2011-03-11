using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Localization;
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
        private readonly Localizer localizer;
        private readonly Entity prototype;
        private readonly ActionDescriptor[,] actions = new ActionDescriptor[4, 4];
        #endregion

        #region Constructors
        public UnitActionProvider(ActionPanel actionPanel, UserInputManager userInputManager,
            GameGraphics graphics, Localizer localizer, Entity prototype)
        {
            Argument.EnsureNotNull(prototype, "unitType");
            Argument.EnsureNotNull(actionPanel, "actionPanel");
            Argument.EnsureNotNull(userInputManager, "userInputManager");
            Argument.EnsureNotNull(graphics, "graphics");
            Argument.EnsureNotNull(localizer, "localizer");
            Argument.EnsureNotNull(prototype, "prototype");
            
            this.actionPanel = actionPanel;
            this.userInputManager = userInputManager;
            this.graphics = graphics;
            this.localizer = localizer;
            this.prototype = prototype;

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
            if (prototype.Components.Has<Attacker>())
            {
                actions[2, 3] = new ActionDescriptor()
                {
                    Name = localizer.GetNoun("Attack"),
                    Texture = graphics.GetActionTexture("Attack"),
                    HotKey = Key.A,
                    Action = () =>
                    {
                        userInputManager.SelectedCommand = new AttackUserCommand(userInputManager, graphics);
                        actionPanel.Push(new CancelActionProvider(actionPanel, userInputManager, graphics));
                    }
                };
            }

            if (prototype.Components.Has<Builder>())
            {
                var buildActionProvider = new BuildActionProvider(actionPanel, userInputManager, graphics, localizer, prototype);
                actions[0, 0] = new ActionDescriptor()
                {
                    Name = localizer.GetNoun("Build"),
                    Texture = graphics.GetActionTexture("Build"),
                    HotKey = Key.B,
                    Action = () => actionPanel.Push(buildActionProvider)
                };

                actions[1, 0] = new ActionDescriptor()
                {
                    Name = localizer.GetNoun("Repair"),
                    Texture = graphics.GetActionTexture("Repair"),
                    HotKey = Key.R,
                    Action = () =>
                    {
                        userInputManager.SelectedCommand = new RepairUserCommand(userInputManager);
                        actionPanel.Push(new CancelActionProvider(actionPanel, userInputManager, graphics));
                    }
                };
            }

            if (prototype.Components.Has<Harvester>())
            {
                actions[1, 2] = new ActionDescriptor()
                {
                    Name = localizer.GetNoun("Harvest"),
                    Texture = graphics.GetActionTexture("Harvest"),
                    HotKey = Key.H,
                    Action = () =>
                    {
                        userInputManager.SelectedCommand = new HarvestUserCommand(userInputManager);
                        actionPanel.Push(new CancelActionProvider(actionPanel, userInputManager, graphics));
                    }
                };
            }

            if (prototype.Components.Has<Healer>())
            {
                actions[3, 2] = new ActionDescriptor()
                {
                    Name = localizer.GetNoun("Heal"),
                    Texture = graphics.GetActionTexture("Heal"),
                    HotKey = Key.H,
                    Action = () =>
                    {
                        userInputManager.SelectedCommand = new HealUserCommand(userInputManager);
                        actionPanel.Push(new CancelActionProvider(actionPanel, userInputManager, graphics));
                    }
                };
            }

            if (prototype.Components.Has<Mobile>())
            {
                actions[0, 3] = new ActionDescriptor()
                {
                    Name = localizer.GetNoun("Move"),
                    Texture = graphics.GetActionTexture("Move"),
                    HotKey = Key.M,
                    Action = () =>
                    {
                        userInputManager.SelectedCommand = new MoveUserCommand(userInputManager);
                        actionPanel.Push(new CancelActionProvider(actionPanel, userInputManager, graphics));
                    }
                };
            }

            if (prototype.Components.Has<Sellable>())
            {
                actions[3, 0] = new ActionDescriptor()
                {
                    Name = localizer.GetNoun("Sell"),
                    Texture = graphics.GetActionTexture("Sell"),
                    Action = () => userInputManager.LaunchSell()
                };
            }

            if (prototype.Components.Has<Attacker>() && prototype.Components.Has<Mobile>())
            {
                actions[3, 3] = new ActionDescriptor()
                {
                    Name = localizer.GetNoun("StandGuard"),
                    Texture = graphics.GetActionTexture("Stand Guard"),
                    HotKey = Key.G,
                    Action = () => userInputManager.LaunchStandGuard()
                };
            }

            if (prototype.Identity.Upgrades.Any(u => !u.IsFree))
            {
                actions[2, 0] = new ActionDescriptor()
                {
                    Name = localizer.GetNoun("Upgrade"),
                    Texture = graphics.GetActionTexture("Upgrade"),
                    Action = () => actionPanel.Push(new UpgradeActionProvider(actionPanel, userInputManager, graphics, prototype))
                };
            }

            CreateTrainButtons();
            CreateResearchButtons();
        }

        private void CreateTrainButtons()
        {
            Trainer trainer = prototype.Components.TryGet<Trainer>();
            if (trainer == null) return;

            var traineePrototypes = userInputManager.Match.UnitTypes
                .Where(traineeType => trainer.Supports(traineeType))
                .OrderBy(traineeType => (int)traineeType.GetStatValue(Identity.AladdiumCostStat)
                    + (int)traineeType.GetStatValue(Identity.AlageneCostStat));

            foreach (Unit traineePrototype in traineePrototypes)
            {
                Point point = FindUnusedButton();

                int aladdiumCost = (int)userInputManager.LocalFaction.GetStat(traineePrototype, Identity.AladdiumCostStat);
                int alageneCost = (int)userInputManager.LocalFaction.GetStat(traineePrototype, Identity.AlageneCostStat);
                int foodCost = (int)userInputManager.LocalFaction.GetStat(traineePrototype, FactionMembership.FoodCostStat);

                Unit traineeTypeForClosure = traineePrototype;
                actions[point.X, point.Y] = new ActionDescriptor()
                {
                    Name = localizer.GetNoun(traineePrototype.Identity.Name),
                    Cost = new ResourceAmount(aladdiumCost, alageneCost, foodCost),
                    Texture = graphics.GetEntityTexture(traineePrototype),
                    Action = () => userInputManager.LaunchTrain(traineeTypeForClosure)
                };
            }
        }

        private void CreateResearchButtons()
        {
            Researcher researcher = prototype.Components.TryGet<Researcher>();
            if (researcher == null) return;

            foreach (Technology technology in userInputManager.Match.TechnologyTree.Technologies)
            {
                if (!userInputManager.LocalFaction.IsResearchable(technology) || !researcher.Supports(technology))
                    continue;

                Point point = FindUnusedButton();
                Technology technologyForClosure = technology;
                actions[point.X, point.Y] = new ActionDescriptor()
                {
                    Name = localizer.GetNoun(technology.Name),
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
