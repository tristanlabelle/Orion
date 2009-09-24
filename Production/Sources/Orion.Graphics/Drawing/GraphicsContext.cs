using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Orion.Graphics;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Math;

namespace Orion.Graphics.Drawing
{
    public class GraphicsContext
    {
		private List<DrawingOperation> operations;
		
		/// <summary>
		/// The bounds of the local coordinates system. 
		/// </summary>
		public Rect CoordsSystem { get; set; }
		
		/// <summary>
		/// Constructs a GraphicsContext object with given bounds for its local coordinates system. 
		/// </summary>
		/// <param name="bounds">
		/// The <see cref="Rect"/> defining the local coordinates system
		/// </param>
		internal GraphicsContext(Rect bounds)
		{
			operations = new List<DrawingOperation>();
			CoordsSystem = bounds;
		}
		
		/// <summary>
		/// Instructs the graphics context to fill the shape when rendering time comes. 
		/// </summary>
		/// <param name="drawable">
		/// A <see cref="IDrawable"/> shape
		/// </param>
		public void Fill(IDrawable drawable)
		{
			operations.Add(new DrawingOperation(drawable, OperationMode.Fill));
		}
		
		/// <summary>
		/// Instructs the graphics context to stroke the edges of the shape when rendering time comes.
		/// </summary>
		/// <param name="drawable">
		/// A <see cref="IDrawable"/>
		/// </param>
		public void Stroke(IDrawable drawable)
		{
			operations.Add(new DrawingOperation(drawable, OperationMode.Stroke));
		}
		
		/// <summary>
		/// Effectively renders all the shapes the context has to the screen.
		/// </summary>
		/// <param name="system">
		/// A <see cref="Rect"/> indicating the global coordinates system in which to render the bounds
		/// </param>
		internal void DrawInto(Rect system)
		{
			SetUpContextMatrix(system);
			
			foreach(DrawingOperation operation in operations)
			{
				if(operation.mode == OperationMode.Fill)
					operation.Operation.Fill();
				else
					operation.Operation.Stroke();
			}
			
			RestoreContextMatrix();
		}
		
		/// <summary>
		/// Clears the rendering list. 
		/// </summary>
		internal void Clear()
		{
			operations.Clear();
		}
		
		private void SetUpContextMatrix(Rect system)
		{
			GL.PushMatrix();
			
			Vector2 positionCopy = CoordsSystem.Position;
			Vector2 baseSize = CoordsSystem.Size;
			baseSize.Add(ref positionCopy);
			Vector2 scale = new Vector2(CoordsSystem.Size.X / system.Size.X, CoordsSystem.Size.Y / system.Size.Y);
			
			GL.Translate(system.Position.X, system.Position.Y, 0);
			GL.Scale(scale.X, scale.Y, 1);
		}
		
		private void RestoreContextMatrix()
		{
			GL.PopMatrix();
		}
    }
}
