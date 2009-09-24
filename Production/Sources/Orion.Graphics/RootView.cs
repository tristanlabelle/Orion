﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using OpenTK.Graphics;
using OpenTK.Math;

using Orion.Graphics;
using Orion.Graphics.Drawing;

namespace Orion.Graphics
{
    /// <summary>
    /// A RootView is a base container. It does no drawing on its own.
    /// </summary>
    class RootView : View
    {
        /// <summary>
        /// Creates a RootView object with a frame and bounds.
        /// </summary>
        /// <param name="frame">The frame of the view</param>
        /// <param name="bounds">The bounds of the view</param>
        public RootView(Rect frame, Rect bounds)
            : base(frame)
        {
            Bounds = bounds;
            GL.Viewport(0, 0, (int)frame.Size.X, (int)frame.Size.Y);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, 1024, 0, 768, -1, 1);
            GL.MatrixMode(MatrixMode.Modelview);
        }

        /// <summary>
        /// Draws nothing.
        /// </summary>
        /// <param name="context"></param>
        protected override void Draw(Orion.Graphics.Drawing.GraphicsContext context)
        {
        }

        internal override void Render()
        {
            GL.ClearColor(Color.White);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            base.Render();
        }
    }
}
