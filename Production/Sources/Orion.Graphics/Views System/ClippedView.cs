using System;

using Orion.Geometry;

using OpenTK.Math;

namespace Orion.Graphics
{
	public abstract class ClippedView : View
	{
		public abstract Rectangle FullBounds { get; }
		
		public ClippedView (Rectangle frame)
			: base(frame)
		{ }
		
		public void Zoom(double factor)
		{
			Zoom(factor, Bounds.Center);
		}
		
		public void Zoom(double factor, Vector2 center)
		{
			// TODO: check for full bounds when zooming
			Vector2 scale = new Vector2((float)factor, (float)factor);
            Vector2 newSize = Bounds.Size;
            newSize.Scale(scale);

            Vector2 newOrigin = Bounds.Origin;
            newOrigin += Bounds.Size - newSize;

            Bounds = new Rectangle(newOrigin, newSize);
		}
		
		public void ScrollBy(double x, double y)
		{
			ScrollBy(new Vector2((float)x, (float)y));
		}
		
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
