using System;
using System.Linq;
using Orion.GameLogic;
using Orion.Geometry;

using Color = System.Drawing.Color;

namespace Orion.Graphics
{
    /// <summary>
    /// Provides methods to draw the game <see cref="World"/>.
    /// </summary>
    public sealed class WorldRenderer : IDisposable
    {
        #region Fields
        private static readonly Color AladdiumColor = Color.LightBlue;
        private static readonly Color AlageneColor = Color.Green;

        private readonly World world;
        private readonly TerrainRenderer terrainRenderer;
        private readonly UnitsRenderer unitRenderer;
        private readonly FogOfWarRenderer fogOfWarRenderer;
        private TextureManager textureManager;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="WorldRenderer"/> from the <see cref="World"/> it is going to render.
        /// </summary>
        /// <param name="world">The <see cref="World"/> to be rendered.</param>
        public WorldRenderer(World world, Faction faction, TextureManager textureManager)
        {
            Argument.EnsureNotNull(world, "world");

            this.world = world;
            this.textureManager = textureManager;
            this.terrainRenderer = new TerrainRenderer(world.Terrain, textureManager);
            this.unitRenderer = new UnitsRenderer(world, faction, textureManager);
            this.fogOfWarRenderer = new FogOfWarRenderer(faction);
        }
        #endregion

        #region Properties
        public Rectangle WorldBounds
        {
            get { return world.Bounds; }
        }

        public UnitsRenderer UnitRenderer
        {
            get { return unitRenderer; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Draws the <see cref="World"/>'s terrain.
        /// </summary>
        /// <param name="graphics">The <see cref="GraphicsContext"/> in which to draw.</param>
        /// <param name="viewRectangle">
        /// A <see cref="Rectangle"/>, in world units, which specifies the parts of the
        /// <see cref="World"/> which have to be drawn.
        /// </param>
        public void DrawTerrain(GraphicsContext graphics)
        {
            Argument.EnsureNotNull(graphics, "graphics");
            terrainRenderer.Draw(graphics);
        }

        public void DrawMiniatureTerrain(GraphicsContext graphics)
        {
            Argument.EnsureNotNull(graphics, "graphics");
            terrainRenderer.DrawMiniature(graphics);
        }

        public void DrawFogOfWar(GraphicsContext graphics)
        {
            Argument.EnsureNotNull(graphics, "graphics");
            fogOfWarRenderer.Draw(graphics);
        }

        /// <summary>
        /// Draws the <see cref="World"/>'s entities, including <see cref="Unit"/>s.
        /// </summary>
        /// <param name="graphics">The <see cref="GraphicsContext"/> in which to draw.</param>
        /// <param name="viewRectangle">
        /// A <see cref="Rectangle"/>, in world units, which specifies the parts of the
        /// <see cref="World"/> which have to be drawn.
        /// </param>
        public void DrawUnits(GraphicsContext graphics)
        {
            Argument.EnsureNotNull(graphics, "graphics");

            unitRenderer.Draw(graphics);
        }

        public void DrawResources(GraphicsContext graphics)
        {
            Argument.EnsureNotNull(graphics, "graphics");

            Rectangle bounds = graphics.CoordinateSystem;
            var resourceNodes = world.Entities
                .OfType<ResourceNode>()
                .Where(node => Rectangle.Intersects(bounds, node.BoundingRectangle));
            foreach (ResourceNode node in resourceNodes)
            {
                string resourceTypeName = node.Type.ToStringInvariant();
                Texture texture = textureManager.Get(resourceTypeName);
                graphics.Fill(node.BoundingRectangle, texture);
            }
        }

        public void DrawMiniatureResources(GraphicsContext graphics)
        {
            Argument.EnsureNotNull(graphics, "graphics");

            Rectangle bounds = graphics.CoordinateSystem;
            var resourceNodes = world.Entities
                .OfType<ResourceNode>()
                .Where(node => Rectangle.Intersects(bounds, node.BoundingRectangle));
            foreach (ResourceNode node in resourceNodes)
            {
                graphics.FillColor = GetResourceColor(node.Type);
                graphics.Fill(node.BoundingRectangle);
            }
        }

        public static Color GetResourceColor(ResourceType type)
        {
            if (type == ResourceType.Aladdium) return Color.Green;
            else if (type == ResourceType.Alagene) return Color.LightCyan;
            else throw new Exception("Ressource type unknown.");
        }

        public void Dispose()
        {
            terrainRenderer.Dispose();
            fogOfWarRenderer.Dispose();
        }
        #endregion
    }
}
