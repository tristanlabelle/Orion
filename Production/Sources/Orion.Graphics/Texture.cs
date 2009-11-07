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
        internal Texture(TextureBuilder builder)
        {
            Argument.EnsureNotNull(builder, "builder");

            int lastID;
            GL.GetInteger(GetPName.Texture2D, out lastID);

            this.id = GL.GenTexture();
            this.width = builder.Width;
            this.height = builder.Height;
            this.format = builder.Format;

            try
            {
                GL.BindTexture(TextureTarget.Texture2D, id);

                if (builder.PixelData != null) ValidatePixelBufferSize(builder.PixelData, width * height, format);

                GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
                GL.TexImage2D(TextureTarget.Texture2D, 0, GetGLInternalPixelFormat(this.format),
                    this.width, this.height, 0, GetGLPixelFormat(this.format), PixelType.UnsignedByte,
                    builder.PixelData);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                    (int)(builder.UseSmoothing ? TextureMinFilter.Linear : TextureMinFilter.Nearest));
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                    (int)(builder.UseSmoothing ? TextureMagFilter.Linear : TextureMagFilter.Nearest));

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
                    (int)(builder.Repeats ? TextureWrapMode.Repeat : TextureWrapMode.ClampToEdge));
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
                    (int)(builder.Repeats ? TextureWrapMode.Repeat : TextureWrapMode.ClampToEdge));

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

        public void SetPixels(Region region, byte[] data)
        {
            EnsureNotDisposed();

            if (region.MinX < 0 || region.MinY < 0
                || region.Width < 0 || region.Height < 0
                || region.ExclusiveMaxX > width || region.ExclusiveMaxY > height)
                throw new ArgumentException("Invalid pixel region.");
            Argument.EnsureNotNull(data, "data");

            ValidatePixelBufferSize(data, region.Area, format);

            int lastID;
            GL.GetInteger(GetPName.Texture2D, out lastID);

            try
            {
                GL.BindTexture(TextureTarget.Texture2D, id);

                GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
                GL.TexSubImage2D(TextureTarget.Texture2D, 0,
                    region.MinX, region.MinY, region.Width, region.Height,
                    GetGLPixelFormat(format), PixelType.UnsignedByte,
                    data);
            }
            finally
            {
                GL.BindTexture(TextureTarget.Texture2D, lastID);
            }
        }

        public void SetPixels(byte[] data)
        {
            Region region = new Region(0, 0, (short)width, (short)height);
            SetPixels(region, data);
        }
        #endregion

        #region Properties
        public bool HasAlphaChannel
        {
            get { return format == TextureFormat.Rgba || format == TextureFormat.Alpha; }
        }

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

        private static void ValidatePixelBufferSize(byte[] buffer, int pixelCount, TextureFormat format)
        {
            Argument.EnsureNotNull(buffer, "buffer");
            if (buffer.Length < (pixelCount * GetBytesPerPixel(format)))
                throw new ArgumentException("Pixel data buffer too small!", "data");
        }

        private static int GetBytesPerPixel(TextureFormat format)
        {
            if (format == TextureFormat.Intensity) return 1;
            if (format == TextureFormat.Alpha) return 1;
            if (format == TextureFormat.Rgb) return 3;
            if (format == TextureFormat.Rgba) return 4;
            throw new InvalidEnumArgumentException(
                "Invalid texture format: {0}.".FormatInvariant(format));
        }

        private static PixelFormat GetGLPixelFormat(TextureFormat format)
        {
            if (format == TextureFormat.Intensity) return PixelFormat.Luminance;
            if (format == TextureFormat.Alpha) return PixelFormat.Alpha;
            if (format == TextureFormat.Rgb) return PixelFormat.Rgb;
            if (format == TextureFormat.Rgba) return PixelFormat.Rgba;
            throw new InvalidEnumArgumentException(
                "Invalid texture format: {0}.".FormatInvariant(format));
        }

        private static PixelInternalFormat GetGLInternalPixelFormat(TextureFormat format)
        {
            if (format == TextureFormat.Intensity) return PixelInternalFormat.Luminance;
            if (format == TextureFormat.Alpha) return PixelInternalFormat.Alpha;
            if (format == TextureFormat.Rgb) return PixelInternalFormat.Rgb;
            if (format == TextureFormat.Rgba) return PixelInternalFormat.Rgba;
            throw new InvalidEnumArgumentException(
                "Invalid texture format: {0}.".FormatInvariant(format));
        }
        #endregion
    }
}
