using System;

namespace Orion.Graphics.Drawing
{
	internal enum OperationMode
	{
		Fill, Stroke	
	}
	
	internal struct DrawingOperation
	{
		public IDrawable Operation;
		public OperationMode mode;
		
		public DrawingOperation (IDrawable operation, OperationMode mode)
		{
			Operation = operation;
			OperationMode = mode;
		}
	}
}
