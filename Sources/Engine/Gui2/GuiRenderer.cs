using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Graphics;
using OpenTK;

namespace Orion.Engine.Gui2
{
    /// <summary>
    /// Base class providing the drawing facility for GUIs.
    /// </summary>
    public abstract class GuiRenderer
    {
        #region Properties
        /// <summary>
        /// Accesses the current clipping rectangle, if any.
        /// </summary>
        public abstract Region? ClippingRectangle { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// Invoked when the rendering of the GUI begins.
        /// </summary>
        public virtual void Begin() { }

        /// <summary>
        /// Invoked when the rendering of the GUI ends.
        /// </summary>
        public virtual void End() { }

        /// <summary>
        /// Attempts to retrieve a texture by its name.
        /// </summary>
        /// <param name="name">The name of the texture to be retrieve.</param>
        /// <returns>The texture identified by that name, or a dummy texture if no such texture was found.</returns>
        public abstract Texture GetTexture(string name);

        /// <summary>
        /// Measures the space occupied by a string of text.
        /// </summary>
        /// <param name="text">The text to be measured.</param>
        /// <param name="options">Options determining how the text should be measured.</param>
        /// <returns>The size occupied by the text, in pixels.</returns>
        public abstract Size MeasureText(string text, ref TextRenderingOptions options);

        /// <summary>
        /// Draws a string of text.
        /// </summary>
        /// <param name="text">The text to be drawn.</param>
        /// <param name="options">Options determining how the text should be drawn.</param>
        public abstract void DrawText(string text, ref TextRenderingOptions options);

        /// <summary>
        /// Draws a sprite.
        /// </summary>
        /// <param name="sprite">The sprite to be drawn.</param>
        public abstract void DrawSprite(ref GuiSprite sprite);

        public void FillNinePart(Region rectangle, Texture texture, ColorRgba color)
        {
            int middleTextureSize = 2;
            int cornerWidth = texture.Width / 2 - 1;
            int cornerHeight = texture.Height / 2 - 1;
            int middleWidth = rectangle.Width - cornerWidth * 2;
            int middleHeight = rectangle.Height - cornerHeight * 2;

            // Min Row
            FillPart(texture, color, rectangle.MinX, rectangle.MinY, cornerWidth, cornerHeight,
                0, 0, cornerWidth, cornerHeight);
            FillPart(texture, color, rectangle.MinX + cornerWidth, rectangle.MinY, middleWidth, cornerHeight,
                cornerWidth, 0, middleTextureSize, cornerHeight);
            FillPart(texture, color, rectangle.MinX + cornerWidth + middleWidth, rectangle.MinY, cornerWidth, cornerHeight,
                cornerWidth + middleTextureSize, 0, cornerWidth, cornerHeight);

            // Middle Row
            FillPart(texture, color, rectangle.MinX, rectangle.MinY + cornerHeight, cornerWidth, middleHeight,
                0, cornerHeight, cornerWidth, middleTextureSize);
            FillPart(texture, color, rectangle.MinX + cornerWidth, rectangle.MinY + cornerHeight, middleWidth, middleHeight,
                cornerWidth, cornerHeight, middleTextureSize, middleTextureSize);
            FillPart(texture, color, rectangle.MinX + cornerWidth + middleWidth, rectangle.MinY + cornerHeight, cornerWidth, middleHeight,
                cornerWidth + middleTextureSize, cornerHeight, cornerWidth, middleTextureSize);

            // Max Row
            FillPart(texture, color, rectangle.MinX, rectangle.MinY + cornerHeight + middleHeight, cornerWidth, cornerHeight,
                0, cornerHeight + middleTextureSize, cornerWidth, cornerHeight);
            FillPart(texture, color, rectangle.MinX + cornerWidth, rectangle.MinY + cornerHeight + middleHeight, middleWidth, cornerHeight,
                cornerWidth, cornerHeight + middleTextureSize, middleTextureSize, cornerHeight);
            FillPart(texture, color, rectangle.MinX + cornerWidth + middleWidth, rectangle.MinY + cornerHeight + middleHeight, cornerWidth, cornerHeight,
                cornerWidth + middleTextureSize, cornerHeight + middleTextureSize, cornerWidth, cornerHeight);
        }

        private void FillPart(Texture texture, ColorRgba color,
            int minX, int minY, int width, int height,
            int minTexX, int minTexY, int texWidth, int texHeight)
        {
            var sprite = new GuiSprite()
            {
                Rectangle = new Region(minX, minY, width, height),
                Texture = texture,
                PixelRectangle = new Region(minTexX, minTexY, texWidth, texHeight),
                Color = color
            };

            DrawSprite(ref sprite);
        }
        #endregion
    }
}
