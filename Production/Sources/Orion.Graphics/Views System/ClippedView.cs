using System;

using Orion.Geometry;

using OpenTK.Math;

namespace Orion.Graphics
{
    /// <summary>
    /// A ClippedView is a view whose full contents is not meant to be visible at once, and thus needs to be scrolled in some way.
    /// </summary>
	public abstract class ClippedView : View
	{
        /// <summary>
        /// Returns the maximum bounds of this object (in contrast to Bounds, which are the visible bounds of this object).
        /// </summary>
		public abstract Rectangle FullBounds { get; }
        private Vector2 BoundsLimitOuter = new Vector2 (100,75);
        private Vector2 BoundsLimitInner = new Vector2 (10, 7); 

        

		
        /// <summary>
        /// Constructs a new ClippedView.
        /// </summary>
        /// <param name="frame">The frame of the view</param>
		public ClippedView (Rectangle frame)
			: base(frame)
		{ }
		
        /// <summary>
        /// Scales the view with a given ratio, revealing a greater or lesser area of the full bounds.
        /// </summary>
        /// <param name="factor">The factor by which to scale the view</param>
		public void Zoom(double factor)
		{
			Zoom(factor, Bounds.Center);
		}

        /// <summary>
        /// Scales the view with a given ratio, revealing a greater or lesser area of the full bounds.
        /// </summary>
        /// <param name="factor">The factor by which to scale the view</param>
        /// <param name="center">The point around which to scale</param>
		public void Zoom(double factor, Vector2 center)
		{
			// TODO: check for full bounds when zooming so we don't break the aspect ratio
			Vector2 scale = new Vector2((float)factor, (float)factor);// Détermine la grosseur du scale. 
			Vector2 newSize = Bounds.Size; // Créer une nouvelle variable de grosseur
            Vector2 newOrigin = Bounds.Origin;
            if (FullBounds.Size.X + 10  >= Bounds.Size.X && FullBounds.Size.Y + 10 >= Bounds.Size.Y)
            {
                newSize.Scale(scale);// Appliquele nouveau scale, et créer la nouvelle grosseur. 
                if (newSize.X > BoundsLimitOuter.X)
                    newSize.X = BoundsLimitOuter.X;
                if (newSize.Y > BoundsLimitOuter.Y)
                    newSize.Y = BoundsLimitOuter.Y;
            }
            Rectangle newBounds = new Rectangle(newOrigin, newSize);

            Vector2 difference = Bounds.Center - newBounds.Center;
            difference.Scale(0.5f, 0.5f);
            newOrigin += difference;
            difference.X= Math.Abs(difference.X);
            difference.Y =Math.Abs(difference.Y);
            if (scale.X > 1 || scale.Y > 1)
            {
                difference.X = difference.X * -1;
                difference.Y = difference.Y * -1; 
            }
            if (Bounds.Origin.X > 0)
            {
                newOrigin.X += difference.X;
                if (newOrigin.X < 0)
                    newOrigin.X = 0;

            }
            if (Bounds.Origin.Y > 0)
            {
                newOrigin.Y += difference.Y;
                if (newOrigin.Y < 0)
                    newOrigin.Y = 0;
            }
            
               


            Bounds = new Rectangle(newOrigin, newSize);
            
		}
        /// <summary>
        /// Translates the bounds origin by given X and Y offsets. Checks the bounds so it's impossible to scroll past them.
        /// </summary>
        /// <param name="x">The abscissa offset to apply</param>
        /// <param name="y">The ordinates offset to apply</param>
		public void ScrollBy(double x, double y)
		{
			ScrollBy(new Vector2((float)x, (float)y));
		}

        /// <summary>
        /// Translates the bounds origin by a given vector. Checks the bounds so it's impossible to scroll past them.
        /// </summary>
        /// <param name="direction">The vector to apply to the bounds origin</param>
		public void ScrollBy(Vector2 direction)
		{
			Rectangle newBounds = Bounds.Translate(direction);
			Vector2 newOrigin = newBounds.Origin;
			Vector2 newSize = newBounds.Size;
			
			if(newOrigin.X < FullBounds.X)
				newOrigin.X = FullBounds.X;
			
			if(newOrigin.Y < FullBounds.Y)
				newOrigin.Y = FullBounds.Y;
			
			if(newBounds.MaxX > FullBounds.MaxX)
				newOrigin.X -= newBounds.MaxX - FullBounds.MaxX;
			
			if(newBounds.MaxY > FullBounds.MaxY)
				newOrigin.Y -= newBounds.MaxY - FullBounds.MaxY;
			
			Bounds = new Rectangle(newOrigin, newSize);
		}
	}
}
