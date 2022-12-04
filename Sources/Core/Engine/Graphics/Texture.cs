using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Runtime.InteropServices;
using OpenTK.Graphics;
using GLPixelFormat = OpenTK.Graphics.PixelFormat;
using SysImage = System.Drawing.Image;

namespace Orion.Engine.Graphics
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
        private bool hasMipmaps = true;
        private int id;
        #endregion

        #region Constructors
        internal Texture(Size size, PixelFormat pixelFormat, IntPtr dataPointer)
        {
            Argument.EnsureStrictlyPositive(size.Area, "size.Area");
            Argument.EnsureDefined(pixelFormat, "pixelFormat");

            Debug.Assert(PowerOfTwo.Is(size.Width) && PowerOfTwo.Is(size.Height));

            this.size = size;
            this.pixelFormat = pixelFormat;
            this.id = GL.GenTexture();

            try
            {
                BindWhile(() =>
                {
                    try
                    {
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, 1);
                    }
                    catch (GraphicsException)
                    {
                        Debug.WriteLine("Automatic mipmap generation not supported and disabled.");
                        hasMipmaps = false;
                    }

                    GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

                    PixelInternalFormat glInternalFormat = GetGLInternalPixelFormat(this.pixelFormat);
                    OpenTK.Graphics.PixelFormat glPixelFormat = GetGLPixelFormat(this.pixelFormat);
                    GL.TexImage2D(TextureTarget.Texture2D, 0, glInternalFormat,
                        size.Width, size.Height, 0, glPixelFormat, PixelType.UnsignedByte,
                        dataPointer);

#if false
                    if (!hasMipmaps)
                    {
                        // "Manually" generate mipmaps
                        Glu.Build2DMipmap(TextureTarget.Texture2D, (int)glInternalFormat,
                            size.Width, size.Height, glPixelFormat, PixelType.UnsignedByte,
                            dataPointer);
                        hasMipmaps = true;
                    }
#endif

                    GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode,
                        (int)TextureEnvMode.Modulate);
                });
            }
            catch
            {
                GL.DeleteTexture(id);
                throw;
            }

            Interlocked.Increment(ref aliveCount);
        }

        internal Texture(Size size, PixelFormat pixelFormat)
            : this(size, pixelFormat, IntPtr.Zero) { }
        #endregion

        #region Properties
        public int Width
        {
            get { return size.Width; }
        }

        public int Height
        {
            get { return size.Height; }
        }

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
            finally
            {
                GL.BindTexture(TextureTarget.Texture2D, lastID);
            }
        }

        private void SetParameter(TextureParameterName name, int value)
        {
            BindWhile(() => { GL.TexParameter(TextureTarget.Texture2D, name, value); });
        }

        private void SetEnv(TextureEnvParameter param, int value)
        {
            BindWhile(() => { GL.TexEnv(TextureEnvTarget.TextureEnv, param, value); });
        }

        public void SetSmooth(bool on)
        {
            BindWhile(() =>
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                    (int)(on
                        ? (hasMipmaps ? TextureMinFilter.LinearMipmapNearest : TextureMinFilter.Linear)
                        : TextureMinFilter.Nearest));
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                    (int)(on ? TextureMagFilter.Linear : TextureMagFilter.Nearest));
            });
        }

        public void SetRepeat(bool on)
        {
            try
            {
                SetWrapMode(on ? TextureWrapMode.Repeat : TextureWrapMode.ClampToEdge);
            }
            catch (GraphicsException)
            {
                Debug.WriteLine("Clamp to edge wrap mode not supported, defaulting to clamp."
                    + " Some artifacts may occur around the edges of the textures.");
                SetWrapMode(on ? TextureWrapMode.Repeat : TextureWrapMode.Clamp);
            }
        }

        private void SetWrapMode(TextureWrapMode wrapMode)
        {
            SetParameter(TextureParameterName.TextureWrapS, (int)wrapMode);
            SetParameter(TextureParameterName.TextureWrapT, (int)wrapMode);
        }
        #endregion

        #region Blitting
        public void Blit(Region region, byte[] pixelData)
        {
            EnsureNotDisposed();

            if (region.ExclusiveMax.X > size.Width || region.ExclusiveMax.Y > size.Height)
                throw new ArgumentException("Invalid pixel region.");
            Argument.EnsureNotNull(pixelData, "data");

            ValidatePixelBufferSize(pixelData, region.Area, pixelFormat);

            int lastID;
            GL.GetInteger(GetPName.Texture2D, out lastID);

            try
            {
                GL.BindTexture(TextureTarget.Texture2D, id);

                GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
                GL.TexSubImage2D(TextureTarget.Texture2D, 0,
                    region.MinX, region.MinY, region.Width, region.Height,
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

        #region Locking
        public void Lock(Region region, Access access, Action<RawPixelSurface> accessor)
        {
            Lock(region, access, false, accessor);
        }

        public void LockToOverwrite(Region region, Action<RawPixelSurface> accessor)
        {
            Lock(region, Access.Write, true, accessor);
        }

        private void Lock(Region region, Access access, bool discard, Action<RawPixelSurface> accessor)
        {
            if (region.ExclusiveMax.X > size.Width || region.ExclusiveMax.Y > size.Height)
                throw new ArgumentException("Invalid texture region.", "region");
            Argument.EnsureDefined(access, "access");
            Argument.EnsureNotNull(accessor, "accessor");

            if (access == Access.None) return;
            byte[] data = new byte[size.Area * pixelFormat.GetBytesPerPixel()];

            BindWhile(() =>
            {
                if ((access & Access.Read) == Access.Read)
                {
                    GL.PixelStore(PixelStoreParameter.PackAlignment, 1);
                    GL.GetTexImage(TextureTarget.Texture2D, 0, GetGLPixelFormat(pixelFormat), PixelType.UnsignedByte, data);
                }

                BufferedPixelSurface surface = new BufferedPixelSurface(region.Size, pixelFormat,
                    new ArraySegment<byte>(data), access);
                surface.Lock(region, access, accessor);

                if ((access & Access.Write) == Access.Write)
                {
                    GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
                    throw new NotImplementedException("Texture locking to write.");
                }
            });
        }
        #endregion

        #region Object Model
        public void Dispose()
        {
            EnsureNotDisposed();
            GL.DeleteTexture(id);
            id = 0;
            Interlocked.Decrement(ref aliveCount);
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
            get { return Access.ReadWrite; }
        }
        #endregion
        #endregion

        #region Static
        #region Fields
        private static int aliveCount;
        #endregion

        #region Properties
        public static int AliveCount
        {
            get { return aliveCount; }
        }
        #endregion

        #region Methods
        public static Texture CreateBlank(Size size, PixelFormat pixelFormat, bool smooth, bool repeat)
        {
            Texture texture = new Texture(size, pixelFormat);
            texture.SetSmooth(smooth);
            texture.SetRepeat(repeat);
            return texture;
        }

        /// <summary>
        /// Creates a texture with a checkerboard pattern of a given size.
        /// </summary>
        /// <param name="size">The size of the texture to be created.</param>
        /// <param name="firstColor">The first color of the checkerboard pattern.</param>
        /// <param name="secondColor">The second color of the checkerboard pattern.</param>
        /// <returns>The newly created checkerboard texture.</returns>
        public static Texture CreateCheckerboard(Size size, ColorRgb firstColor, ColorRgb secondColor)
        {
            using (BufferedPixelSurface surface = BufferedPixelSurface.CreateCheckerboard(size, firstColor, secondColor))
                return FromPixelSurface(surface, false, true);
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

        public static Texture FromStream(Stream stream, bool smooth, bool repeat)
        {
            Argument.EnsureNotNull(stream, "stream");

            try
            {
                using (SysImage image = SysImage.FromStream(stream))
                {
                    return FromDrawingImage(image, smooth, repeat);
                }
            }
            catch (OutOfMemoryException e)
            {
                // System.Drawing.Image.FromFile throws an OutOfMemoryException when it fails to decode an image.
                throw new IOException(e.Message, e);
            }
        }

        public static Texture FromFile(string filePath, bool smooth, bool repeat)
        {
            Argument.EnsureNotNull(filePath, "filePath");

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                return FromStream(stream, smooth, repeat);
        }

        public static Texture FromDrawingImage(SysImage image, bool smooth, bool repeat)
        {
            Argument.EnsureNotNull(image, "image");

            IPixelSurface surface = BufferedPixelSurface.FromImage(image);
            return Texture.FromPixelSurface(surface, smooth, repeat);
        }
        #endregion
        #endregion
    }
}
