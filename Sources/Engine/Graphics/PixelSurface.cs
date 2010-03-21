using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using SysPixelFormat = System.Drawing.Imaging.PixelFormat;
using SysGraphics = System.Drawing.Graphics;

namespace Orion.Engine.Graphics
{
    /// <summary>
    /// Provides utility methods to deal with <see cref="IPixelSurface"/>s.
    /// </summary>
    public static class PixelSurface
    {
        #region Methods
        #region Extension Methods
        public static void Lock(this IPixelSurface surface, Access access, Action<RawPixelSurface> accessor)
        {
            Argument.EnsureNotNull(surface, "surface");
            surface.Lock((Region)surface.Size, access, accessor);
        }

        public static void LockToOverwrite(this IPixelSurface surface, Action<RawPixelSurface> accessor)
        {
            Argument.EnsureNotNull(surface, "surface");
            surface.LockToOverwrite((Region)surface.Size, accessor);
        }
        #endregion

        #region SaveToFile
        public static void SaveToFile(IPixelSurface surface, string filePath)
        {
            Argument.EnsureNotNull(surface, "surface");
            surface.Lock((Region)surface.Size, Access.Read, rawSurface => SaveToFile(rawSurface, filePath));
        }

        public static void SaveToFile(RawPixelSurface surface, string filePath)
        {
            Argument.EnsureNotNull(surface, "surface");

            using (Bitmap bitmap = new Bitmap(surface.Size.Width, surface.Size.Height, SysPixelFormat.Format32bppArgb))
            {
                using (SysGraphics graphics = SysGraphics.FromImage(bitmap))
                    graphics.Clear(Color.Black);

                Rectangle lockingRectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                BitmapData data = bitmap.LockBits(lockingRectangle, ImageLockMode.WriteOnly, bitmap.PixelFormat);
                try
                {
                    int sourceBpp = surface.PixelFormat.GetBytesPerPixel();
                    for (int pixelIndex = 0; pixelIndex < surface.Size.Area; ++pixelIndex)
                    {
                        for (int pixelComponentIndex = 0; pixelComponentIndex < sourceBpp; ++pixelComponentIndex)
                        {
                            long sourcePointer = (long)surface.DataPointer + pixelIndex * sourceBpp + pixelComponentIndex;
                            byte value = Marshal.ReadByte((IntPtr)sourcePointer);
                            long destinationPointer = (long)data.Scan0 + pixelIndex * 4 + pixelComponentIndex;
                            Marshal.WriteByte((IntPtr)destinationPointer, value);
                        }
                    }
                }
                finally
                {
                    bitmap.UnlockBits(data);
                }

                bitmap.Save(filePath);
            }
        }
        #endregion
        #endregion
    }
}
