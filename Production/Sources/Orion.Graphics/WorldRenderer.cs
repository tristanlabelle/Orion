using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK.Math;

using Orion.GameLogic;
using Orion.Geometry;

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
        }
        #endregion

        #region Properties
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

            // Later, walkable and non-walkable tiles should be distinguishable.
            Rectangle? rectangle = world.Bounds; //.Intersection(viewRectangle);
            if (rectangle.HasValue)
            {
                graphics.FillColor = Color.Gray;
                graphics.Fill(rectangle.Value);
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
            graphics.StrokeStyle = StrokeStyle.DotDash;
            // Later, walkable and non-walkable tiles should be distinguishable.
            foreach (Unit unit in world.Units)
            {
                if (viewRectangle.ContainsPoint(unit.Position))
                {
                    if (unit.Faction == null) graphics.StrokeColor = Color.White;
                    else graphics.StrokeColor = unit.Faction.Color;

                    Circle circle = new Circle(unit.Position, 1);
                    graphics.Stroke(circle);
                }
            }
        }
        #endregion
    }
}
