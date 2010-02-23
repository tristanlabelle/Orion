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
        private readonly TextureManager textureManager;
        #endregion

        #region Constructors
        public ResourcesRenderer(Faction faction, TextureManager textureManager)
        {
            Argument.EnsureNotNull(faction, "faction");
            Argument.EnsureNotNull(textureManager, "textureManager");

            this.faction = faction;
            this.textureManager = textureManager;
        }
        #endregion

        #region Properties
        public World World
        {
            get { return faction.World; }
        }
        #endregion

        #region Methods
        public void Draw(GraphicsContext graphics)
        {
            DrawClipped(graphics, DrawUnclipped);
        }

        public void DrawMiniature(GraphicsContext graphics)
        {
            DrawClipped(graphics, DrawMiniatureUnclipped);
        }

        private void DrawClipped(GraphicsContext graphics, Action<GraphicsContext, ResourceNode> drawDelegate)
        {
            Argument.EnsureNotNull(graphics, "graphics");

            Rectangle clippingBounds = graphics.CoordinateSystem;
            foreach (Entity entity in World.Entities)
            {
                ResourceNode resourceNode = entity as ResourceNode;
                if (resourceNode == null) continue;

                Rectangle boundingRectangle = entity.BoundingRectangle;
                if (!Rectangle.Intersects(clippingBounds, boundingRectangle)
                    || !faction.HasPartiallySeen(resourceNode.GridRegion))
                    continue;

                drawDelegate(graphics, resourceNode);
            }
        }

        private void DrawUnclipped(GraphicsContext graphics, ResourceNode resourceNode)
        {
            string resourceTypeName = resourceNode.Type.ToStringInvariant();
            Texture texture = textureManager.Get(resourceTypeName);
            graphics.Fill(resourceNode.BoundingRectangle, texture);
        }

        private void DrawMiniatureUnclipped(GraphicsContext graphics, ResourceNode resourceNode)
        {
            graphics.FillColor = GetResourceColor(resourceNode.Type);
            graphics.Fill(resourceNode.BoundingRectangle);
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
