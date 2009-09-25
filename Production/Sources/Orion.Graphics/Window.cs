using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Orion.Graphics
{
    /// <summary>
    /// The base game window class. 
    /// </summary>
    public partial class Window : Form
    {
        private View rootView;

        /// <summary>
        /// Instantiates a new game window. 
        /// </summary>
        public Window()
        {
            InitializeComponent();
            Rectangle maxResolution = new Rectangle(1024f, 768f);
            Rectangle windowBounds = new Rectangle(glControl.Width, glControl.Height);
            rootView = new RootView(windowBounds, maxResolution);

            View terrain = new TerrainView(maxResolution);
            terrain.Bounds = new Rectangle(0, 0, 500, 500);
            rootView.Children.Add(terrain);
        }

        private void glControl_Paint(object sender, PaintEventArgs e)
        {
            rootView.Render();
            glControl.SwapBuffers();
        }

        /// <summary>
        /// Fires the Resized event to all listener, and resizes the glControl.
        /// </summary>
        /// <param name="e">Unused arguments</param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (rootView != null)
            {
                rootView.Frame = rootView.Frame.ResizeTo(glControl.Width, glControl.Height);
                glControl.Refresh();
            }
        }

        /// <summary>
        /// Executes the test program. Creates a game window and runs it. 
        /// </summary>
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.Run(new Window());
        }
    }
}