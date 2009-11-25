using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Orion.Graphics
{
    /// <summary>
    /// Provides access to a 2D pixel surface's raw data via a pointer.
    /// </summary>
    [Serializable]
    public sealed class RawPixelSurface : IPixelSurface
    {
        #region Fields
        private readonly Size size;
        private readonly PixelFormat pixelFormat;
        private readonly IntPtr dataPointer;
        private readonly int stride;
        private readonly Access allowedAccess;
        #endregion

        #region Constructors
        public RawPixelSurface(Size size, PixelFormat pixelFormat, IntPtr dataPointer, int stride, Access allowedAccess)
        {
            Argument.EnsureDefined(pixelFormat, "pixelFormat");
            if (dataPointer == IntPtr.Zero) throw new ArgumentNullException("dataPointer");
            if (stride < size.Width * pixelFormat.GetBytesPerPixel())
                throw new ArgumentException("The image's stride is too small its data.", "stride");
            Argument.EnsureDefined(allowedAccess, "allowedAccess");

            this.size = size;
            this.pixelFormat = pixelFormat;
            this.dataPointer = dataPointer;
            this.stride = stride;
            this.allowedAccess = allowedAccess;
        }
        #endregion

        #region Properties
        public Size Size
        {
            get { return size; }
        }

        public PixelFormat PixelFormat
        {
            get { return pixelFormat; }
        }

        public IntPtr DataPointer
        {
            get { return dataPointer; }
        }

        public int Stride
        {
            get { return stride; }
        }

        public Access AllowedAccess
        {
            get { return allowedAccess; }
        }
        #endregion

        #region Methods
        public void Lock(Region region, Access access, Action<RawPixelSurface> accessor)
        {
            if (region.ExclusiveMax.X >= size.Width || region.ExclusiveMax.Y >= size.Height)
                throw new ArgumentException("The accessed region exceeds the image's bounds.", "region");
            Argument.EnsureDefined(access, "access");
            Argument.EnsureNotNull(accessor, "accessor");

            if ((access & AllowedAccess) != access) throw new ArgumentException("Illegal access specified.", "access");
            if (access == Access.None) return;

            int bytesPerPixel = pixelFormat.GetBytesPerPixel();
            int stride = size.Width * bytesPerPixel;
            long pointer = (long)dataPointer + region.Min.Y * stride + region.Min.X * bytesPerPixel;
            RawPixelSurface rawImage = new RawPixelSurface(region.Size, pixelFormat, (IntPtr)pointer, stride, access);
            accessor(rawImage);
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
    }
}
