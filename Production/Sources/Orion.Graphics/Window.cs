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

            Rect fullScreen = new Rect(1024f, 768f);
            Rect rootBounds = new Rect(glControl.Width, glControl.Height);
            rootView = new RootView(rootBounds, fullScreen);

            View terrain = new TerrainView(new Rect(200, 200, 100, 100));
            rootView.AddSubview(terrain);
        }

        private void glControl_Paint(object sender, PaintEventArgs e)
        {
            rootView.Render();
            glControl.SwapBuffers();
        }
		
		private void form_Resize(object sender, EventArgs args)
		{
			rootView.Frame = rootView.Frame.ResizeTo(Size.Width, Size.Height);
		}

		/// <summary>
		/// Executes the test program. Creates a game window and runs it, for fun. 
		/// </summary>
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.Run(new Window());
        }
    }
}
