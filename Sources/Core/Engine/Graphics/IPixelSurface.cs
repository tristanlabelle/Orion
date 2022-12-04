using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Graphics
{
    /// <summary>
    /// Basic interface for objects representing 2D grids of pixels.
    /// </summary>
    public interface IPixelSurface : IDisposable
    {
        #region Properties
        /// <summary>
        /// Gets the size of this surface, in pixels.
        /// </summary>
        Size Size { get; }

        /// <summary>
        /// Gets the format in which this surface's pixels are stored.
        /// </summary>
        PixelFormat PixelFormat { get; }

        /// <summary>
        /// Gets the type of access allowed to this surface.
        /// </summary>
        Access AllowedAccess { get; }
        #endregion

        #region Methods
        /// <summary>
        /// Locks this surface's pixels, allowing access to its raw data.
        /// </summary>
        /// <param name="region">The region of the surface to be accessed.</param>
        /// <param name="access">The type of access required to the surface.</param>
        /// <param name="accessor">A delegate to a method which accesses the surface's pixels.</param>
        void Lock(Region region, Access access, Action<RawPixelSurface> accessor);

        /// <summary>
        /// Locks this surface's pixels, allowing write access to its raw data.
        /// The caller garantees that the whole accessed region will be overwritten.
        /// </summary>
        /// <param name="region">The region of the surface to be accessed.</param>
        /// <param name="accessor">A delegate to a method which accesses the surface's pixels.</param>
        void LockToOverwrite(Region region, Action<RawPixelSurface> accessor);
        #endregion
    }
}
