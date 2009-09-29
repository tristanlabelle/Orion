using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Orion.GameLogic;
using Orion.Geometry;

namespace Orion.Graphics
{
    /// <summary>
    /// Objects of this class are used to render the game. They encapsulate a game window and a world renderer.
    /// They are capable of refreshing the OpenGL control.
    /// </summary>
    public class GameRenderer
    {
        private readonly Window mainWindow;

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
        public GameRenderer(World world)
        {
            mainWindow = new Window();
            Renderer = new WorldRenderer(world);
            WorldView view = new WorldView(mainWindow.rootView.Bounds, Renderer);
            view.Bounds = new Rectangle(0, 0, 32, 24);
            mainWindow.rootView.Children.Add(view);
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
