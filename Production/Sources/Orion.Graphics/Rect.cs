using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Math;

namespace Orion.Graphics
{
    /// <summary>
    /// A Rect instance encapsulates an origin vector (<see cref="Rect.X"/> and <see cref="Rect.Y"/>)
    /// and a size vector (<see cref="Rect.Width"/> and <see cref="Rect.Height"/>). They can be
    /// intersected, translated and resized.
    /// </summary>
	public struct Rect
	{
		#region Fields
		#region Static
		
		/// <summary>
		/// An empty rectangle (origin, width and height are all zeroed).
		/// </summary>
		public static readonly Rect EmptyRect = new Rect(0, 0, 0, 0);
		#endregion
		
		/// <summary>
		/// The position of the rectangle
		/// </summary>
		public readonly Vector2 Position;

		/// <summary>
		/// The size of the rectangle (X is the width, and Y is the height)
		/// </summary>
		public readonly Vector2 Size;
		#endregion
		
		#region Propeties
		
		/// <summary>
		/// The origin abscissa of the rectangle
		/// </summary>
		public float X { get { return Position.X; } }
		/// <summary>
		/// The origin ordinate of the rectangle
		/// </summary>
		public float Y { get { return Position.Y; } }
		/// <summary>
		/// The width of the rectangle
		/// </summary>
		public float Width { get { return Size.X; } }
		/// <summary>
		/// The height of the rectangle
		/// </summary>
		public float Height { get { return Size.Y; } }
		
		/// <summary>
		/// The second X coordinate of the rectangle (origin's abscissa plus width) 
		/// </summary>
		public float MaxX { get { return X + Width; } }
		
		/// <summary>
		/// The second Y coordinate of the rectangle (origin's ordinate plus height) 
		/// </summary>
		public float MaxY { get { return Y + Height; } }
		#endregion
		
		#region Public Methods
		#region Constructors

        /// <summary>
        /// Constructs a Rect object with a given width and height. The origin is set to zero.
        /// </summary>
        /// <param name="width">The width of the rect</param>
        /// <param name="height">The height of the rect</param>
        public Rect(float width, float height)
            : this(new Vector2(width, height))
        { }
		
		/// <summary>
		/// Constructs a Rect object with a given size. The origin is set to zero. 
		/// </summary>
		/// <param name="size">
		/// A <see cref="Vector2"/> representing the size of the rectangle
		/// </param>
		public Rect(Vector2 size)
			: this(new Vector2(0, 0), size)
		{ }

		/// <summary>
		/// Constructs a Rect object with a given X and Y origin, and Width and Height parameters.
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
		public Rect(float x, float y, float width, float height)
			: this(new Vector2(x, y), new Vector2(width, height))
		{ }
		
		/// <summary>
		/// Constructs a Rect object with a given origin and size.
		/// </summary>
		/// <param name="position">
		/// A <see cref="Vector2"/> representing the origin of the rectangle
		/// </param>
		/// <param name="size">
		/// A <see cref="Vector2"/> representing the size of the rectangle
		/// </param>
		public Rect(Vector2 position, Vector2 size)
		{
			Size = size;
			Position = position;
			
			// size must never be negative! this would break so many things.
			// the origin must always be the bottom left corner; reajust rectangles so they match this rule if necessary
			if(Size.X < 0)
			{
				Position.X += Size.X;
				Size.X *= -1;
			}
			
			if(Size.Y < 0)
			{
				Position.Y += Size.Y;
				Size.Y *= -1;
			}
		}
		#endregion
		
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
			return point.X.IsBetween(X, MaxX) && point.Y.IsBetween(Y, MaxY);
		}
		
		#endregion
		
		#region Intersecting
		
		/// <summary>
		/// Indicates if this rectangle intersects with another one.
		/// </summary>
		/// <param name="otherRect">
		/// The <see cref="Rect"/> we want to test for intersection
		/// </param>
		/// <returns>
		/// true if the rectangle intersects with this one; false otherwise
		/// </returns>
		public bool Intersects(Rect otherRect)
		{
			return ContainsPoint(otherRect.Position) || otherRect.ContainsPoint(this.Position);
		}
		
		/// <summary>
		/// Returns the intersection of two rectangles
		/// </summary>
		/// <param name="otherRect">
		/// The <see cref="Rect"/> with which we want this rectangle to intersect
		/// </param>
		/// <returns>
		/// The intersection <see cref="Rect"/> of both rectangles, or <see cref="Rect.EmptyRect"/> if they don't intersect
		/// </returns>
		public Rect Intersection(Rect otherRect)
		{
			if(ContainsPoint(otherRect.Position))
			{
				return OnewayIntersection(otherRect);
			}
			if(otherRect.ContainsPoint(this.Position))
			{
				return otherRect.OnewayIntersection(this);
			}
			return EmptyRect;
		}
		#endregion
		
		#region Translating
		
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
		/// A new <see cref="Rect"/> based on this one whose origin is translated by the specified units
		/// </returns>
		public Rect Translate(float x, float y)
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
		/// A new <see cref="Rect"/> based on this one, whose origin is translated by the specified vector
		/// </returns>
		public Rect Translate(Vector2 direction)
		{
			return TranslateTo(Position + direction);
		}
		
		/// <summary>
		/// Creates a new rectangle whose abscissa is translated by the specified units
		/// </summary>
		/// <param name="x">
		/// A <see cref="System.Single"/> indicating the move along the X axis
		/// </param>
		/// <returns>
		/// A new <see cref="Rect"/> based on this one, whose origin abscissa is translated by the specified units
		/// </returns>
		public Rect TranslateX(float x)
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
		/// A new <see cref="Rect"/> based on this one, whose origin ordinate is translated by the specified units 
		/// </returns>
		public Rect TranslateY(float y)
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
		/// A new <see cref="Rect"/> with the size of this one but the specified origin
		/// </returns>
		public Rect TranslateTo(float x, float y)
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
		/// A new <see cref="Rect"/> with the same size as this one but the specified origin
		/// </returns>
		public Rect TranslateTo(Vector2 origin)
		{
			return new Rect(origin, Size);
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
		/// A new <see cref="Rect"/> with the modified size
		/// </returns>
		public Rect Resize(float width, float height)
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
		/// A new <see cref="Rect"/> with the modified size
		/// </returns>
		public Rect Resize(Vector2 sizeChange)
		{
			return ResizeTo(Size + sizeChange);
		}
		
		/// <summary>
		/// Creates a new rectangle with the width resized by a specified value
		/// </summary>
		/// <param name="width">
		/// A <see cref="System.Single"/> representing the width increment or decrement to the current size
		/// </param>
		/// <returns>
		/// A new <see cref="Rect"/> with the modified width
		/// </returns>
		public Rect ResizeWidth(float width)
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
		/// A new <see cref="Rect"/> with the modified height
		/// </returns>
		public Rect ResizeHeight(float height)
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
		/// A new <see cref="Rect"/> at the same origin but with a different size
		/// </returns>
		public Rect ResizeTo(float width, float height)
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
		/// A new <see cref="Rect"/> with the same origin as this rectangle's one but a different size
		/// </returns>
		public Rect ResizeTo(Vector2 newSize)
		{
			return new Rect(Position, newSize);
		}
		#endregion

        #region Overrides

        /// <summary>
        /// Returns a textual representation of the <see cref="Rect"/> with the form {{x,y}, {w,h}}.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{{{0}, {1}}}", Position, Size);
        }
        #endregion
        #endregion

        #region Private Methods
		
		private Rect OnewayIntersection(Rect otherRect)
		{
			return otherRect.ResizeTo(otherRect.Size + Position - otherRect.Position);
		}
		
		#endregion
	}
	
	internal static class RangeChecking
	{
		/// <summary>
		/// Checks if a float is within the [min, max[ interval.
		/// </summary>
		/// <param name="val">
		/// This <see cref="System.Single"/>
		/// </param>
		/// <param name="min">
		/// The (inclusive) minimum value
		/// </param>
		/// <param name="max">
		/// The (non-inclusive) maximum value
		/// </param>
		/// <returns>
		/// True if the float is in the [min, max[ range; false otherwise
		/// </returns>
		public static bool IsBetween(this float val, float min, float max)
		{
			return val >= min && val < max; 
		}
	}
}
