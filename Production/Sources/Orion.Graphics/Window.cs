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
    public partial class Window : Form
    {
        private View rootView;

        public Window()
        {
            InitializeComponent();

            Rect fullScreen = new Rect(1024f, 768f);
            Rect rootBounds = new Rect(glControl.Width, glControl.Height);
            rootView = new RootView(rootBounds, fullScreen);

            View terrain = new TerrainView(fullScreen);
            //terrain.Bounds = new Rect(50, 50);
            rootView.AddSubview(terrain);
        }

        private void glControl_Paint(object sender, PaintEventArgs e)
        {
            rootView.Render();
            glControl.SwapBuffers();
        }

        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.Run(new Window());
        }
    }
}
