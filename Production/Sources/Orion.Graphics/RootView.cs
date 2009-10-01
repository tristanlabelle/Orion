using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK.Graphics;
using OpenTK.Math;

using Orion.Geometry;

using Color = System.Drawing.Color;

namespace Orion.Graphics
{
    /// <summary>
    /// A RootView is a base container. It does no drawing on its own.
    /// </summary>
    class RootView : Responder
    {
		private Rectangle frame;
		private readonly List<View> children;
		
        public override Rectangle Frame
        {
            get { return frame; }
            set
            {
                frame = value;
                ResetViewport();
            }
        }
		
		public override Rectangle Bounds { get; set; }
		
		public List<View> Children { get { return children; } }

        /// <summary>
        /// Creates a RootView object with a frame and bounds.
        /// </summary>
        /// <param name="frame">The frame of the view</param>
        /// <param name="bounds">The bounds of the view</param>
        public RootView(Rectangle frame, Rectangle bounds)
        {
            Bounds = bounds;
			Frame = frame;
			
			children = new List<View>();
        }
        
        protected internal override bool PropagateMouseEvent(MouseEventType eventType, MouseEventArgs args)
        {
            Matrix4 transformMatrix = Matrix4.Scale(Bounds.Width / Frame.Width, Bounds.Height / Frame.Height, 1);
            Vector2 coords = Vector4.Transform(new Vector4(args.X, args.Y, 0, 1), transformMatrix).Xy;
            
            args = new MouseEventArgs(coords.X, coords.Y, args.ButtonPressed, args.Clicks);
            
            foreach(View child in Enumerable.Reverse(children))
            {
                if(child.Frame.ContainsPoint(coords))
                {
                    child.PropagateMouseEvent(eventType, args);
                }
            }
            
            return false;
        }
		
		protected internal override bool PropagateKeyboardEvent(KeyboardEventType type, KeyboardEventArgs args)
		{
			// for now, just propagate keyboard events to everyone, since more precise handling will require a focus system
			// and we don't need that until we have UI widgets
			foreach(View child in children)
			{
				child.DispatchKeyboardEvent(type, args);
			}
			
			return DispatchKeyboardEvent(type, args);
		}

        internal void Render()
        {
            GL.ClearColor(Color.ForestGreen); // we all love forest green!
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.LoadIdentity();

            foreach (View child in Children)
                child.Render();
        }

        private void ResetViewport()
        {
            Rectangle bounds = Bounds;
            GL.Viewport(0, 0, (int)Frame.Size.X, (int)Frame.Size.Y);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(bounds.X, bounds.Width, bounds.Y, bounds.Height, -1, 1);
            GL.MatrixMode(MatrixMode.Modelview);
        }
    }
}
