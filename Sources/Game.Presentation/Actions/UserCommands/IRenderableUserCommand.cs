using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Graphics;
using Orion.Engine.Geometry;

namespace Orion.Game.Presentation.Actions.UserCommands
{
    /// <summary>
    /// Provides the interface that allows a <see cref="UserCommand"/> to draw visual cues on-screen.
    /// </summary>
    public interface IRenderableUserCommand
    {
        /// <summary>
        /// Draws the content of a view.
        /// </summary>
        /// <param name="graphicsContext">The graphics context to be used.</param>
        /// <param name="bounds">The bounds within which to draw.</param>
        void Draw(GraphicsContext graphicsContext, Rectangle bounds);
    }
}
