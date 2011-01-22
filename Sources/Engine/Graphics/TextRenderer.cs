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
    public sealed partial class TextRenderer : IDisposable
    {
        #region Fields
        private const int TextureSize = 512;
        private const int GlyphPaddingSize = 1;

        private static readonly StringFormat stringFormat;

        private readonly GraphicsContext graphicsContext;
        private readonly Dictionary<Font, RenderedFont> renderedFonts = new Dictionary<Font, RenderedFont>();
        private StringBuilder tempStringBuilder = new StringBuilder();
        private Bitmap fontRenderTarget;
        private Graphics fontRenderer;
        #endregion

        #region Constructors
        public TextRenderer(GraphicsContext graphicsContext)
        {
            Argument.EnsureNotNull(graphicsContext, "graphicsContext");

            this.graphicsContext = graphicsContext;
        }

        static TextRenderer()
        {
            stringFormat = new StringFormat();
            stringFormat.SetMeasurableCharacterRanges(new[] { new CharacterRange(0, 1) });
        }
        #endregion

        #region Methods
        public Size Draw(IEnumerable<char> text, ref TextRenderingOptions options)
        {
            Argument.EnsureNotNull(text, "text");

            foreach (char character in text)
                tempStringBuilder.Append(character);

            Size size = Draw(ref options);

            tempStringBuilder.Clear();

            return size;
        }

        public Size Draw(Substring text, ref TextRenderingOptions options)
        {
            tempStringBuilder.Append(text.BaseString, text.StartIndex, text.Length);
            Size size = Draw(ref options);
            tempStringBuilder.Clear();

            return size;
        }

        public Size Measure(Substring text, ref TextRenderingOptions options)
        {
            tempStringBuilder.Append(text.BaseString, text.StartIndex, text.Length);
            Size size = Draw(ref options, false);
            tempStringBuilder.Clear();

            return size;
        }

        private Size Draw(ref TextRenderingOptions options)
        {
        	return Draw(ref options, true);
        }
        
        private Size Draw(ref TextRenderingOptions options, bool draw)
        {
            RenderedFont renderedFont = FindOrCreateRenderedFont(options.Font);

            int totalWidth = 0;
            int totalHeight = (int)Math.Ceiling(renderedFont.Height);

            Vector2 origin = options.Origin.ToVector();
            Vector2 position = origin;
            for (int i = 0; i < tempStringBuilder.Length; ++i)
            {
                char character = tempStringBuilder[i];
                if (char.IsWhiteSpace(character))
                {
                    position.X += renderedFont.SpaceWidth * renderedFont.Height;
                    if (position.X - origin.X > totalWidth)
                        totalWidth = (int)Math.Ceiling(position.X - origin.X);
                }
                else
                {
                    Glyph glyph = FindOrRenderGlyph(renderedFont, character);
                    Texture texture = renderedFont.Textures[glyph.TextureIndex];
                    Box textureRectangle = glyph.TextureRectangle;
                    Box box = new Box(position, glyph.TextureRectangle.Size * texture.Height);

                    if (box.MaxX > options.MaxWidthInPixels)
                    {
                        if (options.HorizontalOverflowPolicy == TextOverflowPolicy.Clip)
                        {
                            float amount = (options.MaxWidthInPixels.Value - box.MinX) / box.Width;
                            box = new Box(box.MinX, box.MinY, box.Width * amount, box.Height);
                            textureRectangle = new Box(textureRectangle.MinX, textureRectangle.MinY, textureRectangle.Width * amount, textureRectangle.Height);

                            // Prevent other characters from being drawn
                            i = tempStringBuilder.Length;
                        }
                        else if (options.HorizontalOverflowPolicy == TextOverflowPolicy.Wrap)
                        {
                            position.X = options.Origin.X;
                            position.Y -= renderedFont.Height;
                            box = new Box(position.X, position.Y, box.Width, box.Height);
                        }
                    }

                    if (box.MaxX - origin.X > totalWidth) totalWidth = (int)Math.Ceiling(box.MaxX - origin.X);
                    if (box.MaxY - origin.Y > totalHeight) totalHeight = (int)Math.Ceiling(box.MaxY - origin.Y);

                    if (draw) graphicsContext.Fill(box, texture, textureRectangle, options.Color);
                    position.X += glyph.TextureRectangle.Width * texture.Height;
                }
            }

            return new Size(totalWidth, totalHeight);
        }

        public void Dispose()
        {
            foreach (RenderedFont renderedFont in renderedFonts.Values)
                renderedFont.Dispose();
            renderedFonts.Clear();

            if (fontRenderer != null)
            {
                fontRenderer.Dispose();
                fontRenderTarget.Dispose();
            }
        }

        private RenderedFont FindOrCreateRenderedFont(Font font)
        {
            RenderedFont renderedFont;
            if (renderedFonts.TryGetValue(font, out renderedFont))
                return renderedFont;

            // Cached for performance
            int fontHeight = font.Height;
            if (fontRenderTarget == null || fontRenderTarget.Height < fontHeight * 2)
            {
                if (fontRenderer != null)
                {
                    fontRenderer.Dispose();
                    fontRenderTarget.Dispose();
                }

                fontRenderTarget = new Bitmap(fontHeight * 2, fontHeight * 2, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                fontRenderer = Graphics.FromImage(fontRenderTarget);
            }

            renderedFont = new RenderedFont(font);
            renderedFonts.Add(font, renderedFont);

            // use the width of an 'i' for the size of a space.
            Glyph iGlyph = FindOrRenderGlyph(renderedFont, 'i');
            renderedFont.SpaceWidth = iGlyph.TextureRectangle.Width * renderedFont.Textures[iGlyph.TextureIndex].Height / fontHeight;

            return renderedFont;
        }

        private Glyph FindOrRenderGlyph(RenderedFont renderedFont, char character)
        {
            Glyph glyph;
            if (renderedFont.Glyphs.TryGetValue(character, out glyph)) return glyph;

            glyph = RenderGlyph(renderedFont, character);
            renderedFont.Glyphs.Add(character, glyph);
            return glyph;
        }

        private Glyph RenderGlyph(RenderedFont renderedFont, char character)
        {
            string characterString = character.ToString();

            fontRenderer.Clear(Color.FromArgb(0, 255, 255, 255));
            fontRenderer.DrawString(characterString, renderedFont.Font, Brushes.White, new PointF(0, 0));
            Rectangle renderedBounds = GetRenderedBounds(renderedFont, characterString);

            if (renderedFont.Textures.Count == 0)
            {
                CreateTexture(renderedFont);
            }
            else
            {
                if (renderedFont.NextGlyphX + renderedBounds.Width > renderedFont.TopmostTexture.Width)
                {
                    renderedFont.NextGlyphX = 0;
                    renderedFont.NextGlyphY += renderedFont.MaxLineGlyphHeight + GlyphPaddingSize;
                    renderedFont.MaxLineGlyphHeight = 0;
                }

                if (renderedFont.NextGlyphY + renderedBounds.Height > renderedFont.TopmostTexture.Height)
                {
                    CreateTexture(renderedFont);
                    renderedFont.NextGlyphX = 0;
                    renderedFont.NextGlyphY = 0;
                    renderedFont.MaxLineGlyphHeight = 0;
                }
            }

            Texture texture = renderedFont.TopmostTexture;

            if (renderedBounds.Width * renderedBounds.Height > 0)
                BlitToTexture(renderedBounds, texture, new Point(renderedFont.NextGlyphX, renderedFont.NextGlyphY));

            Box textureRectangle = new Box(
                renderedFont.NextGlyphX / (float)texture.Width,
                renderedFont.NextGlyphY / (float)texture.Height,
                renderedBounds.Width / (float)texture.Width,
                renderedBounds.Height / (float)texture.Height);

            renderedFont.NextGlyphX += renderedBounds.Width + GlyphPaddingSize;
            if (renderedFont.MaxLineGlyphHeight < renderedBounds.Height)
                renderedFont.MaxLineGlyphHeight = renderedBounds.Height;

            return new Glyph(renderedFont.Textures.Count - 1, textureRectangle);
        }

        private void BlitToTexture(Rectangle renderedBounds, Texture texture, Point texturePoint)
        {
            BitmapData bitmapData = fontRenderTarget.LockBits(renderedBounds,
                ImageLockMode.ReadOnly, fontRenderTarget.PixelFormat);
            try
            {
                unsafe
                {
                    byte* sourcePointer = (byte*)bitmapData.Scan0;
                    texture.LockToOverwrite(new Region(texturePoint.X, texturePoint.Y, renderedBounds.Width, renderedBounds.Height), surface =>
                    {
                        byte* destinationPointer = (byte*)surface.DataPointer;
                        for (int y = 0; y < renderedBounds.Height; ++y)
                        {
                            for (int x = 0; x < renderedBounds.Width; ++x)
                            {
                                byte* sourcePixelPointer = sourcePointer + bitmapData.Stride * y + x * 4;
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

        private Rectangle GetRenderedBounds(RenderedFont renderedFont, string characterString)
        {
            RectangleF layoutRect = new RectangleF(0, 0, renderedFont.Height * 2, renderedFont.Height * 2);
            using (var characterRegion = fontRenderer.MeasureCharacterRanges(characterString, renderedFont.Font, layoutRect, stringFormat)[0])
            {
                RectangleF characterBounds = characterRegion.GetBounds(fontRenderer);
                return Rectangle.FromLTRB(
                    (int)characterBounds.Left,
                    (int)characterBounds.Top,
                    (int)Math.Ceiling(characterBounds.Right),
                    (int)Math.Ceiling(characterBounds.Bottom));
            }
        }

        private Texture CreateTexture(RenderedFont renderedFont)
        {
            Texture texture = graphicsContext.CreateBlankTexture(new Size(TextureSize, TextureSize), PixelFormat.Rgba);
            texture.SetSmooth(true);
            renderedFont.Textures.Add(texture);
            return texture;
        }
        #endregion
    }
}
