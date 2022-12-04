using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using OpenTK.Graphics;
using OpenTK.Math;
using Orion.Geometry;
using RectangleF = System.Drawing.RectangleF;
using Color = System.Drawing.Color;
using Font = System.Drawing.Font;
using Image = System.Drawing.Image;

namespace Orion.Engine.Graphics
{
    /// <summary>
    /// Represents a space in which it is possible to draw. Methods to fill and stroke shapes are supplied.
    /// </summary>
    public sealed class GraphicsContext
    {
        #region Instance
        #region Fields
        private readonly Stack<Region> scissorStack = new Stack<Region>();
        private Font font = new Font("Trebuchet MS", 14);
        #endregion

        #region Constructors
        [Obsolete("To be made internal and created by the engine.")]
        public GraphicsContext() { }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the size of the viewport.
        /// </summary>
        public Size ViewportSize
        {
            get
            {
                int[] viewportCoordinates = new int[4];
                GL.GetInteger(GetPName.Viewport, viewportCoordinates);

                return new Size(viewportCoordinates[2], viewportCoordinates[3]);
            }
        }

        /// <summary>
        /// Gets the clipping region of the viewport.
        /// </summary>
        public Region ScissorRegion
        {
            get
            {
                return scissorStack.Count == 0
                ? new Region(ViewportSize)
                : scissorStack.Peek();
            }
        }

        /// <summary>
        /// Accesses the <see cref="Font"/> currently used to render text.
        /// </summary>
        public Font Font
        {
            get { return font; }
            set { font = value; }
        }
        #endregion

        #region Methods
        #region OpenGL Context
        [Obsolete("Superseded by Transform.")]
        public DisposableHandle SetViewTransform(Rectangle parentSystem, Rectangle bounds)
        {
            GL.PushMatrix();

            GL.Translate(parentSystem.MinX, parentSystem.MinY, 0);
            GL.Scale(parentSystem.Width / bounds.Width, parentSystem.Height / bounds.Height, 1);
            GL.Translate(-bounds.MinX, -bounds.MinY, 0);

            return new DisposableHandle(() => GL.PopMatrix());
        }
        #endregion

        #region Clearing
        /// <summary>
        /// Clears the backbuffer to a given color.
        /// </summary>
        /// <param name="color">The color to which the backbuffer should be cleared.</param>
        public void Clear(ColorRgb color)
        {
            GL.ColorMask(true, true, true, true);
            GL.ClearColor(color.R, color.G, color.B, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.ColorMask(true, true, true, false);
        }
        #endregion

        #region Textures
        /// <summary>
        /// Creates a texture from the data contained in an array.
        /// </summary>
        /// <param name="size">The size of the texture to be created.</param>
        /// <param name="pixelFormat">The pixel format of the texture data.</param>
        /// <param name="pixelData">The pixel data used to initialize the texture.</param>
        /// <returns>A newly created texture.</returns>
        public Texture CreateTexture<T>(Size size, PixelFormat pixelFormat, ArraySegment<T> pixelData) where T : struct
        {
            Argument.EnsureNotNull(pixelData.Array, "pixelData.Array");

            GCHandle pinningHandle = GCHandle.Alloc(pixelData.Array, GCHandleType.Pinned);
            try
            {
                long address = (long)pinningHandle.AddrOfPinnedObject() + (pixelData.Offset * Marshal.SizeOf(typeof(T)));
                return new Texture(size, pixelFormat, (IntPtr)address);
            }
            finally
            {
                pinningHandle.Free();
            }
        }

        /// <summary>
        /// Creates a new texture from a given pixel surface.
        /// </summary>
        /// <param name="surface">The pixel surface to be copied to the new texture.</param>
        /// <returns>A newly created texture.</returns>
        public Texture CreateTexture(IPixelSurface surface)
        {
            Texture texture = null;
            surface.Lock((Region)surface.Size, Access.Read, rawImage =>
            {
                texture = new Texture(rawImage.Size, rawImage.PixelFormat, rawImage.DataPointer);
            });

            return texture;
        }
        /// <summary>
        /// Creates a new texture from a <see cref="System.Drawing.Image"/>.
        /// </summary>
        /// <param name="image">The image to be copied to the new texture.</param>
        /// <returns>A newly created texture.</returns>
        public Texture CreateTexture(Image image)
        {
            Argument.EnsureNotNull(image, "image");

            IPixelSurface surface = BufferedPixelSurface.FromImage(image);
            return CreateTexture(surface);
        }

        /// <summary>
        /// Creates a texture without initializing its contents.
        /// </summary>
        /// <param name="size">The size of the texture to be created.</param>
        /// <param name="pixelFormat">The pixel format of the texture to be created.</param>
        /// <returns>A newly created texture.</returns>
        public Texture CreateBlankTexture(Size size, PixelFormat pixelFormat)
        {
            return new Texture(size, pixelFormat);
        }

        /// <summary>
        /// Creates a texture with a checkerboard pattern.
        /// </summary>
        /// <param name="size">The size of the texture to be created.</param>
        /// <param name="firstColor">The first color of the checkerboard pattern.</param>
        /// <param name="secondColor">The second color of the checkerboard pattern.</param>
        /// <returns>A newly created checkerboard texture.</returns>
        public Texture CreateCheckerboardTexture(Size size, ColorRgb firstColor, ColorRgb secondColor)
        {
            using (BufferedPixelSurface surface = BufferedPixelSurface.CreateCheckerboard(size, firstColor, secondColor))
                return CreateTexture(surface);
        }

        /// <summary>
        /// Creates a new texture from a file via a stream.
        /// </summary>
        /// <param name="stream">A stream which accesses an image file.</param>
        /// <returns>A newly created texture with the data from that image.</returns>
        public Texture CreateTextureFromStream(Stream stream)
        {
            Argument.EnsureNotNull(stream, "stream");

            try
            {
                using (Image image = Image.FromStream(stream))
                    return CreateTexture(image);
            }
            catch (OutOfMemoryException e)
            {
                // System.Drawing.Image.FromFile throws an OutOfMemoryException when it fails to decode an image.
                throw new IOException(e.Message, e);
            }
        }

        /// <summary>
        /// Creates a new texture from an image stored in a file.
        /// </summary>
        /// <param name="filePath">The path to the image to be loaded.</param>
        /// <returns>A newly created texture with the data from that image.</returns>
        public Texture CreateTextureFromFile(string filePath)
        {
            Argument.EnsureNotNull(filePath, "filePath");

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                return CreateTextureFromStream(stream);
        }
        #endregion

        #region Scissor Rectangle
        /// <summary>
        /// Applies a temporary scissor rectangle.
        /// </summary>
        /// <param name="region">The region of the viewport to be scissored.</param>
        /// <returns>
        /// A handle that should be disposed when the scope of the scissor rectangle ends.
        /// </returns>
        public DisposableHandle PushScissorRegion(Region region)
        {
            Region clippedRegion = Region.Intersection(ScissorRegion, region) ?? new Region(0, 0);

            GL.Enable(EnableCap.ScissorTest);
            GL.Scissor(
                clippedRegion.MinX, clippedRegion.MinY,
                clippedRegion.Width, clippedRegion.Height);

            scissorStack.Push(clippedRegion);

            return new DisposableHandle(() =>
            {
                scissorStack.Pop();
                if (scissorStack.Count == 0) GL.Disable(EnableCap.ScissorTest);
            });
        }
        #endregion

        #region Geometric Transformation
        /// <summary>
        /// Applies a temporary geometric transformation.
        /// This method is best called in a C# <c>using</c> statement.
        /// </summary>
        /// <param name="transform">The geometric transformation to be applied.</param>
        /// <returns>
        /// A handle that should be disposed when the scope of the transformation ends.
        /// </returns>
        public DisposableHandle PushTransform(Transform transform)
        {
            GL.PushMatrix();
            GL.Translate(transform.Translation.X, transform.Translation.Y, 0);
            float rotationAngleInDegrees = (float)(transform.Rotation * 180 / Math.PI);
            GL.Rotate(rotationAngleInDegrees, Vector3.UnitZ);
            GL.Scale(transform.Scaling.X, transform.Scaling.Y, 1);
            return new DisposableHandle(() => GL.PopMatrix());
        }

        public DisposableHandle PushTransform(Vector2 translation, float rotation, Vector2 scaling)
        {
            Transform transform = new Transform(translation, rotation, scaling);
            return PushTransform(transform);
        }

        public DisposableHandle PushTransform(Vector2 translation, float rotation)
        {
            return PushTransform(translation, rotation, new Vector2(1, 1));
        }

        public DisposableHandle PushTransform(Vector2 translation, float rotation, float scaling)
        {
            return PushTransform(translation, rotation, new Vector2(scaling, scaling));
        }

        public DisposableHandle PushTranslate(Vector2 translation)
        {
            return PushTransform(translation, 0, new Vector2(1, 1));
        }
        #endregion

        #region Drawing
        #region Ellipses (and implicitly converted circles)
        /// <summary>
        /// Fills a given <see cref="Ellipse"/>.
        /// </summary>
        /// <param name="ellipse">An <see cref="Ellipse"/> to be filled.</param>
        /// <param name="color">The color to be used to fill the shape.</param>
        public void Fill(Ellipse ellipse, ColorRgba color)
        {
            CommitColor(color);
            GL.Begin(BeginMode.Polygon);
            DrawVertices(ellipse);
            GL.End();
        }

        /// <summary>
        /// Strokes the outline of a given <see cref="Ellipse"/>.
        /// </summary>
        /// <param name="ellipse">An <see cref="Ellipse"/> to be strokes.</param>
        /// <param name="color">The color to be used to stroke the shape.</param>
        public void Stroke(Ellipse ellipse, ColorRgba color)
        {
            CommitColor(color);
            GL.Begin(BeginMode.LineLoop);
            DrawVertices(ellipse);
            GL.End();
        }

        private void DrawVertices(Ellipse ellipse)
        {
            for (int i = 0; i < unitCirclePoints.Length; ++i)
            {
                Vector2 unitCirclePoint = unitCirclePoints[i];
                float x = ellipse.Center.X + unitCirclePoint.X * ellipse.Radii.X;
                float y = ellipse.Center.Y + unitCirclePoint.Y * ellipse.Radii.Y;
                DrawVertex(x, y);
            }
        }
        #endregion

        #region Rectangles
        /// <summary>
        /// Fills a <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="rectangle">A <see href="Rectangle"/> to fill with color.</param>
        /// <param name="color">The color to be used to fill the shape.</param>
        public void Fill(Rectangle rectangle, ColorRgba color)
        {
            CommitColor(color);
            GL.Begin(BeginMode.Quads);
            DrawVertices(rectangle);
            GL.End();
        }

        /// <summary>
        /// Strokes the outline of a <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="rectangle">A <see href="Rectangle"/> to stroke.</param>
        /// <param name="color">The color to be used to stroke the shape.</param>
        public void Stroke(Rectangle rectangle, ColorRgba color)
        {
            CommitColor(color);
            GL.Begin(BeginMode.LineLoop);
            DrawVertices(rectangle);
            GL.End();
        }

        private void DrawVertices(Rectangle rectangle)
        {
            DrawVertex(rectangle.MinX, rectangle.MinY);
            DrawVertex(rectangle.MaxX, rectangle.MinY);
            DrawVertex(rectangle.MaxX, rectangle.MaxY);
            DrawVertex(rectangle.MinX, rectangle.MaxY);
        }
        #endregion

        #region Triangles
        /// <summary>
        /// Fills a <see cref="Triangle"/> shape.
        /// </summary>
        /// <param name="triangle">The <see cref="Triangle"/> to be filled.</param>
        /// <param name="color">The color to be used to fill the shape.</param>
        public void Fill(Triangle triangle, ColorRgba color)
        {
            CommitColor(color);
            GL.Begin(BeginMode.Triangles);
            DrawVertices(triangle);
            GL.End();
        }

        /// <summary>
        /// Strokes the outline of a <see cref="Triangle"/> shape.
        /// </summary>
        /// <param name="triangle">The <see cref="Triangle"/> to be stroked.</param>
        /// <param name="color">The color to be used to stroke the shape.</param>
        public void Stroke(Triangle triangle, ColorRgba color)
        {
            CommitColor(color);
            GL.Begin(BeginMode.LineLoop);
            DrawVertices(triangle);
            GL.End();
        }

        private void DrawVertices(Triangle triangle)
        {
            DrawVertex(triangle.Vertex1);
            DrawVertex(triangle.Vertex2);
            DrawVertex(triangle.Vertex3);
        }
        #endregion

        #region Lines
        public void StrokeLineStrip(IEnumerable<Vector2> points, ColorRgba color)
        {
            CommitColor(color);
            GL.Begin(BeginMode.LineStrip);
            DrawVertices(points);
            GL.End();
        }

        public void Stroke(LineSegment lineSegment, ColorRgba color)
        {
            CommitColor(color);
            GL.Begin(BeginMode.LineStrip);
            DrawVertex(lineSegment.EndPoint1);
            DrawVertex(lineSegment.EndPoint2);
            GL.End();
        }

        private void DrawVertices(IEnumerable<Vector2> points)
        {
            foreach (Vector2 point in points) DrawVertex(point);
        }
        #endregion

        #region Text
        public void Draw(string text, ColorRgba color)
        {
            Draw(text, new Vector2(0, 0), color);
        }

        public void Draw(string text, Vector2 position, ColorRgba color)
        {
            Text textObject = new Text(text, font);
            Draw(textObject, position, color);
        }

        public void Draw(Text text, ColorRgba color)
        {
            Draw(text, Vector2.Zero, color);
        }

        public void Draw(Text text, Rectangle clippingRect, ColorRgba color)
        {
            Draw(text, Vector2.Zero, clippingRect, color);
        }

        public void Draw(Text text, Vector2 position, ColorRgba color)
        {
            Draw(text, position, text.Frame, color);
        }

        /// <summary>
        /// Draws a Text object inside a clipping rectangle.
        /// </summary>
        /// <remarks>Words or lines not fitting in the rectangle will be completely trimmed.</remarks>
        /// <param name="text">The <see cref="Text"/> object to draw</param>
        /// <param name="clippingRect">The rectangle clipping the text</param>
        /// <param name="color">The color with which to draw the text.</param>
        public void Draw(Text text, Vector2 origin, Rectangle clippingRect, ColorRgba color)
        {
            GL.PushMatrix();
            GL.Translate(origin.X, origin.Y, 0);
            GL.Scale(1, -1, 1);
            RectangleF renderInto = new RectangleF(0, -clippingRect.Height, clippingRect.Width, clippingRect.Height);

            Color systemColor = Color.FromArgb(color.ByteA, color.ByteR, color.ByteG, color.ByteB);

            // We could enable blending here when alpha < 255 but OpenTK doesn't support it :(.
            Text.defaultTextPrinter.Print(text.Value, text.Font, systemColor, renderInto);

            GL.PopMatrix();
        }
        #endregion

        #region Textured
        public void Fill(Rectangle rectangle, Texture texture, Rectangle textureRectangle, ColorRgba tint)
        {
            Argument.EnsureNotNull(texture, "texture");

            CommitColor(tint);

            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, texture.ID);
            if (texture.PixelFormat.HasAlphaChannel())
            {
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            }

            DrawTexturedQuad(rectangle, textureRectangle);

            if (texture.PixelFormat.HasAlphaChannel()) GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.Texture2D);
        }

        public void Fill(Rectangle rectangle, Texture texture, Rectangle textureRectangle)
        {
            Fill(rectangle, texture, textureRectangle, Colors.White);
        }

        public void Fill(Rectangle rectangle, Texture texture, ColorRgba tint)
        {
            Fill(rectangle, texture, Rectangle.Unit, tint);
        }

        public void Fill(Rectangle rectangle, Texture texture)
        {
            Fill(rectangle, texture, Rectangle.Unit, Colors.White);
        }

        /// <summary>
        /// Draws a texture using another texture as a drawing mask.
        /// </summary>
        /// <param name="rectangle">The rectangle where the texture is to be drawn.</param>
        /// <param name="maskedTexture">The masked texture to be drawn.</param>
        /// <param name="maskedTextureRectangle">The texture coordinates of the masked texture.</param>
        /// <param name="maskingTexture">The texture to use as a mask.</param>
        /// <param name="maskingTextureRectangle">The texture coordinates of the masking texture.</param>
        public void FillMasked(Rectangle rectangle,
            Texture maskedTexture, Rectangle maskedTextureRectangle,
            Texture maskingTexture, Rectangle maskingTextureRectangle)
        {
            Argument.EnsureNotNull(maskedTexture, "maskedTexture");
            Argument.EnsureNotNull(maskingTexture, "maskingTexture");

            CommitColor(Colors.White);

            GL.ColorMask(false, false, false, true);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            Fill(rectangle, maskingTexture, maskingTextureRectangle);

            GL.ColorMask(true, true, true, false);
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.DstAlpha, BlendingFactorDest.OneMinusDstAlpha);
            maskedTexture.BindWhile(() => DrawTexturedQuad(rectangle, maskedTextureRectangle));
            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.Texture2D);

            GL.ColorMask(true, true, true, true);
        }

        private void DrawTexturedQuad(Rectangle rectangle, Rectangle textureRectangle)
        {
            GL.Begin(BeginMode.Quads);
            GL.TexCoord2(textureRectangle.MinX, textureRectangle.MinY);
            DrawVertex(rectangle.MinX, rectangle.MinY);
            GL.TexCoord2(textureRectangle.MaxX, textureRectangle.MinY);
            DrawVertex(rectangle.MaxX, rectangle.MinY);
            GL.TexCoord2(textureRectangle.MaxX, textureRectangle.MaxY);
            DrawVertex(rectangle.MaxX, rectangle.MaxY);
            GL.TexCoord2(textureRectangle.MinX, textureRectangle.MaxY);
            DrawVertex(rectangle.MinX, rectangle.MaxY);
            GL.End();
        }

        private void DrawTexturedQuad(Rectangle rectangle)
        {
            DrawTexturedQuad(rectangle, Rectangle.Unit);
        }
        #endregion
        #endregion

        #region Non-Public
        private void DrawVertex(Vector2 vector)
        {
            GL.Vertex2(vector);
        }

        private void DrawVertex(float x, float y)
        {
            // A profile session and some reflection found out that OpenTK
            // does locking and sanity checks on GL.Vertex2(float, float),
            // but not on GL.Vertex2(Vector2), so we can save performance
            // by calling that overload instead. Silly huh?
            DrawVertex(new Vector2(x, y));
        }

        private void CommitColor(ColorRgba color)
        {
            if (color.A < 1f)
            {
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            }
            else
                GL.Disable(EnableCap.Blend);

            GL.Color4(color.R, color.G, color.B, color.A);
        }
        #endregion
        #endregion
        #endregion

        #region Static
        #region Fields
        private static readonly Vector2[] unitCirclePoints;
        #endregion

        #region Constructor
        static GraphicsContext()
        {
            unitCirclePoints = new Vector2[32];
            double angleIncrement = Math.PI * 2 / unitCirclePoints.Length;
            for (int i = 0; i < unitCirclePoints.Length; ++i)
            {
                double angle = angleIncrement * i;
                double x = Math.Cos(angle);
                double y = Math.Sin(angle);
                unitCirclePoints[i] = new Vector2((float)x, (float)y);
            }
        }
        #endregion
        #endregion
    }
}
