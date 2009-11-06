using System;

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
        Intensity,

        /// <summary>
        /// Specifies that the texture uses three bytes per pixel to store
        /// red, green and blue components.
        /// </summary>
        Rgb,

        /// <summary>
        /// Specifies that the texture uses four bytes per pixel to store
        /// red, green, blue, and transparency components.
        /// </summary>
        Rgba
    }
}
