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
    /// A <see cref="View"/> which displays the game <see cref="World"/>.
    /// </summary>
    public sealed class WorldView : View
    {
        #region Fields
        private readonly WorldRenderer worldRenderer;
        private readonly SelectionRenderer selectionRenderer;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructs the main game view. 
        /// </summary>
        /// <param name="frame">
        /// The <see cref="Rectangle"/> frame of the view (normally the full OpenGL control size).
        /// </param>
        /// <param name="renderer">The <see cref="WorldRenderer"/> to be used to draw the game <see cref="World"/>.</param>
        public WorldView(Rectangle frame, WorldRenderer renderer, SelectionManager selection)
            : base(frame)
        {
            Argument.EnsureNotNull(renderer, "renderer");
            Argument.EnsureNotNull(selection, "selection");

            worldRenderer = renderer;
            selectionRenderer = new SelectionRenderer(selection);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Draws the main game view. 
        /// </summary>
        protected override void Draw()
        {
            worldRenderer.DrawTerrain(context, Bounds);
            worldRenderer.DrawEntities(context, Bounds);
            selectionRenderer.DrawAboveUnits(context);
            selectionRenderer.DrawBelowUnits(context);
        }
        #endregion
    }
}
