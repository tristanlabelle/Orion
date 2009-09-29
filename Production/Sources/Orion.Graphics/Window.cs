using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Color = System.Drawing.Color;

using OpenTK.Math;

using Orion.Geometry;

namespace Orion.Graphics
{
    /// <summary>
    /// The base game window class. 
    /// </summary>
    public partial class Window : Form
    {
        #region Fields
        internal readonly View rootView;
        #endregion

        #region Constructors
        /// <summary>
        /// Instantiates a new game window. 
        /// </summary>
        public Window()
        {
            InitializeComponent();

            Rectangle maxResolution = new Rectangle(1024f, 768f);
            Rectangle windowBounds = new Rectangle(glControl.Width, glControl.Height);
            rootView = new RootView(windowBounds, maxResolution);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Refreshes this game window, causing a render.
        /// </summary>
        public override void Refresh()
        {
            glControl.Refresh();
        }

        private void glControl_Paint(object sender, PaintEventArgs e)
        {
            rootView.Render();
            glControl.SwapBuffers();
        }
        
        private void glControl_MouseClick(object sender, System.Windows.Forms.MouseEventArgs args)
        {
            TriggerMouseEvent(MouseEventType.MouseClicked, args.X, args.Y, args.Button, args.Clicks);
        }
        
        private void glControl_MouseDown(object sender, System.Windows.Forms.MouseEventArgs args)
        {
            TriggerMouseEvent(MouseEventType.MouseDown, args.X, args.Y, args.Button, args.Clicks);
        }
        
        private void glControl_MouseUp(object sender, System.Windows.Forms.MouseEventArgs args)
        {
            TriggerMouseEvent(MouseEventType.MouseUp, args.X, args.Y, args.Button, args.Clicks);
        }
        
        private void glControl_MouseMove(object sender, System.Windows.Forms.MouseEventArgs args)
        {
            TriggerMouseEvent(MouseEventType.MouseMoved, args.X, args.Y, args.Button, args.Clicks);
        }
        
        private void TriggerMouseEvent(MouseEventType type, float x, float y, MouseButtons argsButton, int clicks)
        {
            MouseButton pressedButton = MouseButton.None;
            switch (argsButton)
            {
                case System.Windows.Forms.MouseButtons.Left: pressedButton = MouseButton.Left; break;
                case System.Windows.Forms.MouseButtons.Middle: pressedButton = MouseButton.Middle; break;
                case System.Windows.Forms.MouseButtons.Right: pressedButton = MouseButton.Right; break;
            }
            
            rootView.PropagateMouseEvent(type, new Orion.MouseEventArgs(x, (glControl.Height - 1) - y, pressedButton, clicks));
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
        #endregion
    }
}