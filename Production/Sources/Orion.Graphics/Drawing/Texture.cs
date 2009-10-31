using System;
using System.ComponentModel;
using OpenTK.Graphics;

namespace Orion.Graphics
{
    /// <summary>
    /// Provides a representation of an image stored in the video memory.
    /// </summary>
    public sealed class Texture : IDisposable
    {
        #region Fields
        private int id;
        private readonly int width;
        private readonly int height;
        private readonly TextureFormat format;
        #endregion

        #region Constructors
        public Texture(int width, int height, TextureFormat format, byte[] data)
        {
            Argument.EnsureStrictlyPositive(width, "width");
            Argument.EnsureStrictlyPositive(height, "height");
            Argument.EnsureDefined(format, "format");

            int lastID;
            GL.GetInteger(GetPName.Texture2D, out lastID);

            this.id = GL.GenTexture();
            this.width = width;
            this.height = height;
            this.format = format;

            try
            {
                GL.BindTexture(TextureTarget.Texture2D, id);

                SetPixels(data);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                    (int)TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                    (int)TextureMagFilter.Nearest);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
                    (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
                    (int)TextureWrapMode.Repeat);

                GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode,
                    (int)TextureEnvMode.Modulate);
            }
            catch
            {
                GL.BindTexture(TextureTarget.Texture2D, lastID);
                GL.DeleteTexture(id);
                throw;
            }
        }

        public void SetPixels(byte[] data)
        {
            EnsureNotDisposed();

            PixelFormat pixelFormat;
            PixelInternalFormat pixelInternalFormat;
            int bytesPerPixel;
            GetPixelFormatInfo(this.format, out bytesPerPixel, out pixelFormat, out pixelInternalFormat);

            if (data != null && data.Length < (this.width * this.height * bytesPerPixel))
                throw new ArgumentException("Pixel data buffer too small!", "data");

            GL.TexImage2D(TextureTarget.Texture2D, 0, pixelInternalFormat,
                this.width, this.height, 0, pixelFormat, PixelType.UnsignedByte,
                data);
        }

        private static void GetPixelFormatInfo(TextureFormat format, out int bytesPerPixel,
            out PixelFormat pixelFormat, out PixelInternalFormat pixelInternalFormat)
        {
            if (format == TextureFormat.Intensity)
            {
                bytesPerPixel = 1;
                pixelFormat = PixelFormat.Luminance;
                pixelInternalFormat = PixelInternalFormat.Luminance;
            }
            else if (format == TextureFormat.Rgb)
            {
                bytesPerPixel = 3;
                pixelFormat = PixelFormat.Rgb;
                pixelInternalFormat = PixelInternalFormat.Rgb;
            }
            else if (format == TextureFormat.Rgba)
            {
                bytesPerPixel = 4;
                pixelFormat = PixelFormat.Rgba;
                pixelInternalFormat = PixelInternalFormat.Rgba;
            }
            else
            {
                throw new InvalidEnumArgumentException(
                    "Invalid texture format: {0}.".FormatInvariant(format));
            }
        }
        #endregion

        #region Properties
        public bool IsDisposed
        {
            get { return id == 0; }
        }

        /// <summary>
        /// Gets the OpenGL identifier of this texture.
        /// </summary>
        internal int ID
        {
            get
            {
                EnsureNotDisposed();
                return id;
            }
        }
        #endregion

        #region Methods
        public void Dispose()
        {
            EnsureNotDisposed();
            GL.DeleteTexture(id);
            id = 0;
        }

        private void EnsureNotDisposed()
        {
            if (IsDisposed) throw new ObjectDisposedException(null);
        }
        #endregion
    }
}
