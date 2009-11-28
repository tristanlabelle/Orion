
using OpenTK.Math;
using Orion.Commandment;
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
        private readonly UserInputManager userInputManager;
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
        public SelectionRenderer(UserInputManager manager)
        {
            Argument.EnsureNotNull(manager, "manager");
            userInputManager = manager;
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
            foreach (Unit unit in userInputManager.SelectionManager.SelectedUnits)
            {
                graphics.Stroke(unit.BoundingRectangle);

                if (unit.Faction == userInputManager.Commander.Faction)
                {
                    if (unit.RallyPoint != null && (unit.RallyPoint.Value - unit.Position).Length > 1)
                    {
                        graphics.StrokeLineStrip(unit.Center, unit.RallyPoint.Value);
                    }
                }
            }
        }

        /// <summary>
        /// Draws the selection markers over the <see cref="Unit"/>s.
        /// </summary>
        /// <param name="graphics">The <see cref="GraphicsContext"/> to be used for rendering.</param>
        public void DrawSelectionRectangle(GraphicsContext graphics)
        {
            Argument.EnsureNotNull(graphics, "graphics");

            if (userInputManager.SelectionRectangle.HasValue)
            {
                Rectangle selectionRectangle = userInputManager.SelectionRectangle.Value;
                graphics.StrokeStyle = StrokeStyle.Solid;
                graphics.StrokeColor = selectionRectangleStrokeColor;
                graphics.Stroke(selectionRectangle);
                graphics.FillColor = selectionRectangleFillColor;
                graphics.Fill(selectionRectangle);
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
