using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Orion.GameLogic;
using Orion.Geometry;

using OpenTK.Math;

namespace Orion.Graphics
{
    /// <summary>
    /// Objects of this class are used to render the game. They encapsulate a game window and a world renderer.
    /// They are capable of refreshing the OpenGL control.
    /// </summary>
    public class GameUI
    {
        private readonly Window mainWindow;
        private SelectionManager selectionManager;

        /// <summary>
        /// The object used to render the world.
        /// </summary>
        public readonly WorldRenderer Renderer;
        /// <summary>
        /// The Form in which resides the OpenGL control that renders the game.
        /// </summary>
        public Form MainWindow { get { return mainWindow; } }

        /// <summary>
        /// Constructs a GameRenderer from a passed World object.
        /// </summary>
        /// <param name="world">The World object containing the game data</param>
        public GameUI(World world)
        {
            mainWindow = new Window();
            Renderer = new WorldRenderer(world);

            selectionManager = new SelectionManager(world);

            WorldView view = new WorldView(mainWindow.rootView.Bounds, Renderer, selectionManager);
            view.Bounds = new Rectangle(0, 0, 32, 24);
            mainWindow.rootView.Children.Add(view);

            view.MouseDown += WorldViewMouseDown;
            view.MouseMoved += WorldViewMouseMove;
            view.MouseUp += WorldViewMouseUp;
        }

        private void WorldViewMouseDown(View source, MouseEventArgs args)
        {
            selectionManager.OnMouseButton(args.ButtonPressed, true);
        }

        private void WorldViewMouseUp(View source, MouseEventArgs args)
        {
            selectionManager.OnMouseButton(args.ButtonPressed, false);
        }

        private void WorldViewMouseMove(View source, MouseEventArgs args)
        {
            selectionManager.OnMouseMove(new Vector2(args.X, args.Y));
        }

        /// <summary>
        /// Refreshes the OpenGL view of the game.
        /// </summary>
        public void Refresh()
        {
            mainWindow.rootView.Render();
        }
    }
}
