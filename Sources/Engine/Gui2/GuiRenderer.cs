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
        #region Methods
        #region Begin/End
        /// <summary>
        /// Invoked when the rendering of the GUI begins.
        /// </summary>
        /// <returns>A disposable object to enable using C# using statements to automatically invoke <see cref="M:End"/>.</returns>
        public DisposableHandle Begin()
        {
            BeginImpl();
            return new DisposableHandle(End);
        }

        /// <summary>
        /// Invoked when the rendering of the GUI ends.
        /// </summary>
        public void End()
        {
            EndImpl();
        }

        /// <summary>
        /// Setups the renderer to begin drawing operations for a frame.
        /// </summary>
        protected virtual void BeginImpl() { }

        /// <summary>
        /// Cleans up the renderer after the last drawing operations of a frame.
        /// </summary>
        protected virtual void EndImpl() { }
        #endregion

        #region Transformations
        /// <summary>
        /// Adds a new transformation to the transformation stack.
        /// </summary>
        /// <param name="transform">The transformation to be pushed.</param>
        /// <returns>A disposable object enabling the use of C# using statements to invoke <see cref="PopTransform"/>.</returns>
        public DisposableHandle PushTransform(Transform transform)
        {
            PushTransformImpl(transform);
            return new DisposableHandle(PopTransform);
        }

        /// <summary>
        /// Reverts the transformation to its value before the last <see cref="PushTransform"/> call.
        /// </summary>
        public void PopTransform()
        {
            PopTransformImpl();
        }

        /// <summary>
        /// Adds a new transformation to the transformation stack.
        /// </summary>
        /// <param name="transform">The transformation to be pushed.</param>
        protected abstract void PushTransformImpl(Transform transform);

        /// <summary>
        /// Reverts the transformation to its value before the last <see cref="PushTransformImpl"/> call.
        /// </summary>
        protected abstract void PopTransformImpl();
        #endregion

        #region Clipping
        /// <summary>
        /// Changes the clipping rectangle by pushing a new value on the clipping rectangle stack.
        /// </summary>
        /// <param name="rectangle">The clipping rectangle to be pushed.</param>
        /// <returns>A disposable object to be used in a C# using expression.</returns>
        public DisposableHandle PushClippingRectangle(Region rectangle)
        {
            PushClippingRectangleImpl(rectangle);
            return new DisposableHandle(PopClippingRectangle);
        }

        /// <summary>
        /// Reverts to the clipping rectangle in place before the last <see cref="M:PushClippingRectangle(Region)"/> call.
        /// </summary>
        public void PopClippingRectangle()
        {
            PopClippingRectangleImpl();
        }

        /// <summary>
        /// Changes the clipping rectangle by pushing a new value on the clipping rectangle stack.
        /// </summary>
        /// <param name="rectangle">The clipping rectangle to be pushed.</param>
        protected abstract void PushClippingRectangleImpl(Region rectangle);

        /// <summary>
        /// Reverts to the clipping rectangle in place before the last <see cref="M:PushClippingRectangleImpl(Region)"/> call.
        /// </summary>
        protected abstract void PopClippingRectangleImpl();
        #endregion

        /// <summary>
        /// Attempts to retrieve a texture by its name.
        /// </summary>
        /// <param name="name">The name of the texture to be retrieve.</param>
        /// <returns>The texture identified by that name, or a dummy texture if no such texture was found.</returns>
        [Obsolete("Use GameGraphics texture getters.")]
        public abstract Texture GetTexture(string name);

        #region Text
        /// <summary>
        /// Measures the space occupied by a string of text.
        /// </summary>
        /// <param name="text">The text to be measured.</param>
        /// <param name="options">Options determining how the text should be measured.</param>
        /// <returns>The size occupied by the text, in pixels.</returns>
        public abstract Size MeasureText(Substring text, ref TextRenderingOptions options);

        /// <summary>
        /// Draws a string of text.
        /// </summary>
        /// <param name="text">The text to be drawn.</param>
        /// <param name="options">Options determining how the text should be drawn.</param>
        public abstract void DrawText(Substring text, ref TextRenderingOptions options);
        #endregion

        /// <summary>
        /// Draws a sprite.
        /// </summary>
        /// <param name="sprite">The sprite to be drawn.</param>
        public abstract void DrawSprite(ref GuiSprite sprite);

        /// <summary>
        /// Draws a uniformly colored filled rectangle.
        /// </summary>
        /// <param name="rectangle">The rectangle to be filled.</param>
        /// <param name="color">The color to fill the rectangle with.</param>
        public void DrawRectangle(Region rectangle, ColorRgba color)
        {
            var sprite = new GuiSprite
            {
                Rectangle = rectangle,
                Color = color
            };
            DrawSprite(ref sprite);
        }

        public void DrawNinePart(Region rectangle, Texture texture, ColorRgba color)
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
