using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK.Math;

using Orion.GameLogic;
using Orion.Geometry;
using Orion.Core;

using Color = System.Drawing.Color;

namespace Orion.Graphics
{
    /// <summary>
    /// Provides methods to draw the game <see cref="World"/>.
    /// </summary>
    public sealed class WorldRenderer
    {
        #region Fields
        private readonly World world;
        private GameMap map;
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
            map = MapGenerator.GenerateNewMap(world.Width, world.Height, new MersenneTwister());
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

            /*
            // Later, walkable and non-walkable tiles should be distinguishable.
            Rectangle? rectangle = world.Bounds; //.Intersection(viewRectangle);
            if (rectangle.HasValue)
            {
                graphics.FillColor = Color.Gray;
                graphics.Fill(rectangle.Value);
            }
            */

            for (int i = 0; i < world.Width; i++)
            {
                for (int j = 0; j < world.Height; j++)
                {
                    Rectangle rectangle = new Rectangle(i, j, 1, 1);
                    if (map[i, j])
                    {
                        graphics.FillColor = Color.White;
                    }
                    else
                    {
                        graphics.FillColor = Color.Black;
                    }
                    graphics.Fill(rectangle);
                }
            }
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
            // Later, walkable and non-walkable tiles should be distinguishable.
            foreach (Unit unit in world.Units)
            {
                if (!unit.IsAlive)
                    continue;
                if (viewRectangle.ContainsPoint(unit.Position))
                {
                    if (unit.Faction == null) graphics.StrokeColor = Color.White;
                    else graphics.StrokeColor = unit.Faction.Color;

                    Circle circle = new Circle(unit.Position, 1);
                    graphics.Stroke(circle);
                }
            }

            //Renders Ressource Nodes to the game world
            foreach (RessourceNode node in world.RessourceNodes)
            {
                if (viewRectangle.ContainsPoint(node.Position))
                {
                    if (node.RessourceType == RessourceType.Alladium)
                        graphics.FillColor = Color.LightBlue;
                    else if (node.RessourceType == RessourceType.Allagene)
                        graphics.FillColor = Color.Green;
                    else
                        continue;

                    graphics.Fill(node.Circle);       
                }
            }
        }
        #endregion
    }
}
