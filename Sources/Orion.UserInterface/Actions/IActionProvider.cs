using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.UserInterface.Actions
{
    /// <summary>
    /// Responsible of managing the <see cref="ActionButtons"/> available in a context.
    /// </summary>
    public interface IActionProvider : IDisposable
    {
        /// <summary>
        /// Gets an <see cref="ActionButton"/> at a given position.
        /// </summary>
        /// <param name="point">The position of the <see cref="ActionButton"/> to be returned.</param>
        /// <returns>The <see cref="ActionButton"/> at that position.</returns>
        /// <remarks>
        /// This <see cref="IActionProvider"/> still owns the returned <see cref="ActionButton"/>.
        /// </remarks>
        ActionButton GetButtonAt(Point point);
    }
}
