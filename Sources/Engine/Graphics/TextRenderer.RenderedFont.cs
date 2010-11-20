using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Orion.Engine.Graphics
{
    partial class TextRenderer
    {
        private sealed class RenderedFont : IDisposable
        {
            public readonly Font Font;
            public readonly List<Texture> Textures = new List<Texture>();
            public readonly Dictionary<char, Glyph> Glyphs = new Dictionary<char, Glyph>();
            public float SpaceWidth;
            public int NextGlyphX;
            public int NextGlyphY;
            public int MaxLineGlyphHeight;

            public RenderedFont(Font font)
            {
                Argument.EnsureNotNull(font, "font");

                this.Font = font;
            }

            public float LineSpacing
            {
                get
                {
                    return Font.FontFamily.GetLineSpacing(Font.Style) / (float)Font.FontFamily.GetEmHeight(Font.Style) * Font.GetHeight();
                }
            }

            public Texture TopmostTexture
            {
                get { return Textures[Textures.Count - 1]; }
            }

            public void Dispose()
            {
                foreach (Texture texture in Textures)
                    texture.Dispose();
                Textures.Clear();
                Glyphs.Clear();
            }
        }
    }
}
