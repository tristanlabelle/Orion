﻿using System;
using Orion.Matchmaking;
using Orion.GameLogic;
using Orion.Geometry;

namespace Orion.Graphics.Renderers
{
    public sealed class MatchRenderer : IRenderer, IDisposable
    {
        #region Fields
        private readonly UserInputManager inputManager;
        private readonly SelectionRenderer selectionRenderer;
        private readonly WorldRenderer worldRenderer;
        private readonly MinimapRenderer minimap;
        private readonly TextureManager textureManager;
        #endregion

        #region Constructors
        public MatchRenderer(UserInputManager inputManager, TextureManager textureManager)
        {
            Argument.EnsureNotNull(inputManager, "inputManager");
            Argument.EnsureNotNull(textureManager, "textureManager");

            this.textureManager = textureManager;
            this.inputManager = inputManager;
            this.selectionRenderer = new SelectionRenderer(inputManager);
            this.worldRenderer = new WorldRenderer(inputManager.LocalCommander.Faction, textureManager);
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
        public void Draw(GraphicsContext context)
        {
            Argument.EnsureNotNull(context, "context");

            minimap.VisibleRect = context.CoordinateSystem;
            worldRenderer.DrawTerrain(context);
            worldRenderer.DrawResources(context);
            worldRenderer.DrawUnits(context);
            selectionRenderer.DrawSelectionMarkers(context);

            if (SelectionManager.HoveredUnit != null && Faction.CanSee(SelectionManager.HoveredUnit))
                HealthBarRenderer.Draw(context, SelectionManager.HoveredUnit);

            worldRenderer.DrawExplosions(context);
            worldRenderer.DrawFogOfWar(context);

            if (inputManager.SelectedCommand is IRenderer)
                ((IRenderer)inputManager.SelectedCommand).Draw(context);
            selectionRenderer.DrawSelectionRectangle(context);
        }

        public void Dispose()
        {
            worldRenderer.Dispose();
        }
        #endregion
    }
}
