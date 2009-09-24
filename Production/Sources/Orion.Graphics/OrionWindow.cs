using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Audio;
using OpenTK.Math;
using OpenTK.Input;
using OpenTK.Platform;

namespace Orion.Graphics
{
    public class OrionWindow : OpenTK.GameWindow
    {
        // Creates a new TextPrinter to draw text on the screen.
        TextPrinter printer = new TextPrinter(TextQuality.Medium);
        Font sans_serif = new Font(FontFamily.GenericSansSerif, 18.0f);

        /// <summary>Creates a 800x600 window with the specified title.</summary>
        public OrionWindow()
            : base(800, 600, new GraphicsMode(new ColorFormat(8,8,8,0), 0, 1), "OpenTK Quick Start Sample")
        {
            VSync = VSyncMode.On;
        }

        /// <summary>Load resources here.</summary>
        /// <param name="e">Not used.</param>
        public override void OnLoad(EventArgs e)
        {
            GL.ClearColor(System.Drawing.Color.SteelBlue);
            GL.Enable(EnableCap.DepthTest);
        }

        /// <summary>
        /// Called when your window is resized. Set your viewport here. It is also
        /// a good place to set up your projection matrix (which probably changes
        /// along when the aspect ratio of your window).
        /// </summary>
        /// <param name="e">Contains information on the new Width and Size of the GameWindow.</param>
        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            Glu.Perspective(45.0, Width / (double)Height, 1.0, 64.0);
        }

        /// <summary>
        /// Called when it is time to setup the next frame. Add you game logic here.
        /// </summary>
        /// <param name="e">Contains timing information for framerate independent logic.</param>
        public override void OnUpdateFrame(UpdateFrameEventArgs e)
        {
            if (Keyboard[Key.Escape])
                Exit();
        }

        /// <summary>
        /// Called when it is time to render the next frame. Add your rendering code here.
        /// </summary>
        /// <param name="e">Contains timing information.</param>
        public override void OnRenderFrame(RenderFrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            Glu.LookAt(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY);

            GL.Begin(BeginMode.Triangles);

            GL.Color3(Color.Red);
			GL.Vertex3(-1.0f, -2.0f, 10.0f);
			GL.Vertex3(0.0f, 2.0f, 10.0f);
			GL.Vertex3(1.0f, -2.0f, 10.0f);

            GL.End();

            printer.Begin();

            printer.Print(((int)(1 / e.Time)).ToString("F0"), sans_serif, Color.SpringGreen);

            printer.End();

            SwapBuffers();
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // The 'using' idiom guarantees proper resource cleanup.
            // We request 30 UpdateFrame events per second, and unlimited
            // RenderFrame events (as fast as the computer can handle).
            using (OrionWindow game = new OrionWindow())
            {
                game.Run(30.0, 0.0);
            }
        }
    }
}
