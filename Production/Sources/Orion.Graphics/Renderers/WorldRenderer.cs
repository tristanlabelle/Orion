﻿using System;
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
        public WorldRenderer(World world, Faction faction)
        {
            Argument.EnsureNotNull(world, "world");

            this.world = world;
            this.textureManager = new TextureManager(@"../../../Assets");
            this.terrainRenderer = new TerrainRenderer(world.Terrain);
            this.unitRenderer = new UnitsRenderer(world, faction, textureManager);
            this.fogOfWarRenderer = new FogOfWarRenderer(faction);
            ;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the bounds of the world.
        /// </summary>
        public Rectangle WorldBounds
        {
            get
            {
                return world.Bounds;
            }
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
            Texture texture;
            foreach (ResourceNode node in resourceNodes)
            {
                if (node.Type == ResourceType.Aladdium)
                    texture = textureManager.GetTexture("Aladdium");
                else if (node.Type == ResourceType.Alagene)
                    texture = textureManager.GetTexture("Alagene");
                else throw new Exception("Ressource Type Unknown");

                graphics.Fill(node.BoundingRectangle, texture);
            }
        }

        public void Dispose()
        {
            terrainRenderer.Dispose();
        }
        #endregion
    }
}
