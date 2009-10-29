using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Color = System.Drawing.Color;

using Orion.Geometry;
using Orion.GameLogic;
using Orion.Commandment;

namespace Orion.Graphics
{
    public class MatchRenderer : IRenderer
    {
        #region Nested Types
        public class Minimap : FrameRenderer
        {
            private TerrainRenderer terrain;
            private UnitRenderer units;

            internal Minimap(MatchRenderer renderer)
            {
                terrain = renderer.worldRenderer.TerrainRenderer;
                units = renderer.worldRenderer.UnitRenderer;
            }

            internal Rectangle VisibleRect { get; set; }

            public override void RenderInto(GraphicsContext context)
            {
                terrain.Draw(context);
                units.DrawMiniature(context);
                context.StrokeColor = Color.Orange;
                context.Stroke(VisibleRect);
            }
        }
        #endregion

        #region Fields
        private SelectionRenderer selectionRenderer;
        private WorldRenderer worldRenderer;
        private Minimap minimap;
        #endregion

        public MatchRenderer(World world, UserInputCommander commander)
        {
            selectionRenderer = new SelectionRenderer(commander.SelectionManager);
            worldRenderer = new WorldRenderer(world, commander.Faction.FogOfWar);
            minimap = new Minimap(this);
        }

        public Minimap MinimapRenderer
        {
            get { return minimap; }
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
            minimap.VisibleRect = context.CoordinateSystem;
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
