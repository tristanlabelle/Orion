using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Graphics
{
    /// <summary>
    /// Describes the format of a texture in memory.
    /// </summary>
    [Serializable]
    public enum TextureFormat
    {
        /// <summary>
        /// Specifies that the texture uses one byte per pixel storing
        /// a luminance value.
        /// </summary>
        Luminance,

        /// <summary>
        /// Specifies that the texture uses three bytes per pixel to store
        /// red, green and blue components.
        /// </summary>
        Color,

        /// <summary>
        /// Specifies that the texture uses four bytes per pixel to store
        /// red, green, blue, and transparency components.
        /// </summary>
        ColorAndAlpha
    }
}
