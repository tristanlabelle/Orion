using System;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Engine.Graphics;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Components;
using System.Diagnostics;
using System.Linq;

namespace Orion.Game.Presentation.Renderers
{
    /// <summary>
    /// Responsible for drawing the resource nodes on-screen.
    /// </summary>
    public sealed class ResourcesRenderer
    {
        #region Fields
        private static readonly ColorRgb miniatureAladdiumColor = Colors.Green;
        private static readonly ColorRgb miniatureAlageneColor = Colors.LightCyan;

        private readonly Faction faction;
        private readonly GameGraphics gameGraphics;
        #endregion

        #region Constructors
        public ResourcesRenderer(Faction faction, GameGraphics gameGraphics)
        {
            Argument.EnsureNotNull(faction, "faction");
            Argument.EnsureNotNull(gameGraphics, "gameGraphics");

            this.faction = faction;
            this.gameGraphics = gameGraphics;
        }
        #endregion

        #region Properties
        public World World
        {
            get { return faction.World; }
        }
        #endregion

        #region Methods
        public void Draw(GraphicsContext graphicsContext, Rectangle viewBounds)
        {
            DrawClipped(graphicsContext, viewBounds, DrawUnclipped);
        }

        public void DrawMiniature(GraphicsContext graphicsContext)
        {
            DrawClipped(graphicsContext, World.Bounds, DrawMiniatureUnclipped);
        }

        private void DrawClipped(GraphicsContext graphicsContext, Rectangle viewBounds,
            Action<GraphicsContext, Entity> drawDelegate)
        {
            Argument.EnsureNotNull(graphicsContext, "graphicsContext");

            Rectangle clippingBounds = viewBounds;
            foreach (Entity entity in World.Entities)
            {
                Spatial spatial = entity.Spatial;
                if (spatial == null
                    || !entity.Components.Has<Harvestable>()
                    || !Rectangle.Intersects(clippingBounds, spatial.BoundingRectangle)
                    || !faction.HasPartiallySeen(spatial.GridRegion))
                    continue;

                drawDelegate(graphicsContext, entity);
            }
        }

        private void DrawUnclipped(GraphicsContext graphicsContext, Entity resourceNode)
        {
            Harvestable harvestable = resourceNode.Components.Get<Harvestable>();
            Spatial spatial = resourceNode.Spatial;

            string resourceTypeName = harvestable.Type.ToStringInvariant();
            Texture texture = gameGraphics.GetMiscTexture(resourceTypeName);

            graphicsContext.Fill(spatial.BoundingRectangle, texture);
        }

        private void DrawMiniatureUnclipped(GraphicsContext graphicsContext, Entity resourceNode)
        {
            Harvestable harvestData = resourceNode.Components.Get<Harvestable>();
            Spatial spatial = resourceNode.Spatial;

            ColorRgb color = GetResourceColor(harvestData.Type);

            graphicsContext.Fill(spatial.BoundingRectangle, color);
        }

        public static ColorRgb GetResourceColor(ResourceType type)
        {
            if (type == ResourceType.Aladdium) return miniatureAladdiumColor;
            else if (type == ResourceType.Alagene) return miniatureAlageneColor;
            else throw new Exception("Ressource type unknown.");
        }
        #endregion
    }
}
