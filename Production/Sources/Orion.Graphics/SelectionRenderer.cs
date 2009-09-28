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
    /// Draws selection-related information on-screen.
    /// </summary>
    public sealed class SelectionRenderer
    {
        #region Instance
        #region Fields
        private readonly SelectionManager selectionManager;
        #endregion

        #region Constructors
        public SelectionRenderer(SelectionManager selectionManager)
        {
            Argument.EnsureNotNull(selectionManager, "selectionManager");
            this.selectionManager = selectionManager;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Draws the selection markers under the <see cref="Unit"/>s.
        /// </summary>
        /// <param name="graphics">The <see cref="GraphicsContext"/> to be used for rendering.</param>
        public void DrawBelowUnits(GraphicsContext graphics)
        {
            Argument.EnsureNotNull(graphics, "graphics");

            graphics.StrokeColor = selectionMarkerColor;
            foreach (Unit unit in selectionManager.SelectedUnits)
                graphics.Stroke(new Circle(unit.Position, 1.5f));
        }

        /// <summary>
        /// Draws the selection markers over the <see cref="Unit"/>s.
        /// </summary>
        /// <param name="graphics">The <see cref="GraphicsContext"/> to be used for rendering.</param>
        public void DrawAboveUnits(GraphicsContext graphics)
        {
            Argument.EnsureNotNull(graphics, "graphics");

            if (selectionManager.IsSelecting)
            {
                graphics.StrokeColor = selectionRectangleStrokeColor;
                graphics.FillColor = selectionRectangleFillColor;

                graphics.Fill(selectionManager.SelectionRectangle.Value);
                graphics.Stroke(selectionManager.SelectionRectangle.Value);
            }
        }
        #endregion
        #endregion

        #region Static
        #region Fields
        private static readonly Color selectionMarkerColor = Color.FromArgb(51, 153, 255);
        private static readonly Color selectionRectangleStrokeColor = Color.FromArgb(51, 153, 255);
        private static readonly Color selectionRectangleFillColor = Color.FromArgb(100, 168, 202, 236);
        #endregion
        #endregion
    }
}
