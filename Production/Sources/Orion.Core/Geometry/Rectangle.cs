using System;

using OpenTK;
using OpenTK.Graphics;
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
        private readonly Vector2 origin;

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
            this.origin = position;
        }
        #endregion

        #region Properties
        #region Coordinates
        /// <summary>
        /// Gets the origin of this <see cref="Rectangle"/>.
        /// </summary>
        public Vector2 Origin
        {
            get { return origin; }
        }

        /// <summary>
        /// Gets the origin abscissa of the rectangle.
        /// </summary>
        public float X
        {
            get { return origin.X; }
        }

        /// <summary>
        /// Gets the origin ordinate of the rectangle.
        /// </summary>
        public float Y
        {
            get { return origin.Y; }
        }

        /// <summary>
        /// Gets the coordinates of the center of this <see cref="Rectangle"/>.
        /// </summary>
        public Vector2 Center
        {
            get { return origin + Extent; }
        }

        /// <summary>
        /// Gets the coordinates of the corner of this <see cref="Rectangle"/> at the opposite of its origin.
        /// </summary>
        public Vector2 Max
        {
            get { return origin + size; }
        }

        /// <summary>
        /// The second X coordinate of this <see cref="Rectangle"/> (origin's abscissa plus width).
        /// </summary>
        public float MaxX
        {
            get { return X + Width; }
        }

        /// <summary>
        /// The second Y coordinate of this <see cref="Rectangle"/> (origin's ordinate plus height).
        /// </summary>
        public float MaxY
        {
            get { return Y + Height; }
        }
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
        /// <param name="x">
        /// A <see cref="System.Single"/> for the point's abscissa
        /// </param>
        /// <param name="y">
        /// A <see cref="System.Single"/> for the point's ordinate
        /// </param>
        /// <returns>
        /// A <see cref="System.Boolean"/>; true if the rect conains the point, false otherwise
        /// </returns>
        public bool ContainsPoint(float x, float y)
        {
            return ContainsPoint(new Vector2(x, y));
        }
        
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
            return point.X >= X && point.X <= MaxX
                && point.Y >= Y && point.Y <= MaxY;
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
            return new Vector2((point.X - X) / Width, (point.Y - Y) / Height);
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
            return new Vector2(point.X * Width + X, point.Y * Height + Y);
        }
        #endregion

        #region Intersection
        /// <summary>
        /// Indicates if this rectangle intersects with another one.
        /// </summary>
        /// <param name="otherRect">
        /// The <see cref="Rectangle"/> we want to test for intersection
        /// </param>
        /// <returns>
        /// true if the rectangle intersects with this one; false otherwise
        /// </returns>
        public bool Intersects(Rectangle otherRect)
        {
            return ContainsPoint(otherRect.origin) || otherRect.ContainsPoint(this.origin);
        }
        
        /// <summary>
        /// Returns the intersection of two rectangles
        /// </summary>
        /// <param name="otherRect">
        /// The <see cref="Rectangle"/> with which we want this rectangle to intersect
        /// </param>
        /// <returns>
        /// The intersection <see cref="Rectangle"/> of both rectangles,
        /// or <c>null</c> if they do not intersect.
        /// </returns>
        public Rectangle? Intersection(Rectangle otherRect)
        {
            if (ContainsPoint(otherRect.origin))
                return OnewayIntersection(otherRect);
            
            if (otherRect.ContainsPoint(this.origin))
                return otherRect.OnewayIntersection(this);

            return null;
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
            return TranslateTo(origin + direction);
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
            return new Rectangle(origin, newSize);
        }
        #endregion

        #region Object Model
        /// <summary>
        /// Gets a string representation of this <see cref="Rectangle"/>.
        /// </summary>
        /// <returns>A string representation of this <see cref="Rectangle"/> with the form {{X,Y}, WxH}.</returns>
        public override string ToString()
        {
            return string.Format("{{{0}, {1}x{2}}}", origin, size.X, size.Y);
        }
        #endregion
        #endregion

        #region Private
        private Rectangle OnewayIntersection(Rectangle otherRect)
        {
            return otherRect.ResizeTo(otherRect.size + origin - otherRect.origin);
        }
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
        #endregion
    }
}
