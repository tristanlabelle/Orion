using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.GameLogic;

using Color = System.Drawing.Color;
using Orion.Geometry;

namespace Orion.Graphics.Renderers
{
    /// <summary>
    /// Responsible for drawing the resource nodes on-screen.
    /// </summary>
    public sealed class ResourcesRenderer : IRenderer
    {
        #region Fields
        private static readonly Color miniatureAladdiumColor = Color.Green;
        private static readonly Color miniatureAlageneColor = Color.LightCyan;

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
        private IEnumerable<ResourceNode> GetVisibleClippedNodes(Rectangle clippingBounds)
        {
            return World.Entities
                .OfType<ResourceNode>()
                .Where(node => Rectangle.Intersects(clippingBounds, node.BoundingRectangle)
                    && faction.HasPartiallySeen(node.GridRegion));
        }

        public void Draw(GraphicsContext graphics)
        {
            Argument.EnsureNotNull(graphics, "graphics");

            var resourceNodes = GetVisibleClippedNodes(graphics.CoordinateSystem);
            foreach (ResourceNode node in resourceNodes)
            {
                string resourceTypeName = node.Type.ToStringInvariant();
                Texture texture = textureManager.Get(resourceTypeName);
                graphics.Fill(node.BoundingRectangle, texture);
            }
        }

        public void DrawMiniature(GraphicsContext graphics)
        {
            Argument.EnsureNotNull(graphics, "graphics");

            var resourceNodes = GetVisibleClippedNodes(graphics.CoordinateSystem);
            foreach (ResourceNode node in resourceNodes)
            {
                graphics.FillColor = GetResourceColor(node.Type);
                graphics.Fill(node.BoundingRectangle);
            }
        }

        public static Color GetResourceColor(ResourceType type)
        {
            if (type == ResourceType.Aladdium) return miniatureAladdiumColor;
            else if (type == ResourceType.Alagene) return miniatureAlageneColor;
            else throw new Exception("Ressource type unknown.");
        }
        #endregion
    }
}
