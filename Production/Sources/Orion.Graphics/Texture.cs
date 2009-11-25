using System;
using System.ComponentModel;
using OpenTK.Graphics;
using GLPixelFormat = OpenTK.Graphics.PixelFormat;
using System.Runtime.InteropServices;
using SysImage = System.Drawing.Image;

namespace Orion.Graphics
{
    /// <summary>
    /// Provides a representation of an image stored in the video memory.
    /// </summary>
    public sealed class Texture : IPixelSurface
    {
        #region Instance
        #region Fields
        private readonly Size size;
        private readonly PixelFormat pixelFormat;
        private int id;
        #endregion

        #region Constructors
        internal Texture(Size size, PixelFormat pixelFormat, IntPtr dataPointer)
        {
            Argument.EnsureStrictlyPositive(size.Area, "size.Area");
            Argument.EnsureDefined(pixelFormat, "pixelFormat");

            this.size = size;
            this.pixelFormat = pixelFormat;
            this.id = GL.GenTexture();

            try
            {
                BindWhile(() =>
                {
                    GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
                    GL.TexImage2D(TextureTarget.Texture2D, 0, GetGLInternalPixelFormat(this.pixelFormat),
                        size.Width, size.Height, 0, GetGLPixelFormat(this.pixelFormat), PixelType.UnsignedByte,
                        dataPointer);

                    GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode,
                        (int)TextureEnvMode.Modulate);
                });
            }
            catch
            {
                GL.DeleteTexture(id);
                throw;
            }
        }

        internal Texture(Size size, PixelFormat pixelFormat)
            : this(size, pixelFormat, IntPtr.Zero) { }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the size of this texture, in pixels.
        /// </summary>
        public Size Size
        {
            get { return size; }
        }

        /// <summary>
        /// Gets the format in which the pixels of this texture are stored.
        /// </summary>
        public PixelFormat PixelFormat
        {
            get { return pixelFormat; }
        }

        /// <summary>
        /// Gets a value indicating if this texture has been disposed.
        /// </summary>
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
        #region OpenGL Accessors
        /// <summary>
        /// Binds this <see cref="Texture"/> for the duration of an action
        /// before reverting to the last bound texture.
        /// </summary>
        /// <param name="action">The action to be performed on the bound texture.</param>
        internal void BindWhile(Action action)
        {
            Argument.EnsureNotNull(action, "action");

            int lastID;
            GL.GetInteger(GetPName.Texture2D, out lastID);

            try
            {
                GL.BindTexture(TextureTarget.Texture2D, id);

                action();
            }
            catch
            {
                GL.BindTexture(TextureTarget.Texture2D, lastID);
                throw;
            }
        }

        internal void SetParameter(TextureParameterName name, int value)
        {
            BindWhile(() => { GL.TexParameter(TextureTarget.Texture2D, name, value); });
        }

        internal void SetEnv(TextureEnvParameter param, int value)
        {
            BindWhile(() => { GL.TexEnv(TextureEnvTarget.TextureEnv, param, value); });
        }

        internal void SetSmooth(bool on)
        {
            SetParameter(TextureParameterName.TextureMinFilter,
                (int)(on ? TextureMinFilter.Linear : TextureMinFilter.Nearest));
            SetParameter(TextureParameterName.TextureMagFilter,
                (int)(on ? TextureMagFilter.Linear : TextureMagFilter.Nearest));
        }

        internal void SetRepeat(bool on)
        {
            SetParameter(TextureParameterName.TextureWrapS,
                (int)(on ? TextureWrapMode.Repeat : TextureWrapMode.ClampToEdge));
            SetParameter(TextureParameterName.TextureWrapT,
                (int)(on ? TextureWrapMode.Repeat : TextureWrapMode.ClampToEdge));
        }
        #endregion

        #region Blitting
        public void Blit(Region region, byte[] pixelData)
        {
            EnsureNotDisposed();

            if (region.ExclusiveMax.X > size.Width || region.ExclusiveMax.Y > size.Height)
                throw new ArgumentException("Invalid pixel region.");
            Argument.EnsureNotNull(pixelData, "data");

            ValidatePixelBufferSize(pixelData, region.Size.Area, pixelFormat);

            int lastID;
            GL.GetInteger(GetPName.Texture2D, out lastID);

            try
            {
                GL.BindTexture(TextureTarget.Texture2D, id);

                GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
                GL.TexSubImage2D(TextureTarget.Texture2D, 0,
                    region.Min.X, region.Min.Y, region.Size.Width, region.Size.Height,
                    GetGLPixelFormat(pixelFormat), PixelType.UnsignedByte,
                    pixelData);
            }
            finally
            {
                GL.BindTexture(TextureTarget.Texture2D, lastID);
            }
        }

        public void Blit(byte[] pixelData)
        {
            Blit((Region)Size, pixelData);
        }
        #endregion

        #region Object Model
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

        private static void ValidatePixelBufferSize(byte[] buffer, int pixelCount, PixelFormat format)
        {
            Argument.EnsureNotNull(buffer, "buffer");
            if (buffer.Length < (pixelCount * format.GetBytesPerPixel()))
                throw new ArgumentException("Pixel data buffer too small!", "data");
        }

        private static GLPixelFormat GetGLPixelFormat(PixelFormat format)
        {
            if (format == PixelFormat.Intensity) return GLPixelFormat.Luminance;
            if (format == PixelFormat.Alpha) return GLPixelFormat.Alpha;
            if (format == PixelFormat.Rgb) return GLPixelFormat.Rgb;
            if (format == PixelFormat.Rgba) return GLPixelFormat.Rgba;
            throw new InvalidEnumArgumentException(
                "Invalid texture format: {0}.".FormatInvariant(format));
        }

        private static PixelInternalFormat GetGLInternalPixelFormat(PixelFormat format)
        {
            if (format == PixelFormat.Intensity) return PixelInternalFormat.Luminance;
            if (format == PixelFormat.Alpha) return PixelInternalFormat.Alpha;
            if (format == PixelFormat.Rgb) return PixelInternalFormat.Rgb;
            if (format == PixelFormat.Rgba) return PixelInternalFormat.Rgba;
            throw new InvalidEnumArgumentException(
                "Invalid texture format: {0}.".FormatInvariant(format));
        }
        #endregion

        #region Explicit Members
        Access IPixelSurface.AllowedAccess
        {
            get { return Access.None; } // Although that could be implemented.
        }

        void IPixelSurface.Lock(Region region, Access access, Action<RawPixelSurface> accessor)
        {
            throw new NotImplementedException();
        }

        void IPixelSurface.LockToOverwrite(Region region, Action<RawPixelSurface> accessor)
        {
            throw new NotImplementedException();
        }
        #endregion
        #endregion

        #region Static
        #region Methods
        public static Texture CreateBlank(Size size, PixelFormat pixelFormat, bool smooth, bool repeat)
        {
            Texture texture = new Texture(size, pixelFormat);
            texture.SetSmooth(smooth);
            texture.SetRepeat(repeat);
            return texture;
        }

        public static Texture FromPixelSurface(IPixelSurface surface, bool smooth, bool repeat)
        {
            Argument.EnsureNotNull(surface, "surface");

            Texture texture = null;
            surface.Lock((Region)surface.Size, Access.Read, rawImage =>
                {
                    texture = new Texture(rawImage.Size, rawImage.PixelFormat, rawImage.DataPointer);
                });

            texture.SetSmooth(smooth);
            texture.SetRepeat(repeat);

            return texture;
        }

        public static Texture FromBuffer(Size size, PixelFormat pixelFormat,
            byte[] data, bool smooth, bool repeat)
        {
            Argument.EnsureNotNull(data, "data");

            Texture texture = null;

            GCHandle pinningHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                texture = new Texture(size, pixelFormat, pinningHandle.AddrOfPinnedObject());
            }
            finally
            {
                pinningHandle.Free();
            }

            texture.SetSmooth(smooth);
            texture.SetRepeat(repeat);

            return texture;
        }

        public static Texture FromFile(string filePath, bool smooth, bool repeat)
        {
            Argument.EnsureNotNull(filePath, "filePath");

            using (SysImage image = SysImage.FromFile(filePath))
            {
                return FromDrawingImage(image, smooth, repeat);
            }
        }

        public static Texture FromDrawingImage(SysImage image, bool smooth, bool repeat)
        {
            Argument.EnsureNotNull(image, "image");

            IPixelSurface surface = BufferedPixelSurface.FromDrawingImage(image);
            return Texture.FromPixelSurface(surface, smooth, repeat);
        }
        #endregion
        #endregion
    }
}
