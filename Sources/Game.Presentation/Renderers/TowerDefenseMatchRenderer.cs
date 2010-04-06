using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Engine.Graphics;
using Orion.Engine.Gui;
using Orion.Game.Simulation;
using Orion.Game.Matchmaking.TowerDefense;

namespace Orion.Game.Presentation.Renderers
{
    /// <summary>
    /// Draws a tower defense game to the main view and minimap.
    /// </summary>
    public sealed class TowerDefenseMatchRenderer : IMatchRenderer
    {
        #region Fields
        private readonly UserInputManager inputManager;
        private readonly GameGraphics graphics;
        private readonly SelectionRenderer selectionRenderer;
        private readonly WorldRenderer worldRenderer;
        private readonly CreepPathRenderer creepPathRenderer;
        private readonly CreepMoneyRenderer creepMoneyRenderer;
        #endregion

        #region Constructors
        public TowerDefenseMatchRenderer(UserInputManager inputManager, GameGraphics graphics,
            CreepPath creepPath)
        {
            Argument.EnsureNotNull(inputManager, "inputManager");
            Argument.EnsureNotNull(graphics, "graphics");
            Argument.EnsureNotNull(creepPath, "creepPath");

            this.inputManager = inputManager;
            this.graphics = graphics;
            this.selectionRenderer = new SelectionRenderer(inputManager);
            this.worldRenderer = new WorldRenderer(inputManager.LocalFaction, graphics);
            this.creepPathRenderer = new CreepPathRenderer(creepPath, graphics);
            this.creepMoneyRenderer = new CreepMoneyRenderer(inputManager.LocalFaction);
        }
        #endregion

        #region Properties
        public WorldRenderer WorldRenderer
        {
            get { return worldRenderer; }
        }

        private SelectionManager SelectionManager
        {
            get { return inputManager.SelectionManager; }
        }

        private Faction Faction
        {
            get { return inputManager.LocalFaction; }
        }

        private World World
        {
            get { return inputManager.World; }
        }
        #endregion

        #region Methods
        public void Draw(Rectangle visibleBounds)
        {
            GraphicsContext context = graphics.Context;

            worldRenderer.DrawTerrain(context, visibleBounds);
            creepPathRenderer.Draw(context, visibleBounds);
            worldRenderer.DrawResources(context, visibleBounds);
            worldRenderer.DrawUnits(context, visibleBounds);
            selectionRenderer.DrawSelectionMarkers(context);

            if (inputManager.HoveredUnit != null && Faction.CanSee(inputManager.HoveredUnit))
                HealthBarRenderer.Draw(context, inputManager.HoveredUnit);

            worldRenderer.DrawExplosions(context, visibleBounds);
            worldRenderer.DrawFogOfWar(context, visibleBounds);
            creepMoneyRenderer.Draw(context, visibleBounds);

            IViewRenderer selectedCommandRenderer = inputManager.SelectedCommand as IViewRenderer;
            if (selectedCommandRenderer != null)
                selectedCommandRenderer.Draw(context, visibleBounds);

            selectionRenderer.DrawSelectionRectangle(context);
        }

        public void DrawMinimap()
        {
            worldRenderer.DrawMiniatureTerrain(graphics.Context);
            creepPathRenderer.Draw(graphics.Context, World.Bounds);
            worldRenderer.DrawMiniatureResources(graphics.Context);
            worldRenderer.DrawMiniatureUnits(graphics.Context);
            worldRenderer.DrawFogOfWar(graphics.Context, World.Bounds);
            creepMoneyRenderer.Draw(graphics.Context, World.Bounds);
        }

        public void Dispose()
        {
            worldRenderer.Dispose();
        }
        #endregion
    }
}
