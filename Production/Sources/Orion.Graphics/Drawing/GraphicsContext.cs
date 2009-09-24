using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Math;

namespace Orion.Graphics.Drawing
{
    class GraphicsContext
    {
		private List<DrawingOperation> operations;
		
		public Rect coordsSystem { get; private set; }
		
		public GraphicsContext(Rect bounds)
		{
			coordsSystem = bounds;
		}
		
		private void SetUpContextMatrix(Rect system)
		{
			GL.PushMatrix();
			
			Vector2 positionCopy = coordsSystem.Position;
			Vector2 baseSize = coordsSystem.Size;
			baseSize.Add(ref positionCopy);
			Vector2 scale = new Vector2(coordsSystem.Size.X / system.Size.X, coordsSystem.Size.Y / system.Size.Y);
			
			GL.Translate(system.Position.X, system.Position.Y, 0);
			GL.Scale(scale.X, scale.Y, 1);
		}
		
		private void RestoreContextMatrix()
		{
			GL.PopMatrix();
		}
		
		public void DrawInto(Rect system)
		{
			SetUpContextMatrix(system);
			
			foreach(DrawingOperation operation in operations)
			
			RestoreContextMatrix();
		}
    }
}
