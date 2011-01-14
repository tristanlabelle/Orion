using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;

namespace Orion.Game.Presentation.Actions
{
    /// <summary>
    /// Responsible of managing the <see cref="ActionButtons"/> available in a context.
    /// </summary>
    public interface IActionProvider : IDisposable
    {
        /// <summary>
        /// Gets an <see cref="ActionDescriptor"/> at a given position.
        /// </summary>
        /// <param name="point">The position of the <see cref="ActionDescriptor"/> to be returned.</param>
        /// <returns>The <see cref="ActionDescriptor"/> at that position.</returns>
        /// <remarks>
        /// This <see cref="IActionProvider"/> still owns the returned <see cref="ActionDescriptor"/>.
        /// </remarks>
        ActionDescriptor GetActionAt(Point point);

        /// <summary>
        /// Recreates the <see cref="ActionDescriptor"/>s as to reflect some new context.
        /// </summary>
        void Refresh();
    }
}
