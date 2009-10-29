﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK.Math;

using Orion.GameLogic;
using Orion.GameLogic.Tasks;
using Orion.GameLogic.Pathfinding;
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
        private readonly World world;
        private readonly TerrainRenderer terrainRenderer;
        private readonly UnitRenderer unitRenderer;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="WorldRenderer"/> from the <see cref="World"/> it is going to render.
        /// </summary>
        /// <param name="world">The <see cref="World"/> to be rendered.</param>
        public WorldRenderer(World world)
        {
            Argument.EnsureNotNull(world, "world");

            this.world = world;
            this.terrainRenderer = new TerrainRenderer(world.Terrain);
            this.unitRenderer = new UnitRenderer(world);
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

        /// <summary>
        /// Draws the <see cref="World"/>'s entities, including <see cref="Unit"/>s.
        /// </summary>
        /// <param name="graphics">The <see cref="GraphicsContext"/> in which to draw.</param>
        /// <param name="viewRectangle">
        /// A <see cref="Rectangle"/>, in world units, which specifies the parts of the
        /// <see cref="World"/> which have to be drawn.
        /// </param>
        public void DrawEntities(GraphicsContext graphics)
        {
            Argument.EnsureNotNull(graphics, "graphics");

            unitRenderer.Draw(graphics);
        }

        public void DrawResources(GraphicsContext graphics)
        {
            Rectangle bounds = graphics.CoordinateSystem;
            foreach (ResourceNode node in world.ResourceNodes)
            {
                if (Intersection.Test(bounds, node.Circle))
                {
                    if (node.ResourceType == ResourceType.Alladium)
                        graphics.FillColor = Color.LightBlue;
                    else if (node.ResourceType == ResourceType.Alagene)
                        graphics.FillColor = Color.Green;
                    else continue;

                    graphics.Fill(node.Circle);
                }
            }
        }

        public void Dispose()
        {
            terrainRenderer.Dispose();
        }
        #endregion
    }
}