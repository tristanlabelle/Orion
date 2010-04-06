using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Geometry;

namespace Orion.Game.Presentation.Renderers
{
    /// <summary>
    /// Draws the match in the main view and the minimap.
    /// </summary>
    public interface IMatchRenderer : IDisposable
    {
        #region Properties
        /// <summary>
        /// Gets the underlying world renderer.
        /// </summary>
        WorldRenderer WorldRenderer { get; }
        #endregion

        #region Methods
        /// <summary>
        /// Draws the main view of the match, using full-scale textures.
        /// </summary>
        /// <param name="visibleBounds">A rectangle representing the area of the world that is currently visible.</param>
        void Draw(Rectangle visibleBounds);

        /// <summary>
        /// Draws a minimap view of the match.
        /// </summary>
        /// <param name="graphics">The graphics to be used.</param>
        void DrawMinimap();
        #endregion
    }
}
