using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using OpenTK;
using OpenTK.Math;
using Rectangle = Orion.Geometry.Rectangle;
using SysGraphics = System.Drawing.Graphics;
using SysPixelFormat = System.Drawing.Imaging.PixelFormat;
using SysRectangle = System.Drawing.Rectangle;

namespace Orion.Graphics
{
    public sealed class TextRenderer : IDisposable
    {
        #region Fields
        private readonly Dictionary<char, Rectangle> characterTextureRectangles
            = new Dictionary<char, Rectangle>();
        private readonly Texture texture;
        #endregion

        #region Constructors
        public TextRenderer()
        {
            using (Bitmap fontImage = new Bitmap(256, 256, SysPixelFormat.Format32bppArgb))
            {
                using (SysGraphics graphics = SysGraphics.FromImage(fontImage))
                {
                    graphics.Clear(Color.Black);

                    using (Font font = new Font("Calibri", 24, FontStyle.Bold, GraphicsUnit.Pixel))
                    {
                        Vector2 position = Vector2.Zero;
                        float maxHeight = 0;
                        for (char character = ' '; character < (char)126; ++character)
                        {
                            string characterString = character.ToString();
                            SizeF characterSizeF = graphics.MeasureString(characterString, font);
                            if (characterSizeF.Height > maxHeight) maxHeight = characterSizeF.Height;
                            if (position.X + characterSizeF.Width > fontImage.Width)
                            {
                                position.X = 0;
                                position.Y += font.Height;
                            }

                            Rectangle characterTextureRectangle = new Rectangle(
                                position.X / fontImage.Width,
                                1 - position.Y / fontImage.Height - characterSizeF.Height / fontImage.Height,
                                characterSizeF.Width / fontImage.Width,
                                characterSizeF.Height / fontImage.Height);
                            characterTextureRectangles.Add(character, characterTextureRectangle);

                            if (!char.IsWhiteSpace(character)) graphics.DrawString(characterString, font, Brushes.White, position.X, position.Y);
                            position.X += characterSizeF.Width;
                        }
                    }
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

                // BGRA -> RGBA
                for (int rowIndex = 0; rowIndex < fontImage.Height; ++rowIndex)
                {
                    for (int columnIndex = 0; columnIndex < fontImage.Width; ++columnIndex)
                    {
                        int pixelIndex = rowIndex * fontImage.Width + columnIndex;
                        int pixelDataOffset = pixelIndex * pixelSizeInBytes;

                        byte blue = pixelData[pixelDataOffset];
                        pixelData[pixelDataOffset] = pixelData[pixelDataOffset + 2];
                        pixelData[pixelDataOffset + 2] = blue;
                    }
                }

                texture = Texture.FromBuffer(new Size(fontImage.Width, fontImage.Height),
                    PixelFormat.Rgba, pixelData, true, false);
            }
        }
        #endregion

        #region Properties
        
        #endregion

        #region Methods
        public void DrawString(GraphicsContext graphics, string text)
        {
            Argument.EnsureNotNull(graphics, "graphics");
            Argument.EnsureNotNull(text, "text");

            float x = 0;
            graphics.StrokeColor = Color.Red;
            foreach (char character in text)
            {
                Rectangle textureRectangle;
                if (!characterTextureRectangles.TryGetValue(character, out textureRectangle))
                {
                    Debug.Fail(
                        "Failed to retrieve texture rectangle for character '{0}'."
                        .FormatInvariant(character));
                    continue;
                }

                Rectangle rectangle = new Rectangle(x, 0, textureRectangle.Width * 100, textureRectangle.Height * 100);

                if (!char.IsWhiteSpace(character)) graphics.Fill(rectangle, texture, textureRectangle);

                x += textureRectangle.Width * 100;
            }
        }

        public void Dispose()
        {
            texture.Dispose();
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
