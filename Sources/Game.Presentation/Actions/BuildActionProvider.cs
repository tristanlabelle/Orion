using System;
using System.Diagnostics;
using System.Linq;
using Orion.Engine;
using Orion.Engine.Localization;
using Orion.Game.Presentation.Actions.UserCommands;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Skills;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Presentation.Actions
{
    /// <summary>
    /// Provides the buttons to build each <see cref="Entity"/> that is supported by a builder unit.
    /// </summary>
    public sealed class BuildActionProvider : IActionProvider
    {
        #region Fields
        private readonly ActionPanel actionPanel;
        private readonly UserInputManager inputManager;
        private readonly GameGraphics graphics;
        private readonly Localizer localizer;
        private readonly Entity prototype;
        private readonly ActionDescriptor[,] actions = new ActionDescriptor[4, 4];
        #endregion

        #region Constructors
        public BuildActionProvider(ActionPanel actionPanel, UserInputManager inputManager,
            GameGraphics graphics,  Localizer localizer, Entity prototype)
        {
            Argument.EnsureNotNull(actionPanel, "actionPanel");
            Argument.EnsureNotNull(inputManager, "inputManager");
            Argument.EnsureNotNull(graphics, "graphics");
            Argument.EnsureNotNull(localizer, "localizer");
            Argument.EnsureNotNull(prototype, "prototype");

            this.actionPanel = actionPanel;
            this.inputManager = inputManager;
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
            DisposeButtons();
            CreateButtons();
        }

        public void Dispose()
        {
            DisposeButtons();
        }

        private void CreateButtons()
        {
            Builder builder = prototype.Components.TryGet<Builder>();
            Debug.Assert(builder != null);

            int x = 0;
            int y = 3;

            var buildingTypes = inputManager.Match.UnitTypes
                .Where(buildingType => builder.Supports(buildingType))
                .OrderByDescending(buildingType => buildingType.Components.Has<Trainer>())
                .ThenBy(buildingType => (int)buildingType.GetStatValue(Identity.AladdiumCostStat)
                    + (int)buildingType.GetStatValue(Identity.AlageneCostStat));

            foreach (Unit buildingType in buildingTypes)
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

        private ActionDescriptor CreateButton(Unit buildingType)
        {
            Faction faction = inputManager.LocalFaction;
            int aladdiumCost = (int)faction.GetStat(buildingType, Identity.AladdiumCostStat);
            int alageneCost = (int)faction.GetStat(buildingType, Identity.AlageneCostStat);
            
            return new ActionDescriptor()
            {
                Name = localizer.GetNoun(buildingType.Identity.Name),
                Cost = new ResourceAmount(aladdiumCost, alageneCost),
                Texture = graphics.GetEntityTexture(buildingType),
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
