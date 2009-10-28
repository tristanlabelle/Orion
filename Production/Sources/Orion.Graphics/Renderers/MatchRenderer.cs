using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Orion.GameLogic;

namespace Orion.Graphics
{
    public class MatchRenderer : IRenderer
    {
        #region Fields
        private SelectionRenderer selectionRenderer;
        private WorldRenderer worldRenderer;
        #endregion

        public MatchRenderer(World world, SelectionManager selectionManager)
        {
            selectionRenderer = new SelectionRenderer(selectionManager);
            worldRenderer = new WorldRenderer(world);
        }

        public void RenderInto(GraphicsContext context)
        {
            worldRenderer.DrawTerrain(context);
            selectionRenderer.DrawSelectionMarkers(context);
            worldRenderer.DrawResources(context);
            worldRenderer.DrawEntities(context);
            selectionRenderer.DrawHealthBars(context);
            selectionRenderer.DrawSelectionRectangle(context);
        }
    }
}
