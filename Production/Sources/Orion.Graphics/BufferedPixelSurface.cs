using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using SysImage = System.Drawing.Image;
using SysPixelFormat = System.Drawing.Imaging.PixelFormat;
using System.Drawing.Imaging;

namespace Orion.Graphics
{
    /// <summary>
    /// A 2D pixel surface which's pixels are buffered in a byte array.
    /// </summary>
    public sealed class BufferedPixelSurface : IPixelSurface
    {
        #region Instance
        #region Fields
        private readonly Size size;
        private readonly PixelFormat pixelFormat;
        private readonly ArraySegment<byte> data;
        private readonly int stride;
        private readonly Access allowedAccess;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="BufferedImage"/> from its dimensions and <see cref="PixelFormat"/>.
        /// </summary>
        /// <param name="size">The dimensions of the surface.</param>
        /// <param name="pixelFormat">The format of the pixels of the surface.</param>
        public BufferedPixelSurface(Size size, PixelFormat pixelFormat)
        {
            Argument.EnsureStrictlyPositive(size.Area, "size.Area");
            Argument.EnsureDefined(pixelFormat, "pixelFormat");

            this.size = size;
            this.pixelFormat = pixelFormat;
            this.data = new ArraySegment<byte>(new byte[size.Area * pixelFormat.GetBytesPerPixel()]);
            this.stride = data.Count;
            this.allowedAccess = Access.ReadWrite;
        }

        public BufferedPixelSurface(Size size, PixelFormat pixelFormat,
            ArraySegment<byte> data, int stride, Access allowedAccess)
        {
            Argument.EnsureStrictlyPositive(size.Area, "size.Area");
            Argument.EnsureDefined(pixelFormat, "pixelFormat");
            Argument.EnsureNotNull(data.Array, "data.Array");
            Argument.EnsureDefined(allowedAccess, "allowedAccess");
            if (data.Count < size.Area * pixelFormat.GetBytesPerPixel())
                throw new ArgumentException("Data buffer is too small for the surface.", "data");
            if (stride < size.Width * pixelFormat.GetBytesPerPixel())
                throw new ArgumentException("Stride is too small for the surface.", "stride");

            this.size = size;
            this.pixelFormat = pixelFormat;
            this.data = data;
            this.stride = stride;
            this.allowedAccess = allowedAccess;
        }

        public BufferedPixelSurface(Size size, PixelFormat pixelFormat,
            ArraySegment<byte> data, Access allowedAccess)
            : this(size, pixelFormat, data, size.Width * pixelFormat.GetBytesPerPixel(), allowedAccess) { }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the size of this surface, in pixels.
        /// </summary>
        public Size Size
        {
            get { return size; }
        }

        /// <summary>
        /// Gets the format of the pixels of this surface.
        /// </summary>
        public PixelFormat PixelFormat
        {
            get { return pixelFormat; }
        }

        /// <summary>
        /// Gets the buffer in which this surface's raw pixel data is stored.
        /// </summary>
        public ArraySegment<byte> Data
        {
            get { return data; }
        }

        /// <summary>
        /// Gets the type of access to this surface's data which is allowed.
        /// </summary>
        public Access AllowedAccess
        {
            get { return Access.ReadWrite; }
        }
        #endregion

        #region Methods
        public void Lock(Region region, Access access, Action<RawPixelSurface> accessor)
        {
            if (region.ExclusiveMax.X > size.Width || region.ExclusiveMax.Y > size.Height)
                throw new ArgumentException("The accessed region exceeds the image's bounds.", "region");
            Argument.EnsureDefined(access, "access");
            Argument.EnsureNotNull(accessor, "accessor");

            if (access == Access.None) return;
            if ((access & allowedAccess) != access) throw new ArgumentException("Illegal access requested.", "access");

            GCHandle pinningHandle = GCHandle.Alloc(data.Array, GCHandleType.Pinned);
            try
            {
                long pointer = (long)pinningHandle.AddrOfPinnedObject() + data.Offset
                    + region.MinY * stride
                    + region.MinX * pixelFormat.GetBytesPerPixel();
                RawPixelSurface rawImage = new RawPixelSurface(region.Size, pixelFormat, (IntPtr)pointer, stride, access);
                accessor(rawImage);
            }
            finally
            {
                pinningHandle.Free();
            }
        }

        public void Lock(Access access, Action<RawPixelSurface> accessor)
        {
            Lock((Region)Size, access, accessor);
        }

        public void LockToOverwrite(Region region, Action<RawPixelSurface> accessor)
        {
            Lock(region, Access.Write, accessor);
        }
        #endregion

        #region Explicit Members
        void IDisposable.Dispose() { }
        #endregion
        #endregion

        #region Static
        #region Methods
        public static BufferedPixelSurface FromDrawingImage(SysImage image)
        {
            Argument.EnsureNotNull(image, "image");

            if (!IsSupportedPixelFormat(image.PixelFormat))
                throw new NotSupportedException("Image pixel format unsupported.");

            Bitmap bitmap = image as Bitmap;
            if (bitmap == null) bitmap = new Bitmap(image);
            try
            {
                bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
                try
                {
                    BitmapData bitmapData = bitmap.LockBits(
                        new Rectangle(0, 0, image.Width, image.Height),
                        ImageLockMode.ReadOnly, bitmap.PixelFormat);
                    try
                    {
                        return FromBitmapData(bitmapData);
                    }
                    finally
                    {
                        bitmap.UnlockBits(bitmapData);
                    }
                }
                finally
                {
                    bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
                }
            }
            finally
            {
                if (bitmap != image)
                    bitmap.Dispose();
            }
        }


        private static bool IsSupportedPixelFormat(SysPixelFormat pixelFormat)
        {
            return pixelFormat == SysPixelFormat.Alpha
                || pixelFormat == SysPixelFormat.Format24bppRgb
                || pixelFormat == SysPixelFormat.Format32bppArgb;
        }

        private static BufferedPixelSurface FromBitmapData(BitmapData bitmapData)
        {
            if (bitmapData.PixelFormat == SysPixelFormat.Alpha)
                return FromAlphaBitmapData(bitmapData);
            if (bitmapData.PixelFormat == SysPixelFormat.Format24bppRgb)
                return FromBgrBitmapData(bitmapData);
            if (bitmapData.PixelFormat == SysPixelFormat.Format32bppArgb)
                return FromBgraBitmapData(bitmapData);
            throw new NotSupportedException("Image pixel data unsupported.");
        }

        private static BufferedPixelSurface FromAlphaBitmapData(BitmapData bitmapData)
        {
            BufferedPixelSurface image = new BufferedPixelSurface(new Size(bitmapData.Width, bitmapData.Height), PixelFormat.Alpha);
            
            int rowLength = image.Size.Width * image.PixelFormat.GetBytesPerPixel();
            for (int rowIndex = 0; rowIndex < bitmapData.Height; ++rowIndex)
            {
                long rowPointer = (long)bitmapData.Scan0 + rowIndex * bitmapData.Stride;
                Marshal.Copy((IntPtr)rowPointer, image.Data.Array, image.Data.Offset + rowIndex * rowLength, rowLength);
            }
            
            return image;
        }

        private static BufferedPixelSurface FromBgrBitmapData(BitmapData bitmapData)
        {
            BufferedPixelSurface image = new BufferedPixelSurface(new Size(bitmapData.Width, bitmapData.Height), PixelFormat.Rgb);
            
            int bytesPerPixel = image.PixelFormat.GetBytesPerPixel();
            int rowLength = image.Size.Width * bytesPerPixel;
            for (int rowIndex = 0; rowIndex < bitmapData.Height; ++rowIndex)
            {
                long rowPointer = (long)bitmapData.Scan0 + rowIndex * bitmapData.Stride;
                Marshal.Copy((IntPtr)rowPointer, image.Data.Array, image.Data.Offset + rowIndex * rowLength, rowLength);
            }

            // BGR -> RGB
            for (int i = image.Data.Offset; i < image.Data.Count; i += bytesPerPixel)
            {
                byte blue = image.Data.Array[i];
                image.Data.Array[i] = image.Data.Array[i + 2];
                image.Data.Array[i + 2] = blue;
            }

            return image;
        }

        private static BufferedPixelSurface FromBgraBitmapData(BitmapData bitmapData)
        {
            BufferedPixelSurface image = new BufferedPixelSurface(new Size(bitmapData.Width, bitmapData.Height), PixelFormat.Rgba);

            int bytesPerPixel = image.PixelFormat.GetBytesPerPixel();
            int rowLength = image.Size.Width * bytesPerPixel;
            for (int rowIndex = 0; rowIndex < bitmapData.Height; ++rowIndex)
            {
                long rowPointer = (long)bitmapData.Scan0 + rowIndex * bitmapData.Stride;
                Marshal.Copy((IntPtr)rowPointer, image.Data.Array, image.Data.Offset + rowIndex * rowLength, rowLength);
            }

            // BGRA -> RGBA
            for (int i = image.Data.Offset; i < image.Data.Count; i += bytesPerPixel)
            {
                byte blue = image.Data.Array[i];
                image.Data.Array[i] = image.Data.Array[i + 2];
                image.Data.Array[i + 2] = blue;
            }

            return image;
        }
        #endregion
        #endregion
    }
}
