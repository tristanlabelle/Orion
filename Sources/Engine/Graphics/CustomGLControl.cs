using System;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;

namespace Orion.Engine.Graphics
{
    /// <summary>
    /// A custom version of OpenTK's <see cref="GLControl"/>
    /// which properly raises key events.
    /// </summary>
    internal sealed class CustomGLControl : GLControl
    {
        #region Constructors
        public CustomGLControl()
            : base(GetGraphicsMode(), 2, 0, GetGraphicsContextFlags()) { }
        #endregion

        #region Methods
        protected override void OnSizeChanged(System.EventArgs e)
        {
            GL.Viewport(ClientRectangle);
            base.OnSizeChanged(e);
        }

        protected override bool IsInputChar(char charCode)
        {
            return true;
        }

        protected override bool IsInputKey(Keys keyData)
        {
            return true;
        }

        private static GraphicsMode GetGraphicsMode()
        {
            return new GraphicsMode(
                new ColorFormat(8, 8, 8, 8), // Color BPP
                0, 0, // Depth, stencil BPP
                0, // Antialiasing samples
                new ColorFormat(0), // Accum buffer BPP
                2, // Backbuffer count
                false); // Stereo rendering
        }

        private static GraphicsContextFlags GetGraphicsContextFlags()
        {
#if DEBUG
            return GraphicsContextFlags.Debug;
#else
            return GraphicsContextFlags.Default;
#endif
        }
        #endregion
    }
}
