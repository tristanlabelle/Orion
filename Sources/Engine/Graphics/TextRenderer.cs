using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using OpenTK;
using Box = Orion.Engine.Geometry.Rectangle;

namespace Orion.Engine.Graphics
{
    using Graphics = System.Drawing.Graphics;

    /// <summary>
    /// Provides means to render text on the screen.
    /// </summary>
    public sealed class TextRenderer : IDisposable
    {
        #region Glyph structure
        private struct Glyph
        {
            public readonly int TextureIndex;
            public readonly Box TextureRectangle;

            public Glyph(int textureIndex, Box textureRectangle)
            {
                this.TextureIndex = textureIndex;
                this.TextureRectangle = textureRectangle;
            }
        }
        #endregion

        #region Fields
        private const int TextureSize = 512;
        private const int GlyphPaddingSize = 1;

        private static readonly StringFormat stringFormat;

        private readonly GraphicsContext graphicsContext;
        private readonly Font font;
        private readonly Bitmap fontRenderTarget;
        private readonly Graphics fontRenderer;
        private readonly List<Texture> textures = new List<Texture>();
        private readonly Dictionary<char, Glyph> glyphs = new Dictionary<char, Glyph>();
        private readonly float spaceWidth;
        private int nextGlyphX;
        private int nextGlyphY;
        private int maxGlyphHeight;
        #endregion

        #region Constructors
        public TextRenderer(GraphicsContext graphicsContext, Font font)
        {
            Argument.EnsureNotNull(graphicsContext, "graphicsContext");
            Argument.EnsureNotNull(font, "font");

            this.graphicsContext = graphicsContext;
            this.font = font;
            fontRenderTarget = new Bitmap(font.Height * 2, font.Height * 2, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            fontRenderer = Graphics.FromImage(fontRenderTarget);

            // use the width of an 'i' for the size of a space.
            Glyph iGlyph = FindOrRenderGlyph('i');
            spaceWidth = iGlyph.TextureRectangle.Width * textures[iGlyph.TextureIndex].Height / font.Height;
        }

        static TextRenderer()
        {
            stringFormat = new StringFormat();
            stringFormat.SetMeasurableCharacterRanges(new[] { new CharacterRange(0, 1) });
        }
        #endregion

        #region Properties
        private Texture TopmostTexture
        {
            get { return textures[textures.Count - 1]; }
        }
        #endregion

        #region Methods
        public void Draw(IEnumerable<char> text, float size, Vector2 origin, ColorRgba tint, float maxWidth)
        {
            Argument.EnsureNotNull(text, "text");
            Argument.EnsureStrictlyPositive(size, "size");
            Argument.EnsureStrictlyPositive(maxWidth, "maxWidth");

            Vector2 position = origin;
            foreach (char character in text)
            {
                if (char.IsWhiteSpace(character))
                {
                    position.X += spaceWidth * size;
                }
                else
                {
                    Glyph glyph = FindOrRenderGlyph(character);
                    Texture texture = textures[glyph.TextureIndex];
                    Box box = new Box(position, glyph.TextureRectangle.Size * texture.Height / font.Height * size);
                    graphicsContext.Fill(box, texture, glyph.TextureRectangle, tint);
                    position.X += glyph.TextureRectangle.Width * texture.Height / font.Height * size;
                }
            }
        }

        public void Dispose()
        {
            foreach (Texture texture in textures)
                texture.Dispose();
            textures.Clear();
            glyphs.Clear();
            fontRenderer.Dispose();
            fontRenderTarget.Dispose();
        }

        private Glyph FindOrRenderGlyph(char character)
        {
            Glyph glyph;
            if (glyphs.TryGetValue(character, out glyph)) return glyph;

            glyph = RenderGlyph(character);
            glyphs.Add(character, glyph);
            return glyph;
        }

        private Glyph RenderGlyph(char character)
        {
            string characterString = character.ToString();

            fontRenderer.Clear(Color.FromArgb(0, 255, 255, 255));
            fontRenderer.DrawString(characterString, font, Brushes.White, new PointF(0, 0));
            Rectangle renderedBounds = GetRenderedBounds(characterString);

            if (textures.Count == 0)
            {
                CreateTexture();
            }
            else
            {
                if (nextGlyphX + renderedBounds.Width > TopmostTexture.Width)
                {
                    nextGlyphX = 0;
                    nextGlyphY += maxGlyphHeight + GlyphPaddingSize;
                    maxGlyphHeight = 0;
                }

                if (nextGlyphY + renderedBounds.Height > TopmostTexture.Height)
                {
                    CreateTexture();
                    nextGlyphX = 0;
                    nextGlyphY = 0;
                    maxGlyphHeight = 0;
                }
            }

            Texture texture = TopmostTexture;

            if (renderedBounds.Width * renderedBounds.Height > 0)
                BlitToTexture(renderedBounds, texture);

            Box textureRectangle = new Box(
                nextGlyphX / (float)texture.Width,
                nextGlyphY / (float)texture.Height,
                renderedBounds.Width / (float)texture.Width,
                renderedBounds.Height / (float)texture.Height);

            nextGlyphX += renderedBounds.Width + GlyphPaddingSize;
            if (maxGlyphHeight < renderedBounds.Height) maxGlyphHeight = renderedBounds.Height;

            return new Glyph(textures.Count - 1, textureRectangle);
        }

        private void BlitToTexture(Rectangle renderedBounds, Texture texture)
        {
            BitmapData bitmapData = fontRenderTarget.LockBits(renderedBounds,
                ImageLockMode.ReadOnly, fontRenderTarget.PixelFormat);
            try
            {
                unsafe
                {
                    byte* sourcePointer = (byte*)bitmapData.Scan0;
                    texture.LockToOverwrite(new Region(nextGlyphX, nextGlyphY, renderedBounds.Width, renderedBounds.Height), surface =>
                    {
                        byte* destinationPointer = (byte*)surface.DataPointer;
                        for (int y = 0; y < renderedBounds.Height; ++y)
                        {
                            for (int x = 0; x < renderedBounds.Width; ++x)
                            {
                                byte* sourcePixelPointer = sourcePointer + bitmapData.Stride * (renderedBounds.Height - y - 1) + x * 4;
                                byte* destinationPixelPointer = destinationPointer + surface.Stride * y + x * 4;

                                // Force to white + alpha
                                destinationPixelPointer[0] = 255;
                                destinationPixelPointer[1] = 255;
                                destinationPixelPointer[2] = 255;
                                float alpha = (sourcePixelPointer[0] + sourcePixelPointer[1] + sourcePixelPointer[2] + sourcePixelPointer[3]) / 4.0f;
                                destinationPixelPointer[3] = (byte)alpha;
                            }
                        }
                    });
                }
            }
            finally
            {
                fontRenderTarget.UnlockBits(bitmapData);
            }
        }

        private Rectangle GetRenderedBounds(string characterString)
        {
            RectangleF layoutRect = new RectangleF(0, 0, font.Height * 2, font.Height * 2);
            using (var characterRegion = fontRenderer.MeasureCharacterRanges(characterString, font, layoutRect, stringFormat)[0])
            {
                RectangleF characterBounds = characterRegion.GetBounds(fontRenderer);
                return Rectangle.FromLTRB(
                    (int)characterBounds.Left,
                    (int)characterBounds.Top,
                    (int)Math.Ceiling(characterBounds.Right),
                    (int)Math.Ceiling(characterBounds.Bottom));
            }
        }

        private Texture CreateTexture()
        {
            Texture texture = graphicsContext.CreateBlankTexture(new Size(TextureSize, TextureSize), PixelFormat.Rgba);
            texture.SetSmooth(true);
            textures.Add(texture);
            return texture;
        }
        #endregion
    }
}
