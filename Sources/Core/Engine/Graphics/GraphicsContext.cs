using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using OpenTK.Graphics;
using OpenTK.Math;
using Orion.Geometry;
using RectangleF = System.Drawing.RectangleF;
using Color = System.Drawing.Color;
using Font = System.Drawing.Font;
using Image = System.Drawing.Image;
using System.Diagnostics;

namespace Orion.Engine.Graphics
{
    /// <summary>
    /// Represents a space in which it is possible to draw. Methods to fill and stroke shapes are supplied.
    /// </summary>
    public sealed class GraphicsContext
    {
        #region Instance
        #region Fields
        private ColorRgba fillColor = Colors.White;
        private ColorRgba strokeColor = Colors.Black;
        private Font font = new Font("Trebuchet MS", 14);
        #endregion

        #region Constructors
        [Obsolete("To be made internal and created by the engine.")]
        public GraphicsContext() { }
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the <see cref="Color"/> currently used to fill shapes.
        /// </summary>
        public ColorRgba FillColor
        {
            get { return fillColor; }
            set { fillColor = value; }
        }

        /// <summary>
        /// Accesses the <see cref="Color"/> currently used to stroke shape outlines.
        /// </summary>
        public ColorRgba StrokeColor
        {
            get { return strokeColor; }
            set { strokeColor = value; }
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
        public DisposableHandle Scissor(Region region)
        {
            bool isActive;
            GL.GetBoolean(GetPName.ScissorTest, out isActive);
            Debug.Assert(!isActive, "Cannot nest Scissor boxes");

            GL.Scissor(region.MinX, region.MinY, region.Width, region.Height);
            GL.Enable(EnableCap.ScissorTest);
            return new DisposableHandle(() => GL.Disable(EnableCap.ScissorTest));
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
        public DisposableHandle Transform(Transform transform)
        {
            GL.PushMatrix();
            GL.Translate(transform.Translation.X, transform.Translation.Y, 0);
            float rotationAngleInDegrees = (float)(transform.Rotation * 180 / Math.PI);
            GL.Rotate(rotationAngleInDegrees, Vector3.UnitZ);
            GL.Scale(transform.Scaling.X, transform.Scaling.Y, 1);
            return new DisposableHandle(() => GL.PopMatrix());
        }

        public DisposableHandle Transform(Vector2 translation, float rotation, Vector2 scaling)
        {
            Transform transform = new Transform(translation, rotation, scaling);
            return Transform(transform);
        }

        public DisposableHandle Transform(Vector2 translation, float rotation)
        {
            return Transform(translation, rotation, new Vector2(1, 1));
        }

        public DisposableHandle Transform(Vector2 translation, float rotation, float scaling)
        {
            return Transform(translation, rotation, new Vector2(scaling, scaling));
        }

        public DisposableHandle Translate(Vector2 translation)
        {
            return Transform(translation, 0, new Vector2(1, 1));
        }
        #endregion

        #region Drawing
        #region Ellipses (and implicitly converted circles)
        /// <summary>
        /// Fills a given <see cref="Ellipse"/> using the current <see cref="P:FillColor"/>.
        /// </summary>
        /// <param name="ellipse">An <see cref="Ellipse"/> to be filled.</param>
        public void Fill(Ellipse ellipse)
        {
            CommitFillColor();
            GL.Begin(BeginMode.Polygon);
            DrawVertices(ellipse);
            GL.End();
        }

        /// <summary>
        /// Strokes the outline of a given <see cref="Ellipse"/> using the current <see cref="P:StrokeColor"/>.
        /// </summary>
        /// <param name="ellipse">An <see cref="Ellipse"/> to be strokes.</param>
        public void Stroke(Ellipse ellipse)
        {
            CommitStrokeColor();
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
        /// Fills a <see cref="Rectangle"/> using the current <see cref="P:FillColor"/>.
        /// </summary>
        /// <param name="rectangle">A <see href="Rectangle"/> to fill with color.</param>
        public void Fill(Rectangle rectangle)
        {
            CommitFillColor();
            GL.Begin(BeginMode.Quads);
            DrawVertices(rectangle);
            GL.End();
        }

        /// <summary>
        /// Strokes the outline of a <see cref="Rectangle"/> using the current <see cref="P:StrokeColor"/>.
        /// </summary>
        /// <param name="rectangle">A <see href="Rectangle"/> to stroke.</param>
        public void Stroke(Rectangle rectangle)
        {
            CommitStrokeColor();
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
        /// Fills a <see cref="Triangle"/> shape using the current <see cref="P:FillColor"/>.
        /// </summary>
        /// <param name="triangle">The <see cref="Triangle"/> to be filled.</param>
        public void Fill(Triangle triangle)
        {
            CommitFillColor();
            GL.Begin(BeginMode.Triangles);
            DrawVertices(triangle);
            GL.End();
        }

        /// <summary>
        /// Strokes the outline of a <see cref="Triangle"/> shape using the current <see cref="P:StrokeColor"/>.
        /// </summary>
        /// <param name="triangle">The <see cref="Triangle"/> to be stroked.</param>
        public void Stroke(Triangle triangle)
        {
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
        /// <summary>
        /// Strokes a line using the current <see cref="P:StrokeColor"/>.
        /// </summary>
        /// <param name="points">A sequence of points forming a line.</param>
        public void StrokeLineStrip(IEnumerable<Vector2> points)
        {
            CommitStrokeColor();
            GL.Begin(BeginMode.LineStrip);
            DrawVertices(points);
            GL.End();
        }

        public void StrokeLineStrip(params Vector2[] points)
        {
            StrokeLineStrip((IEnumerable<Vector2>)points);
        }

        private void DrawVertices(IEnumerable<Vector2> points)
        {
            foreach (Vector2 point in points)
                DrawVertex(point);
        }

        private void DrawVertices(IEnumerable<LineSegment> lineSegments, Vector2 position)
        {
            foreach (LineSegment lineSegment in lineSegments)
            {
                DrawVertex(lineSegment.EndPoint1 + position);
                DrawVertex(lineSegment.EndPoint2 + position);
            }
        }
        #endregion

        #region Text

        /// <summary>
        /// Printes text, using this context's defined font and color, to the view at the origin coordinates
        /// </summary>
        /// <param name="text">The text to print</param>
        public void Draw(string text)
        {
            Draw(text, new Vector2(0, 0));
        }

        /// <summary>
        /// Prints text, using this context's defined font and color, to the view at specified coordinates. 
        /// </summary>
        /// <param name="text">The <see cref="System.String"/> to print</param>
        /// <param name="position">The position at which to print the string</param>
        public void Draw(string text, Vector2 position)
        {
            Draw(new Text(text, font), position);
        }

        public void Draw(Text text)
        {
            Draw(text, Vector2.Zero);
        }

        public void Draw(Text text, Rectangle clippingRect)
        {
            Draw(text, Vector2.Zero, clippingRect);
        }

        public void Draw(Text text, Vector2 position)
        {
            Draw(text, position, text.Frame);
        }

        /// <summary>
        /// Draws a Text object inside a clipping rectangle.
        /// </summary>
        /// <remarks>Words or lines not fitting in the rectangle will be completely trimmed.</remarks>
        /// <param name="text">The <see cref="Text"/> object to draw</param>
        /// <param name="clippingRect">The rectangle clipping the text</param>
        public void Draw(Text text, Vector2 origin, Rectangle clippingRect)
        {
            GL.PushMatrix();
            GL.Translate(origin.X, origin.Y, 0);
            GL.Scale(1, -1, 1);
            RectangleF renderInto = new RectangleF(0, -clippingRect.Height, clippingRect.Width, clippingRect.Height);

            Color color = Color.FromArgb(fillColor.ByteA, fillColor.ByteR, fillColor.ByteG, fillColor.ByteB);

            // We could enable blending here when alpha < 255 but OpenTK doesn't support it :(.
            Text.defaultTextPrinter.Print(text.Value, text.Font, color, renderInto);

            GL.PopMatrix();
        }

        #endregion

        #region Textured
        public void Fill(Rectangle rectangle, Texture texture, Rectangle textureRectangle, ColorRgba tint)
        {
            Argument.EnsureNotNull(texture, "texture");

            FillColor = tint;
            CommitFillColor();

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

        /// <summary>
        /// Commits any changes to <see cref="FillColor"/> to OpenGL.
        /// </summary>
        private void CommitFillColor()
        {
            CommitColor(fillColor);
        }

        /// <summary>
        /// Commits any changes to <see cref="StrokeColor"/> to OpenGL.
        /// </summary>
        private void CommitStrokeColor()
        {
            CommitColor(strokeColor);
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
