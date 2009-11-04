using Orion.Commandment;
using Orion.GameLogic;
using Orion.Geometry;
using Color = System.Drawing.Color;

namespace Orion.Graphics
{
    public class MatchRenderer : IRenderer
    {
        #region Nested Types
        public class Minimap : FrameRenderer
        {
            private TerrainRenderer terrain;
            private UnitRenderer units;
            private FogOfWarRenderer fogOfWar;

            internal Minimap(MatchRenderer renderer)
            {
                terrain = renderer.worldRenderer.TerrainRenderer;
                units = renderer.worldRenderer.UnitRenderer;
                fogOfWar = renderer.worldRenderer.FogOfWarRenderer;
            }

            internal Rectangle VisibleRect { get; set; }

            public override void RenderInto(GraphicsContext context)
            {
                terrain.Draw(context);
                units.DrawMiniature(context);
                fogOfWar.Draw(context);
                
                context.StrokeColor = Color.Orange;
                Rectangle? intersection = Rectangle.Intersection(context.CoordinateSystem, VisibleRect);
                context.Stroke(intersection.GetValueOrDefault());
            }
        }
        #endregion

        #region Fields
        private SelectionRenderer selectionRenderer;
        private WorldRenderer worldRenderer;
        private Minimap minimap;
        #endregion

        public MatchRenderer(World world, UserInputManager inputManager)
        {
            selectionRenderer = new SelectionRenderer(inputManager);
            worldRenderer = new WorldRenderer(world, inputManager.Commander.Faction.FogOfWar);
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
            worldRenderer.DrawFogOfWar(context);
            selectionRenderer.DrawSelectionRectangle(context);
        }
    }
}
