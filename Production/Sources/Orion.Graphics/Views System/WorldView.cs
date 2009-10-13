using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK.Math;

using Orion.GameLogic;
using Orion.Geometry;
using Orion.Graphics.Widgets;

using Color = System.Drawing.Color;
using Orion.Commandment;

namespace Orion.Graphics
{
    /// <summary>
    /// A <see cref="View"/> which displays the game <see cref="World"/>.
    /// </summary>
    public sealed class WorldView : ClippedView
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
        /// <param name="renderer">The <see cref="WorldRenderer"/> to be used to draw the game <see cref="World"/></param>
        /// <param name="selection">The selection manager bound to be rendered by this view</param>
        public WorldView(Rectangle frame, WorldRenderer renderer, SelectionManager selection)
            : base(frame)
        {
            Argument.EnsureNotNull(renderer, "renderer");
            Argument.EnsureNotNull(selection, "selection");

            worldRenderer = renderer;
            selectionRenderer = new SelectionRenderer(selection);
        }
        #endregion
		
		#region Properties
		
		/// <summary>
		/// Accesses the bounds of the world being drawn.
		/// </summary>
		public override Rectangle FullBounds
		{
			get
			{
				return worldRenderer.WorldBounds;
			}
		}

		#endregion

        #region Methods
		
		#region Events Handling
		
		/// <summary>
		/// Zooms in or out when the user scrolls using the mouse wheel.
		/// </summary>
		/// <param name="args">A <see cref="MouseEventArgs"/></param>
		/// <returns>A <see cref="System.Boolean"/></returns>
		protected override bool OnMouseWheel (MouseEventArgs args)
		{
			Zoom(1 + args.WheelDelta / -600.0, args.Position);
			base.OnMouseWheel(args);
			return false;
		}

		
		#endregion
		
        /// <summary>
        /// Draws the main game view. 
        /// </summary>
        protected override void Draw()
        {
            worldRenderer.DrawTerrain(context, Bounds);
            selectionRenderer.DrawBelowUnits(context);
            worldRenderer.DrawResources(context, Bounds);
            worldRenderer.DrawEntities(context, Bounds);
            selectionRenderer.DrawAboveUnits(context);
        }
        #endregion
    }
}
