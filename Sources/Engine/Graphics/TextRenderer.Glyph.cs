using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Geometry;

namespace Orion.Engine.Graphics
{
    partial class TextRenderer
    {
        /// <summary>
        /// Stores the location of a rendered glyph in the TextRenderer's textures.
        /// </summary>
        private struct Glyph
        {
            public readonly int TextureIndex;
            public readonly Rectangle TextureRectangle;

            public Glyph(int textureIndex, Rectangle textureRectangle)
            {
                this.TextureIndex = textureIndex;
                this.TextureRectangle = textureRectangle;
            }
        }
    }
}
