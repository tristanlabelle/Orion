using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK.Math;

using Orion.GameLogic;
using Orion.Geometry;
using Orion.Graphics.Widgets;

using Color = System.Drawing.Color;

namespace Orion.Graphics
{
    /// <summary>
    /// A <see cref="View"/> which displays the game <see cref="Terrain"/>.
    /// </summary>
    public sealed class TerrainView : View
    {
        #region Fields
        private World world;
        #endregion

        #region Constructor
		/// <summary>
		/// Constructs the main game view. 
		/// </summary>
		/// <param name="frame">
		/// The <see cref="Rectangle"/> frame of the view (normally the full OpenGL control size).
		/// </param>
        public TerrainView(Rectangle frame)
            : base(frame)
        {
            world = new World();

            for (uint i = 0; i < 3; i++)
            {
                world.Units.Add(new Unit(i, new UnitType("mcwarrior"), world));
            }
            float count = 0;
            foreach (Unit unit in world.Units)
            {
                unit.Position = new Vector2(count * 10, count * 10);
                count++;
            }

            Frame panel = new Frame(new Rectangle(50, 50, 100, 100));
            Children.Add(panel);
        }
        #endregion

        #region Methods
		/// <summary>
		/// Draws the main game view. 
		/// </summary>
        protected override void Draw()
        {
			//throw new NotImplementedException();
        }
        #endregion
    }
}
