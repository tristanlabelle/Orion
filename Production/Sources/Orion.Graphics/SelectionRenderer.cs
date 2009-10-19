using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK.Math;

using Orion.GameLogic;
using Orion.Geometry;
using Orion.Commandment;
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
        /// <summary>
        /// Initializes a new <see cref="SelectionRenderer"/> from a
        /// <see cref="SelectionManager"/> providing information
        /// on the current selection.
        /// </summary>
        /// <param name="selectionManager">
        /// The <see cref="SelectionManager"/> which provides selection information.
        /// </param>
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
        public void DrawSelectionMarkers(GraphicsContext graphics)
        {
            Argument.EnsureNotNull(graphics, "graphics");

            graphics.StrokeStyle = StrokeStyle.Solid;
            graphics.StrokeColor = selectionMarkerColor;
            foreach (Unit unit in selectionManager.SelectedUnits)
                graphics.Stroke(new Circle(unit.Position, 1));
        }

        /// <summary>
        /// Draws the selection markers over the <see cref="Unit"/>s.
        /// </summary>
        /// <param name="graphics">The <see cref="GraphicsContext"/> to be used for rendering.</param>
        public void DrawSelectionRectangle(GraphicsContext graphics)
        {
            Argument.EnsureNotNull(graphics, "graphics");

            if (selectionManager.IsSelecting)
            {
                graphics.StrokeStyle = StrokeStyle.Solid;
                graphics.StrokeColor = selectionRectangleStrokeColor;
                graphics.Stroke(selectionManager.SelectionRectangle.Value);
                graphics.FillColor = selectionRectangleFillColor;
                graphics.Fill(selectionManager.SelectionRectangle.Value);
            }
        }

        public void DrawHealthBars(GraphicsContext graphics)
        {
            const float healthBarLength = 1;

            foreach (Unit unit in selectionManager.SelectedUnits)
            {
                Circle circle = unit.Circle;

                Vector2 healthBarCenter = circle.Center + Vector2.UnitY * (circle.Radius + 0.5f);
                Vector2 healthBarStart = healthBarCenter - Vector2.UnitX * healthBarLength * 0.5f;
                Vector2 healthBarEnd = healthBarStart + Vector2.UnitX * healthBarLength;

                float healthRatio = unit.Health / unit.Type.MaxHealth;
                Vector2 healthBarLevelPosition = healthBarStart + Vector2.UnitX * healthRatio * healthBarLength;

                graphics.StrokeColor = Color.Lime;
                graphics.StrokeLineStrip(healthBarStart, healthBarLevelPosition);
                graphics.StrokeColor = Color.Red;
                graphics.StrokeLineStrip(healthBarLevelPosition, healthBarEnd);
            }
        }
        #endregion
        #endregion

        #region Static
        #region Fields
        private static readonly Color selectionMarkerColor = Color.FromArgb(51, 153, 255);
        private static readonly Color selectionRectangleStrokeColor = Color.FromArgb(51, 153, 255);
        private static readonly Color selectionRectangleFillColor = Color.FromArgb(100, 51, 153, 255);
        #endregion
        #endregion
    }
}
