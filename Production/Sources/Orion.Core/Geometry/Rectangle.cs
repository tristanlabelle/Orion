using System;

using OpenTK.Math;

namespace Orion.Geometry
{
    /// <summary>
    /// Represents a rectangular shape using an origin vector (<see cref="P:Origin"/>)
    /// and a size vector (<see cref="P:Size"/>). Instances of this structure are immutable.
    /// </summary>
    [Serializable]
    public struct Rectangle
    {
        #region Instance
        #region Fields
        /// <summary>
        /// The position of the rectangle.
        /// </summary>
        /// <remarks>
        /// Encapsulated as <see cref="Vector2"/> is not immutable,
        /// and <c>readonly</c> sadly cannot change this fact.
        /// </remarks>
        private readonly Vector2 min;

        /// <summary>
        /// The size of the rectangle (X is the width, and Y is the height).
        /// </summary>
        /// <remarks>
        /// Encapsulated as <see cref="Vector2"/> is not immutable,
        /// and <c>readonly</c> sadly cannot change this fact.
        /// </remarks>
        private readonly Vector2 size;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructs a Rectangle object with a given width and height. The origin is set to zero.
        /// </summary>
        /// <param name="width">The width of the rect</param>
        /// <param name="height">The height of the rect</param>
        public Rectangle(float width, float height)
            : this(new Vector2(width, height))
        { }

        /// <summary>
        /// Constructs a Rectangle object with a given size. The origin is set to zero. 
        /// </summary>
        /// <param name="size">
        /// A <see cref="Vector2"/> representing the size of the rectangle
        /// </param>
        public Rectangle(Vector2 size)
            : this(new Vector2(0, 0), size)
        { }

        /// <summary>
        /// Constructs a Rectangle object with a given X and Y origin, and Width and Height parameters.
        /// </summary>
        /// <param name="x">
        /// A <see cref="System.Single"/> specifying the abscissa of the origin
        /// </param>
        /// <param name="y">
        /// A <see cref="System.Single"/> specifying the ordinate of the origin
        /// </param>
        /// <param name="width">
        /// A <see cref="System.Single"/> specifying the width of the rectangle
        /// </param>
        /// <param name="height">
        /// A <see cref="System.Single"/> specifying the height of the rectangle
        /// </param>
        public Rectangle(float x, float y, float width, float height)
            : this(new Vector2(x, y), new Vector2(width, height))
        { }

        /// <summary>
        /// Constructs a Rectangle object with a given origin and size.
        /// </summary>
        /// <param name="position">
        /// A <see cref="Vector2"/> representing the origin of the rectangle
        /// </param>
        /// <param name="size">
        /// A <see cref="Vector2"/> representing the size of the rectangle
        /// </param>
        public Rectangle(Vector2 position, Vector2 size)
        {
            // size must never be negative! this would break so many things.
            // the origin must always be the bottom left corner; reajust rectangles so they match this rule if necessary
            if (size.X < 0)
            {
                position.X += size.X;
                size.X *= -1;
            }

            if (size.Y < 0)
            {
                position.Y += size.Y;
                size.Y *= -1;
            }

            this.size = size;
            this.min = position;
        }
        #endregion

        #region Properties
        #region Coordinates
        #region Minimum
        /// <summary>
        /// Gets the point with the minimum coordinates of this <see cref="Rectangle"/>.
        /// </summary>
        public Vector2 Min
        {
            get { return min; }
        }

        /// <summary>
        /// Gets the minimum x coordinate of this <see cref="Rectangle"/>.
        /// </summary>
        public float MinX
        {
            get { return min.X; }
        }

        /// <summary>
        /// Gets the minimum y coordinate of this <see cref="Rectangle"/>.
        /// </summary>
        public float MinY
        {
            get { return min.Y; }
        }
        #endregion

        #region Center
        /// <summary>
        /// Gets the coordinates of the center of this <see cref="Rectangle"/>.
        /// </summary>
        public Vector2 Center
        {
            get { return min + Extent; }
        }

        /// <summary>
        /// Gets the X coordinate of the center of this <see cref="Rectangle"/>.
        /// </summary>
        public float CenterX
        {
            get { return MinX + Width * 0.5f; }
        }

        /// <summary>
        /// Gets the Y coordinate of the center of this <see cref="Rectangle"/>.
        /// </summary>
        public float CenterY
        {
            get { return MinY + Height * 0.5f; }
        }
        #endregion

        #region Maximum
        /// <summary>
        /// Gets the coordinates of the corner of this <see cref="Rectangle"/> at the opposite of its origin.
        /// </summary>
        public Vector2 Max
        {
            get { return min + size; }
        }

        /// <summary>
        /// The second X coordinate of this <see cref="Rectangle"/> (origin's abscissa plus width).
        /// </summary>
        public float MaxX
        {
            get { return MinX + Width; }
        }

        /// <summary>
        /// The second Y coordinate of this <see cref="Rectangle"/> (origin's ordinate plus height).
        /// </summary>
        public float MaxY
        {
            get { return MinY + Height; }
        }
        #endregion
        #endregion

        #region Size
        /// <summary>
        /// Gets the width of this <see cref="Rectangle"/>.
        /// </summary>
        public float Width
        {
            get { return size.X; }
        }

        /// <summary>
        /// Gets the height of this <see cref="Rectangle"/>.
        /// </summary>
        public float Height
        {
            get { return size.Y; }
        }

        /// <summary>
        /// Gets the size of this <see cref="Rectangle"/>.
        /// </summary>
        public Vector2 Size
        {
            get { return size; }
        } 

        /// <summary>
        /// Gets the extent (half-size vector) of this <see cref="Rectangle"/>.
        /// </summary>
        public Vector2 Extent
        {
            get { return size * 0.5f; }
        }
        #endregion
        #endregion

        #region Methods
        #region Public
        #region Hit Testing
        /// <summary>
        /// Indicates if the rectangle contains a point.
        /// </summary>
        /// <param name="point">
        /// A <see cref="OpenTK.Math.Vector2"/> indicating the point's coordinates
        /// </param>
        /// <returns>
        /// A <see cref="System.Boolean"/>; true if the rect contains the point, false otherwise
        /// </returns>
        public bool ContainsPoint(Vector2 point)
        {
            return point.X >= MinX && point.X <= MaxX
                && point.Y >= MinY && point.Y <= MaxY;
        }
        #endregion

        #region ClosestPointInside
        /// <summary>
        /// Gets a point within this <see cref="Rectangle"/> that is the closest to a given point.
        /// </summary>
        /// <param name="point">The point which's closest image is to be found.</param>
        /// <returns>The closest image of that point within this <see cref="Rectangle"/>.</returns>
        public Vector2 ClosestPointInside(Vector2 point)
        {
            if (point.X < MinX) point.X = MinX;
            else if (point.X > MaxX) point.X = MaxX;

            if (point.Y < MinY) point.Y = MinY;
            else if (point.Y > MaxY) point.Y = MaxY;

            return point;
        }
        #endregion

        #region Coordinate Systems
        /// <summary>
        /// Converts a point from the coordinate system in which this <see cref="Rectangle"/>
        /// is defined to the local coordinate system defined by the bounds of this
        /// <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="point">
        /// A <see cref="Point"/> in the coordinate system in which this <see cref="Rectangle"/> is defined.
        /// </param>
        /// <returns>
        /// The corresponding point in the local coordinate system
        /// defined by the bounds of this <see cref="Rectangle"/>.
        /// </returns>
        public Vector2 ParentToLocal(Vector2 point)
        {
            return new Vector2((point.X - MinX) / Width, (point.Y - MinY) / Height);
        }

        /// <summary>
        /// Converts a point from the local coordinate system defined
        /// by the bounds of this <see cref="Rectangle"/> to the
        /// coordinate system in which this <see cref="Rectangle"/> is defined.
        /// </summary>
        /// <param name="point">
        /// A point in the local coordinate system defined
        /// by the bounds of this <see cref="Rectangle"/>.
        /// </param>
        /// <returns>
        /// The corresponding point in the coordinate system
        /// in which this <see cref="Rectangle"/> is defined.
        /// </returns>
        public Vector2 LocalToParent(Vector2 point)
        {
            return new Vector2(point.X * Width + MinX, point.Y * Height + MinY);
        }
        #endregion
        
        #region Translation
        /// <summary>
        /// Returns a new rectangle translated by the specified units.
        /// </summary>
        /// <param name="x">
        /// A <see cref="System.Single"/> representing the move along the X axis
        /// </param>
        /// <param name="y">
        /// A <see cref="System.Single"/> representing the move along the Y axis
        /// </param>
        /// <returns>
        /// A new <see cref="Rectangle"/> based on this one whose origin is translated by the specified units
        /// </returns>
        public Rectangle Translate(float x, float y)
        {
            return Translate(new Vector2(x, y));
        }
        
        /// <summary>
        /// Returns a new rectangle translated by the specified vector.
        /// </summary>
        /// <param name="direction">
        /// A <see cref="Vector2"/> specifying the direction of the translation
        /// </param>
        /// <returns>
        /// A new <see cref="Rectangle"/> based on this one, whose origin is translated by the specified vector
        /// </returns>
        public Rectangle Translate(Vector2 direction)
        {
            return TranslateTo(min + direction);
        }
        
        /// <summary>
        /// Creates a new rectangle whose abscissa is translated by the specified units
        /// </summary>
        /// <param name="x">
        /// A <see cref="System.Single"/> indicating the move along the X axis
        /// </param>
        /// <returns>
        /// A new <see cref="Rectangle"/> based on this one, whose origin abscissa is translated by the specified units
        /// </returns>
        public Rectangle TranslateX(float x)
        {
            return Translate(x, 0);
        }
        
        /// <summary>
        /// Creates a new rectangle whose ordinate is translated by the specified units
        /// </summary>
        /// <param name="y">
        /// A <see cref="System.Single"/> representing the move along the Y axis
        /// </param>
        /// <returns>
        /// A new <see cref="Rectangle"/> based on this one, whose origin ordinate is translated by the specified units 
        /// </returns>
        public Rectangle TranslateY(float y)
        {
            return Translate(0, y);
        }
        
        /// <summary>
        /// Creates a new rectangle at a new origin
        /// </summary>
        /// <param name="x">
        /// A <see cref="System.Single"/> indicating the origin abscissa
        /// </param>
        /// <param name="y">
        /// A <see cref="System.Single"/> indicating the origin ordinate
        /// </param>
        /// <returns>
        /// A new <see cref="Rectangle"/> with the size of this one but the specified origin
        /// </returns>
        public Rectangle TranslateTo(float x, float y)
        {
            return TranslateTo(new Vector2(x, y));
        }
        
        /// <summary>
        /// Creates a new rectangle at a new origin
        /// </summary>
        /// <param name="origin">
        /// A <see cref="Vector2"/> representing the new origin
        /// </param>
        /// <returns>
        /// A new <see cref="Rectangle"/> with the same size as this one but the specified origin
        /// </returns>
        public Rectangle TranslateTo(Vector2 origin)
        {
            return new Rectangle(origin, size);
        }
        #endregion
        
        #region Resizing
        /// <summary>
        /// Creates a new rectangle resized by the specified values.
        /// </summary>
        /// <param name="width">
        /// A <see cref="System.Single"/> by which the width of the rectangle must be increased or decreased
        /// </param>
        /// <param name="height">
        /// A <see cref="System.Single"/> by which the height of the rectangle must be increased or decreased
        /// </param>
        /// <returns>
        /// A new <see cref="Rectangle"/> with the modified size
        /// </returns>
        public Rectangle Resize(float width, float height)
        {
            return Resize(new Vector2(width, height));
        }
        
        /// <summary>
        /// Creates a new rectangle resized by the vector.
        /// </summary>
        /// <param name="sizeChange">
        /// A <see cref="Vector2"/> specifying the width and height increments or decrements to be applied to the current rectangle
        /// </param>
        /// <returns>
        /// A new <see cref="Rectangle"/> with the modified size
        /// </returns>
        public Rectangle Resize(Vector2 sizeChange)
        {
            return ResizeTo(size + sizeChange);
        }
        
        /// <summary>
        /// Creates a new rectangle with the width resized by a specified value
        /// </summary>
        /// <param name="width">
        /// A <see cref="System.Single"/> representing the width increment or decrement to the current size
        /// </param>
        /// <returns>
        /// A new <see cref="Rectangle"/> with the modified width
        /// </returns>
        public Rectangle ResizeWidth(float width)
        {
            return Resize(width, 0);
        }
        
        /// <summary>
        /// Creates a new rectangle with the height resized by a specified value
        /// </summary>
        /// <param name="height">
        /// A <see cref="System.Single"/> representing the height increment or decrement to apply to the current size
        /// </param>
        /// <returns>
        /// A new <see cref="Rectangle"/> with the modified height
        /// </returns>
        public Rectangle ResizeHeight(float height)
        {
            return Resize(0, height);
        }
        
        /// <summary>
        /// Creates a new rectangle at the same origin but a different size
        /// </summary>
        /// <param name="width">
        /// A <see cref="System.Single"/> to determine the width of the new rectangle
        /// </param>
        /// <param name="height">
        /// A <see cref="System.Single"/> indicating the height of the new rectangle
        /// </param>
        /// <returns>
        /// A new <see cref="Rectangle"/> at the same origin but with a different size
        /// </returns>
        public Rectangle ResizeTo(float width, float height)
        {
            return ResizeTo(new Vector2(width, height));
        }
        
        /// <summary>
        /// Creates a new rectangle at the same origin but a different size
        /// </summary>
        /// <param name="newSize">
        /// A <see cref="Vector2"/> specifying the size of the new rectangle
        /// </param>
        /// <returns>
        /// A new <see cref="Rectangle"/> with the same origin as this rectangle's one but a different size
        /// </returns>
        public Rectangle ResizeTo(Vector2 newSize)
        {
            return new Rectangle(min, newSize);
        }
        #endregion

        #region Object Model
        /// <summary>
        /// Gets a string representation of this <see cref="Rectangle"/>.
        /// </summary>
        /// <returns>A string representation of this <see cref="Rectangle"/> with the form {{X,Y}, WxH}.</returns>
        public override string ToString()
        {
            return string.Format("{{{0}, {1}x{2}}}", min, size.X, size.Y);
        }
        #endregion
        #endregion
        #endregion
        #endregion

        #region Static
        #region Fields
        /// <summary>
        /// An empty rectangle (origin, width and height are all zeroed).
        /// </summary>
        public static readonly Rectangle Empty = new Rectangle(0, 0, 0, 0);
        #endregion

        #region Methods
        #region Factory
        public static Rectangle FromMinMax(float minX, float minY, float maxX, float maxY)
        {
            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }

        public static Rectangle FromMinMax(Vector2 min, Vector2 max)
        {
            return new Rectangle(min, max - min);
        }

        /// <summary>
        /// Creates a new <see cref="Rectangle"/> from its center and extent.
        /// </summary>
        /// <param name="center">The position of the <see cref="Rectangle"/>'s center.</param>
        /// <param name="extent">The half-size vector of the <see cref="Rectangle"/>.</param>
        /// <returns>The resulting <see cref="Rectangle"/>.</returns>
        public static Rectangle FromCenterExtent(Vector2 center, Vector2 extent)
        {
            return new Rectangle(center - extent, center + extent);
        }

        /// <summary>
        /// Creates a new <see cref="Rectangle"/> defined by two points.
        /// </summary>
        /// <param name="point1">The first point.</param>
        /// <param name="point2">The second point.</param>
        /// <returns>A <see cref="Rectangle"/> containing both points.</returns>
        public static Rectangle FromPoints(Vector2 point1, Vector2 point2)
        {
            return new Rectangle(point1, point2 - point1);
        }
        #endregion

        public static Vector2 ConvertPoint(Rectangle from, Rectangle to, Vector2 point)
        {
            point -= from.Min;
            point.Scale(to.Width / from.Width, to.Height / from.Height);
            return point + to.Min;
        }

        #region Boolean operations
        /// <summary>
        /// Computes the rectangle formed by this intersection of two rectangles.
        /// </summary>
        /// <param name="a">The first rectangle.</param>
        /// <param name="b">The second rectangle.</param>
        /// <param name="result">Outputs the intersection rectangle, negative if they do not intersect.</param>
        /// <returns>The intersection of the rectangles, or <c>null</c> if they do not intersect.</returns>
        public static Rectangle? Intersection(Rectangle a, Rectangle b)
        {
            Rectangle result = Rectangle.FromMinMax(
                Math.Max(a.MinX, b.MinX), Math.Max(a.MinY, b.MinY),
                Math.Min(a.MaxX, b.MaxX), Math.Min(a.MaxX, b.MaxY));

            if (result.Extent.X < 0.0f || result.Extent.Y < 0.0f)
                return null;

            return result;
        }

        public static bool Instersects(Rectangle a, Rectangle b)
        {
            return Intersection(a, b).HasValue;
        }

        public static Rectangle Union(Rectangle a, Rectangle b)
        {
            return new Rectangle(
                Math.Min(a.MinX, b.MinX), Math.Min(a.MinY, b.MinY),
                Math.Max(a.MaxX, b.MaxX), Math.Max(a.MaxY, b.MaxY));
        }
        #endregion
        #endregion
        #endregion
    }
}
