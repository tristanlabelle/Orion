using System;
using System.Linq;
using Orion.Engine;
using Orion.Game.Simulation;

namespace Orion.Game.Presentation.Actions
{
    /// <summary>
    /// Provides the buttons to build each <see cref="Entity"/> that is supported by a builder unit.
    /// </summary>
    public sealed class UpgradeActionProvider : IActionProvider
    {
        #region Fields
        private readonly ActionPanel actionPanel;
        private readonly UserInputManager inputManager;
        private readonly GameGraphics graphics;
        private readonly Entity prototype;
        private readonly ActionDescriptor[,] buttons = new ActionDescriptor[4, 4];
        #endregion

        #region Constructors
        public UpgradeActionProvider(ActionPanel actionPanel, UserInputManager inputManager,
            GameGraphics graphics, Entity prototype)
        {
            Argument.EnsureNotNull(actionPanel, "actionPanel");
            Argument.EnsureNotNull(inputManager, "inputManager");
            Argument.EnsureNotNull(graphics, "graphics");
            Argument.EnsureNotNull(prototype, "prototype");

            this.actionPanel = actionPanel;
            this.inputManager = inputManager;
            this.graphics = graphics;
            this.prototype = prototype;

            CreateButtons();
        }
        #endregion

        #region Methods
        public ActionDescriptor GetActionAt(Point point)
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
            int x = 0;
            int y = 3;

            foreach (EntityUpgrade upgrade in prototype.Identity.Upgrades.Where(u => !u.IsFree))
            {
                Entity targetPrototype = inputManager.Match.Prototypes.FromName(upgrade.Target);
                if (targetPrototype == null) continue;

                buttons[x, y] = new ActionDescriptor()
                {
                    Name = upgrade.Target,
                    Cost = new ResourceAmount(upgrade.AladdiumCost, upgrade.AlageneCost),
                    Texture = graphics.GetEntityTexture(targetPrototype),
                    Action = () => inputManager.LaunchUpgrade(targetPrototype)
                };

                x++;
                if (x == 4)
                {
                    y--;
                    x = 0;
                }
            }

            buttons[3, 0] = actionPanel.CreateCancelAction(inputManager, graphics);
        }

        private void ClearButtons()
        {
            for (int y = 0; y < buttons.GetLength(1); ++y)
                for (int x = 0; x < buttons.GetLength(0); ++x)
                    if (buttons[x, y] != null)
                        buttons[x, y] = null;
        }
        #endregion
    }
}
