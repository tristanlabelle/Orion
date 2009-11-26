﻿using Orion.Commandment;
using Orion.GameLogic;
using Orion.Geometry;
using Color = System.Drawing.Color;

namespace Orion.Graphics
{
    public class MatchRenderer : IRenderer
    {
        #region Nested Types
        public sealed class Minimap : FrameRenderer
        {
            private WorldRenderer worldRenderer;

            internal Minimap(WorldRenderer worldRenderer)
            {
                Argument.EnsureNotNull(worldRenderer, "worldRenderer");
                this.worldRenderer = worldRenderer;
            }

            internal Rectangle VisibleRect { get; set; }

            public override void Draw(GraphicsContext context)
            {
                worldRenderer.DrawTerrain(context);
                worldRenderer.DrawResources(context);
                worldRenderer.UnitRenderer.DrawMiniature(context);
                worldRenderer.DrawFogOfWar(context);

                context.StrokeColor = Color.Orange;
                Rectangle? intersection = Rectangle.Intersection(context.CoordinateSystem, VisibleRect);
                context.Stroke(intersection.GetValueOrDefault());
                context.StrokeColor = Color.Gray;
                context.Stroke(context.CoordinateSystem);
            }
        }
        #endregion

        #region Fields
        private readonly UserInputManager inputManager;
        private readonly SelectionRenderer selectionRenderer;
        private readonly WorldRenderer worldRenderer;
        private readonly Minimap minimap;
        #endregion

        public MatchRenderer(World world, UserInputManager manager)
        {
            Argument.EnsureNotNull(world, "world");
            Argument.EnsureNotNull(manager, "manager");

            inputManager = manager;
            selectionRenderer = new SelectionRenderer(inputManager);
            worldRenderer = new WorldRenderer(world, inputManager.Commander.Faction);
            minimap = new Minimap(worldRenderer);
        }

        public Minimap MinimapRenderer
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
    }
}
