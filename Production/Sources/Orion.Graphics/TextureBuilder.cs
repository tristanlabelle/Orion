using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Graphics
{
    /// <summary>
    /// Provides a mutable intermediate form to supply texture creation parameters.
    /// </summary>
    [Serializable]
    public sealed class TextureBuilder
    {
        #region Fields
        private int width;
        private int height;
        private TextureFormat format;
        private byte[] pixelData;
        private bool useSmoothing;
        private bool repeats;
        #endregion

        #region Constructors
        public TextureBuilder()
        {
            Reset();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the width of the texture to be created.
        /// </summary>
        public int Width
        {
            get { return width; }
            set
            {
                Argument.EnsureStrictlyPositive(value, "Width");
                EnsureDimensionIsPowerOfTwo(value, "Width");
                width = value;
            }
        }

        /// <summary>
        /// Accesses the height of the texture to be created.
        /// </summary>
        public int Height
        {
            get { return height; }
            set
            {
                Argument.EnsureStrictlyPositive(value, "Height");
                EnsureDimensionIsPowerOfTwo(value, "Width");
                height = value;
            }
        }

        /// <summary>
        /// Accesses the format of this texture.
        /// </summary>
        public TextureFormat Format
        {
            get { return format; }
            set
            {
                Argument.EnsureDefined(value, "Format");
                this.format = value;
            }
        }

        /// <summary>
        /// Accesses the buffer of data from which the texture is initialized.
        /// </summary>
        public byte[] PixelData
        {
            get { return pixelData; }
            set { pixelData = value; }
        }

        /// <summary>
        /// Accesses a value indicating if the <see cref="Texture"/> to be created uses smoothing.
        /// </summary>
        public bool UseSmoothing
        {
            get { return useSmoothing; }
            set { useSmoothing = value; }
        }

        /// <summary>
        /// Accesses a value indicating if the <see cref="Texture"/> to be created
        /// should use repetition to wrap for coordinates outside the [0, 1[ range.
        /// </summary>
        public bool Repeats
        {
            get { return repeats; }
            set { repeats = value; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Resets the properties of this <see cref="TextureBuilder"/> to their default values.
        /// </summary>
        public void Reset()
        {
            width = 1;
            height = 1;
            format = TextureFormat.Rgb;
            pixelData = null;
            useSmoothing = false;
            repeats = false;
        }

        /// <summary>
        /// Builds a new <see cref="Texture"/> using the parameters set here.
        /// </summary>
        /// <returns>The <see cref="Texture"/> that was created.</returns>
        public Texture Build()
        {
            return new Texture(this);
        }

        private static void EnsureDimensionIsPowerOfTwo(int value, string argumentName)
        {
            if ((value & (value - 1)) != 0)
                throw new ArgumentException("The dimensions of a texture should be powers of two.", argumentName);
        }
        #endregion
    }
}
