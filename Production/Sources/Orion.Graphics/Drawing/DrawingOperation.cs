using System;

namespace Orion.Graphics.Drawing
{
    /// <summary>
    /// The RenderMode indicates how to render an <see cref="IDrawable"/> (by filling or stroking it).
    /// </summary>
	internal enum RenderMode
	{
		Fill, Stroke	
	}
	
    /// <summary>
    /// A DrawingOperation object encapsulates the data required to draw an
    /// <see cref="IDrawable"/> object in a GraphicsContext; that is, the object to be drawn,
    /// and the mode of drawing (fill or stroke).
    /// </summary>
	internal struct DrawingOperation
	{
        /// <summary>
        /// The object to draw
        /// </summary>
		public readonly IDrawable Operation;

        /// <summary>
        /// The drawing method to use
        /// </summary>
		public readonly RenderMode Mode;
		
        /// <summary>
        /// Creates a new drawing operation object that encapsulates an object to draw and a
        /// <see cref="RenderMode"/>.
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="mode"></param>
		public DrawingOperation (IDrawable operation, RenderMode mode)
		{
			Operation = operation;
			Mode = mode;
		}
	}
}
