using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Orion.GameLogic;
using Orion.Geometry;

using OpenTK.Math;
using Orion.Commandment;

namespace Orion.Graphics
{
    /// <summary>
    /// Objects of this class are used to render the game. They encapsulate a game window and a world renderer.
    /// They are capable of refreshing the OpenGL control.
    /// </summary>
    public sealed class GameUI : IDisposable
    {
        #region Fields
        private readonly Window mainWindow;
        private readonly WorldRenderer renderer;
        private UserInputCommander userInputCommander;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructs a GameRenderer from a passed World object.
        /// </summary>
        /// <param name="world">The World object containing the game data</param>
        /// <param name="userInputCommander">The user input commander that will receive UI events</param>
        public GameUI(World world, UserInputCommander userInputCommander)
        {
            Argument.EnsureNotNull(world, "world");
            Argument.EnsureNotNull(userInputCommander, "userInputCommander");

            mainWindow = new Window();

            renderer = new WorldRenderer(world);

            this.userInputCommander = userInputCommander;
            WorldView view = new WorldView(mainWindow.rootView.Bounds, renderer, userInputCommander.SelectionManager);
            view.Bounds = new Rectangle(40,30);
            mainWindow.rootView.Children.Add(view);

			Scroller northScroller = new Scroller(new Rectangle(0, mainWindow.rootView.Bounds.Height, mainWindow.rootView.Bounds.Width, -mainWindow.rootView.Bounds.Height / 20), view, new Vector2(0, 1), world.Bounds);
            Scroller southScroller = new Scroller(new Rectangle(0, 0, mainWindow.rootView.Bounds.Width, mainWindow.rootView.Bounds.Height / 20), view, new Vector2(0, -1), world.Bounds);
            Scroller eastScroller = new Scroller(new Rectangle(mainWindow.rootView.Bounds.Width, 0, -mainWindow.rootView.Bounds.Width / 20, mainWindow.rootView.Bounds.Height), view, new Vector2(1, 0), world.Bounds);
            Scroller westScroller = new Scroller(new Rectangle(0, 0, mainWindow.rootView.Bounds.Width / 20, mainWindow.rootView.Bounds.Height), view, new Vector2(-1, 0), world.Bounds);
            mainWindow.rootView.Children.Add(northScroller);
            mainWindow.rootView.Children.Add(southScroller);
            mainWindow.rootView.Children.Add(eastScroller);
            mainWindow.rootView.Children.Add(westScroller);
			
            view.MouseDown += WorldViewMouseDown;
            view.MouseMoved += WorldViewMouseMove;
            view.MouseUp += WorldViewMouseUp;

            mainWindow.Show();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets a value indicating if the window is still created.
        /// </summary>
        public bool IsWindowCreated
        {
            get { return mainWindow.Created; }
        }

        private void WorldViewMouseDown(Responder source, MouseEventArgs args)
        {
            userInputCommander.OnMouseButton(new Vector2(args.X, args.Y), args.ButtonPressed, true);
        }

        private void WorldViewMouseUp(Responder source, MouseEventArgs args)
        {
            userInputCommander.OnMouseButton(new Vector2(args.X, args.Y), args.ButtonPressed, false);
        }

        private void WorldViewMouseMove(Responder source, MouseEventArgs args)
        {
            userInputCommander.OnMouseMove(new Vector2(args.X, args.Y));
        }
        #endregion

        #region Methods
        /// <summary>
        /// Causes the game to render itself.
        /// </summary>
        public void Render()
        {
            Application.DoEvents();
            mainWindow.Refresh();
        }

        /// <summary>
        /// Disposes this <see cref="GameUI"/>, releasing all used resources.
        /// </summary>
        public void Dispose()
        {
            mainWindow.Dispose();
        }
        #endregion
    }
}
