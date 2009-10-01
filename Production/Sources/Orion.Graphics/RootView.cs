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
    class RootView : ViewContainer
    {
		private Rectangle frame;
		
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

        /// <summary>
        /// Creates a RootView object with a frame and bounds.
        /// </summary>
        /// <param name="frame">The frame of the view</param>
        /// <param name="bounds">The bounds of the view</param>
        public RootView(Rectangle frame, Rectangle bounds)
            : base()
        {
            Bounds = bounds;
			Frame = frame;
        }

        public void Update(float delta)
        {
            PropagateUpdateEvent(new UpdateEventArgs(delta));
        }
        
        protected internal override bool PropagateMouseEvent(MouseEventType eventType, MouseEventArgs args)
        {
            Vector2 coords = args.Position;
            coords.Scale(Bounds.Width / Frame.Width, Bounds.Height / Frame.Height);

            return base.PropagateMouseEvent(eventType, new MouseEventArgs(coords.X, coords.Y, args.ButtonPressed, args.Clicks));
        }

        protected internal override void Render()
        {
            GL.ClearColor(Color.ForestGreen); // we all love forest green!
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.LoadIdentity();

            base.Render();
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
