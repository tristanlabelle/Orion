using System;
using System.ComponentModel;

namespace Orion.Graphics
{
    /// <summary>
    /// Describes the format of a texture in memory.
    /// </summary>
    [Serializable]
    public enum PixelFormat
    {
        /// <summary>
        /// Specifies that the texture uses one byte per pixel storing
        /// a luminance value.
        /// </summary>
        Intensity,

        /// <summary>
        /// Specifies that the texture uses one byte per pixel storing
        /// a transparency value.
        /// </summary>
        Alpha,

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

    /// <summary>
    /// Provides extensions to the <see cref="PixelFormat"/> enumeration.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class PixelFormatExtensions
    {
        #region Methods
        /// <summary>
        /// Determines if a <see cref="PixelFormat"/> has an alpha channel.
        /// </summary>
        /// <param name="pixelFormat">The <see cref="PixelFormat"/> to be tested.</param>
        /// <returns>
        /// <c>True</c> if the <see cref="PixelFormat"/> specifies an alpha channel, <c>false</c> if it does not.
        /// </returns>
        public static bool HasAlphaChannel(this PixelFormat pixelFormat)
        {
            return pixelFormat == PixelFormat.Alpha
                || pixelFormat == PixelFormat.Rgba;
        }

        /// <summary>
        /// Gets the size of a pixel in a given <see cref="PixelFormat"/>, in bytes.
        /// </summary>
        /// <param name="pixelFormat">The <see cref="PixelFormat"/>.</param>
        /// <returns>The size, in bytes, of each pixel in that <see cref="PixelFormat"/>.</returns>
        public static int GetBytesPerPixel(this PixelFormat pixelFormat)
        {
            if (pixelFormat == PixelFormat.Intensity) return 1;
            if (pixelFormat == PixelFormat.Alpha) return 1;
            if (pixelFormat == PixelFormat.Rgb) return 3;
            if (pixelFormat == PixelFormat.Rgba) return 4;
            throw new InvalidEnumArgumentException("pixelFormat", (int)pixelFormat, typeof(PixelFormat));
        }
        #endregion
    }
}
