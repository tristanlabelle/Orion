using System;
using System.Linq;
using Orion.Engine.Graphics;
using Orion.Matchmaking;
using Orion.GameLogic;
using Orion.Geometry;
using Orion.Matchmaking.TowerDefense;

namespace Orion.Graphics.Renderers
{
    public sealed class MatchRenderer : IRenderer, IDisposable
    {
        #region Fields
        private readonly UserInputManager inputManager;
        private readonly GameGraphics gameGraphics;
        private readonly SelectionRenderer selectionRenderer;
        private readonly WorldRenderer worldRenderer;
        private readonly MinimapRenderer minimap;
        #endregion

        #region Constructors
        public MatchRenderer(UserInputManager inputManager, GameGraphics gameGraphics, CreepPath creepPath)
        {
            Argument.EnsureNotNull(inputManager, "inputManager");
            Argument.EnsureNotNull(gameGraphics, "gameGraphics");

            this.inputManager = inputManager;
            this.gameGraphics = gameGraphics;
            this.selectionRenderer = new SelectionRenderer(inputManager);
            this.worldRenderer = new WorldRenderer(inputManager.LocalFaction, gameGraphics, creepPath);
            this.minimap = new MinimapRenderer(worldRenderer);
        }
        #endregion

        #region Properties
        public MinimapRenderer MinimapRenderer
        {
            get { return minimap; }
        }

        public WorldRenderer WorldRenderer
        {
            get { return worldRenderer; }
        }

        public bool DrawAllHealthBars
        {
            get { return worldRenderer.DrawHealthBars; }
            set { worldRenderer.DrawHealthBars = value; }
        }

        private SelectionManager SelectionManager
        {
            get { return inputManager.SelectionManager; }
        }

        private Faction Faction
        {
            get { return inputManager.LocalCommander.Faction; }
        }
        #endregion

        #region Methods
        public void Draw(GraphicsContext context, Rectangle bounds)
        {
            Argument.EnsureNotNull(context, "context");

            minimap.VisibleRect = bounds;
            worldRenderer.DrawTerrain(context, bounds);
            worldRenderer.DrawResources(context, bounds);
            worldRenderer.DrawUnits(context, bounds);
            selectionRenderer.DrawSelectionMarkers(context);

            if (inputManager.HoveredUnit != null && Faction.CanSee(inputManager.HoveredUnit))
                HealthBarRenderer.Draw(context, inputManager.HoveredUnit);

            worldRenderer.DrawExplosions(context, bounds);
            worldRenderer.DrawFogOfWar(context, bounds);

            IRenderer selectedCommandRenderer = inputManager.SelectedCommand as IRenderer;
            if (selectedCommandRenderer != null)
                selectedCommandRenderer.Draw(context, bounds);

            selectionRenderer.DrawSelectionRectangle(context);
        }

        public void Dispose()
        {
            worldRenderer.Dispose();
        }
        #endregion
    }
}
