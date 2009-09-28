using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Orion.Geometry;
using Orion.GameLogic;
using Orion.Commandment;

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
			
            WorldView world = new WorldView(maxResolution, new WorldRenderer(new World()));
            world.Bounds = new Rectangle(0, 0, 32, 24);
            rootView.Children.Add(world);
			
			// putting little guys to life
			{
	            CommandManager commandManager = new CommandManager();
	
	            Faction redFaction = new Faction(world, "Red", Color.Red);
	            MockCommander redCommander = new MockCommander(redFaction, world);
	            commandManager.AddCommander(redCommander);
	
	            Unit redJedi = new Unit(0, new UnitType("Jedi"), world);
	            Unit redPirate = new Unit(1, new UnitType("Pirate"), world);
	            Unit redNinja = new Unit(2, new UnitType("Ninja"), world);
	            redJedi.Faction = redFaction;
	            redPirate.Faction = redFaction;
	            redNinja.Faction = redFaction;
	            world.Units.Add(redJedi);
	            world.Units.Add(redPirate);
	            world.Units.Add(redNinja);
	
	            Faction blueFaction = new Faction(world, "Blue", Color.Blue);
	            MockCommander blueCommander = new MockCommander(blueFaction, world);
	            commandManager.AddCommander(blueCommander);
	
	            Unit blueJedi = new Unit(3, new UnitType("Jedi"), world);
	            Unit bluePirate = new Unit(4, new UnitType("Pirate"), world);
	            Unit blueNinja = new Unit(5, new UnitType("Ninja"), world);
	            blueJedi.Faction = blueFaction;
	            bluePirate.Faction = blueFaction;
	            blueNinja.Faction = blueFaction;
	            world.Units.Add(blueJedi);
	            world.Units.Add(bluePirate);
	            world.Units.Add(blueNinja);
			}
        }
		
		/// <summary>
		/// Refreshes the OpenGL control. 
		/// </summary>
		public void RenderGLControl()
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
			
			rootView.PropagateMouseEvent(type, new Orion.Graphics.MouseEventArgs(x, (glControl.Height - 1) - y, pressedButton, clicks));
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