using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Orion.GameLogic;
using Orion.Commandment;

namespace Orion.Graphics
{
    public class MatchRenderer : IRenderer
    {
        #region Fields
        private SelectionRenderer selectionRenderer;
        private WorldRenderer worldRenderer;
        #endregion

        public MatchRenderer(World world, UserInputCommander commander)
        {
            selectionRenderer = new SelectionRenderer(commander.SelectionManager);
            worldRenderer = new WorldRenderer(world, commander.Faction.FogOfWar);
        }

        public WorldRenderer WorldRenderer
        {
            get { return worldRenderer; }
        }

        public SelectionRenderer SelectionRenderer
        {
            get { return selectionRenderer; }
        }

        public void RenderInto(GraphicsContext context)
        {
            worldRenderer.DrawTerrain(context);
            selectionRenderer.DrawSelectionMarkers(context);
            worldRenderer.DrawResources(context);
            worldRenderer.DrawEntities(context);
            selectionRenderer.DrawHealthBars(context);
            //worldRenderer.DrawFogOfWar(context);
            selectionRenderer.DrawSelectionRectangle(context);
        }
    }
}
