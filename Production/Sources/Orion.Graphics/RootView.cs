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
    class RootView : View
    {
        public override Rectangle Frame
        {
            get { return base.Frame; }
            set
            {
                base.Frame = value;
                ResetViewport();
            }
        }

        /// <summary>
        /// Creates a RootView object with a frame and bounds.
        /// </summary>
        /// <param name="frame">The frame of the view</param>
        /// <param name="bounds">The bounds of the view</param>
        public RootView(Rectangle frame, Rectangle bounds)
            : base(frame)
        {
            Bounds = bounds;
            ResetViewport();
        }
        
        internal override bool PropagateMouseEvent(MouseEventType eventType, MouseEventArgs args)
        {
            Matrix4 transformMatrix = Matrix4.Scale(Bounds.Width / Frame.Width, Bounds.Height / Frame.Height, 1);
            Vector2 coords = Vector4.Transform(new Vector4(args.X, args.Y, 0, 1), transformMatrix).Xy;
            
            args = new MouseEventArgs(coords.X, coords.Y, args.ButtonPressed, args.Clicks);
            
            foreach(View child in Enumerable.Reverse(Children))
            {
                if(child.Frame.ContainsPoint(coords))
                {
                    child.PropagateMouseEvent(eventType, args);
                }
            }
            
            return false;
        }

        
        /// <summary>
        /// Draws nothing.
        /// </summary>
        protected override void Draw()
        { }

        public new void Render()
        {
            GL.ClearColor(Color.ForestGreen); // we all love forest green!
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.LoadIdentity();

            Draw();

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
