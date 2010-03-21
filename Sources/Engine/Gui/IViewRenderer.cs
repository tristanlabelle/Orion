using System;
using Orion.Engine.Graphics;
using Orion.Engine.Geometry;

namespace Orion.Engine.Gui
{
    /// <summary>
    /// Allows to render the content of a view.
    /// </summary>
    public interface IViewRenderer
    {
        /// <summary>
        /// Draws the content of a view.
        /// </summary>
        /// <param name="graphicsContext">The graphics context to be used.</param>
        /// <param name="bounds">The bounds within which to draw.</param>
        void Draw(GraphicsContext graphicsContext, Rectangle bounds);
    }
}
