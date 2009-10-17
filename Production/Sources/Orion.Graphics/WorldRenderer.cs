using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK.Math;

using Orion.GameLogic;
using Orion.Geometry;

using Color = System.Drawing.Color;
using Orion.GameLogic.Tasks;

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
        public void DrawTerrain(GraphicsContext graphics, Rectangle viewRectangle)
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
        public void DrawEntities(GraphicsContext graphics, Rectangle viewRectangle)
        {
            Argument.EnsureNotNull(graphics, "graphics");

            var paths = world.Units.Select(unit => unit.Task)
                .OfType<Move>()
                .Select(task => task.Path)
                .Where(path => path != null);

            graphics.StrokeColor = Color.Lime;
            foreach (Path path in paths)
            {
                graphics.Stroke(path.Points.Select(p => new Vector2(p.X, p.Y)));
            }

            foreach (Unit unit in world.Units)
            {
                if (Intersection.Test(viewRectangle, unit.Circle))
                {
                    if (unit.Faction == null) graphics.StrokeColor = Color.White;
                    else graphics.StrokeColor = unit.Faction.Color;

                    graphics.Stroke(unit.Circle);
                }
            }
        }

        public void DrawResources(GraphicsContext graphics, Rectangle viewRectangle)
        {
            foreach (ResourceNode node in world.ResourceNodes)
            {
                if (Intersection.Test(viewRectangle, node.Circle))
                {
                    if (node.ResourceType == ResourceType.Alladium)
                        graphics.FillColor = Color.LightBlue;
                    else if (node.ResourceType == ResourceType.Allagene)
                        graphics.FillColor = Color.Green;
                    else
                        continue;

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
