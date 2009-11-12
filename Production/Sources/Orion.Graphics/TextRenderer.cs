using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using OpenTK;
using OpenTK.Math;
using Rectangle = Orion.Geometry.Rectangle;
using SysRectangle = System.Drawing.Rectangle;
using SysGraphics = System.Drawing.Graphics;

namespace Orion.Graphics
{
    public sealed class TextRenderer
    {
        #region Fields
        private readonly Texture texture;
        #endregion

        #region Constructors
        public TextRenderer()
        {
            using (Bitmap fontImage = new Bitmap(128, 128, PixelFormat.Format32bppArgb))
            {
                using (SysGraphics graphics = SysGraphics.FromImage(fontImage))
                {
                    using (Font font = new Font("Calibri", 16, GraphicsUnit.Pixel))
                    {
                        Vector2 position = Vector2.Zero;
                        float maxHeight = 0;
                        for (char character = 'A'; character < 'Z'; ++character)
                        {
                            string characterString = character.ToString();
                            SizeF characterSizeF = graphics.MeasureString(characterString, font);
                            if (characterSizeF.Height > maxHeight) maxHeight = characterSizeF.Height;
                            if (position.X + characterSizeF.Width > fontImage.Width)
                            {
                                position.X = 0;
                                position.Y += font.Height;
                            }

                            graphics.DrawString(characterString, font, Brushes.White, position.X, position.Y);
                            position.X += characterSizeF.Width;
                        }
                    }

                    graphics.Flush(System.Drawing.Drawing2D.FlushIntention.Flush);
                }

                fontImage.RotateFlip(RotateFlipType.RotateNoneFlipY);

                const int pixelSizeInBytes = 4;
                byte[] pixelData = new byte[fontImage.Width * fontImage.Height * pixelSizeInBytes];
                BitmapData fontBitmapData = fontImage.LockBits(
                    new SysRectangle(0, 0, fontImage.Width, fontImage.Height),
                    ImageLockMode.ReadOnly, fontImage.PixelFormat);
                try
                {
                    int rowSizeInBytes = fontImage.Width * pixelSizeInBytes;
                    for (int rowIndex = 0; rowIndex < fontImage.Height; ++rowIndex)
                    {
                        IntPtr firstRowPixelPtr = (IntPtr)((long)fontBitmapData.Scan0 + rowIndex * fontBitmapData.Stride);
                        Marshal.Copy(firstRowPixelPtr, pixelData, rowIndex * rowSizeInBytes, rowSizeInBytes);
                    }
                }
                finally
                {
                    fontImage.UnlockBits(fontBitmapData);
                }

                // XRGB -> RGBA
                for (int rowIndex = 0; rowIndex < fontImage.Height; ++rowIndex)
                {
                    for (int columnIndex = 0; columnIndex < fontImage.Width; ++columnIndex)
                    {
                        int pixelIndex = rowIndex * fontBitmapData.Width + rowIndex;
                        int pixelDataOffset = pixelIndex * pixelSizeInBytes;

                        byte alpha = pixelData[pixelDataOffset + 3];
                        pixelData[pixelDataOffset] = pixelData[pixelDataOffset + 1];
                        pixelData[pixelDataOffset + 1] = pixelData[pixelDataOffset + 2];
                        pixelData[pixelDataOffset + 2] = pixelData[pixelDataOffset + 3];
                        pixelData[pixelDataOffset + 3] = alpha;
                    }
                }

                texture = new TextureBuilder
                {
                    Width = fontImage.Width,
                    Height = fontImage.Height,
                    Format = TextureFormat.Rgba,
                    PixelData = pixelData,
                    UseSmoothing = true
                }.Build();
            }
        }
        #endregion

        #region Properties
        
        #endregion

        #region Methods
        public void DrawString(GraphicsContext graphics)
        {
            graphics.Fill(new Rectangle(0, 0, 10, 10), texture);
        }

        private static string GetStringFromCharacterRange(char first, char last)
        {
            if (first > last) return GetStringFromCharacterRange(last, first);

            char[] characters = new char[last - first + 1];
            for (int i = 0; i < characters.Length; ++i)
                characters[i] = (char)(first + i);
            return new string(characters);
        }
        #endregion
    }
}
