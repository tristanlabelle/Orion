using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Graphics;
using Orion.GameLogic;
using Orion.Geometry;

namespace Orion.Graphics.Renderers
{
    /// <summary>
    /// Responsible for drawing the resource nodes on-screen.
    /// </summary>
    public sealed class ResourcesRenderer : IRenderer
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

        public void DrawMiniature(GraphicsContext graphicsContext, Rectangle viewBounds)
        {
            DrawClipped(graphicsContext, viewBounds, DrawMiniatureUnclipped);
        }

        private void DrawClipped(GraphicsContext graphicsContext, Rectangle viewBounds,
            Action<GraphicsContext, ResourceNode> drawDelegate)
        {
            Argument.EnsureNotNull(graphicsContext, "graphicsContext");

            Rectangle clippingBounds = viewBounds;
            foreach (Entity entity in World.Entities)
            {
                ResourceNode resourceNode = entity as ResourceNode;
                if (resourceNode == null) continue;

                Rectangle boundingRectangle = entity.BoundingRectangle;
                if (!Rectangle.Intersects(clippingBounds, boundingRectangle)
                    || !faction.HasPartiallySeen(resourceNode.GridRegion))
                    continue;

                drawDelegate(graphicsContext, resourceNode);
            }
        }

        private void DrawUnclipped(GraphicsContext graphicsContext, ResourceNode resourceNode)
        {
            string resourceTypeName = resourceNode.Type.ToStringInvariant();
            Texture texture = gameGraphics.GetMiscTexture(resourceTypeName);
            graphicsContext.Fill(resourceNode.BoundingRectangle, texture);
        }

        private void DrawMiniatureUnclipped(GraphicsContext graphicsContext, ResourceNode resourceNode)
        {
            graphicsContext.FillColor = GetResourceColor(resourceNode.Type);
            graphicsContext.Fill(resourceNode.BoundingRectangle);
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
