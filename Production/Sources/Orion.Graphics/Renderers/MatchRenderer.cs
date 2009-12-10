using System;
using Orion.Commandment;
using Orion.GameLogic;
using Orion.Geometry;
using Color = System.Drawing.Color;

namespace Orion.Graphics
{
    public class MatchRenderer : IRenderer, IDisposable
    {
        #region Fields
        private readonly UserInputManager inputManager;
        private readonly SelectionRenderer selectionRenderer;
        private readonly WorldRenderer worldRenderer;
        private readonly MinimapRenderer minimap;
        private readonly TextureManager textureManager;
        #endregion

        #region Constructors
        public MatchRenderer(World world, UserInputManager manager, TextureManager textureManager)
        {
            Argument.EnsureNotNull(world, "world");
            Argument.EnsureNotNull(manager, "manager");

            this.textureManager = textureManager;
            inputManager = manager;
            selectionRenderer = new SelectionRenderer(inputManager);
            worldRenderer = new WorldRenderer(world, inputManager.Commander.Faction, textureManager);
            minimap = new MinimapRenderer(worldRenderer);
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
            get { return worldRenderer.UnitRenderer.DrawHealthBars; }
            set { worldRenderer.UnitRenderer.DrawHealthBars = value; }
        }
        #endregion

        #region Methods
        public void Draw(GraphicsContext context)
        {
            Argument.EnsureNotNull(context, "context");

            minimap.VisibleRect = context.CoordinateSystem;
            worldRenderer.DrawTerrain(context);
            worldRenderer.DrawResources(context);
            worldRenderer.DrawUnits(context);
            selectionRenderer.DrawSelectionMarkers(context);

            if (inputManager.SelectionManager.HoveredUnit != null)
            {
                worldRenderer.UnitRenderer.DrawHealthBar(context, inputManager.SelectionManager.HoveredUnit);
            }
            worldRenderer.DrawFogOfWar(context);
            selectionRenderer.DrawSelectionRectangle(context);
        }

        public void Dispose()
        {
            worldRenderer.Dispose();
        }
        #endregion
    }
}
