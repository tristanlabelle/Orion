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
            rootView = new RootView(new Rect(glControl.Width, glControl.Height), new Rect(1024f, 768f));
            rootView.AddSubview(new TerrainView(new Rect(-5, -5, 10, 10)));
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
