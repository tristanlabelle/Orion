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
            #region Fields
            public readonly Font Font;
            public readonly List<Texture> Textures = new List<Texture>();
            public readonly Dictionary<char, Glyph> Glyphs = new Dictionary<char, Glyph>();

            /// <summary>
            /// The height of the font, in pixels.
            /// </summary>
            /// <remarks>
            /// This value is cached because of the impact of its computation on performance.
            /// </remarks>
            public readonly float Height;

            public float SpaceWidth;
            public int NextGlyphX;
            public int NextGlyphY;
            public int MaxLineGlyphHeight;
            #endregion

            #region Constructors
            public RenderedFont(Font font)
            {
                Argument.EnsureNotNull(font, "font");

                this.Font = font;
                this.Height = Font.Height;
            }
            #endregion

            #region Properties
            public Texture TopmostTexture
            {
                get { return Textures[Textures.Count - 1]; }
            }
            #endregion

            #region Methods
            public void Dispose()
            {
                foreach (Texture texture in Textures)
                    texture.Dispose();
                Textures.Clear();
                Glyphs.Clear();
            }
            #endregion
        }
    }
}
