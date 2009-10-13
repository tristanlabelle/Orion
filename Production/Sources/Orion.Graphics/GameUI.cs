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

            world.Units.UnitDied += userInputCommander.SelectionManager.UnitDied;

            this.userInputCommander = userInputCommander;
            WorldView worldView = new WorldView(mainWindow.rootView.Bounds, renderer, userInputCommander.SelectionManager);
            worldView.Bounds = new Rectangle(40,30);
            mainWindow.rootView.Children.Add(worldView);

			Rectangle rootBounds = mainWindow.rootView.Bounds;
			Rectangle northFrame = new Rectangle(0, rootBounds.Height, rootBounds.Width, -rootBounds.Height / 20);
			Rectangle southFrame = new Rectangle(0, 0, rootBounds.Width, rootBounds.Height / 20);
			Rectangle eastFrame = new Rectangle(rootBounds.Width, 0, -rootBounds.Width / 20, rootBounds.Height);
			Rectangle westFrame = new Rectangle(0, 0, rootBounds.Width / 20, rootBounds.Height);
			Scroller northScroller = new Scroller(northFrame, worldView, new Vector2(0, 1), Keys.Up);
            Scroller southScroller = new Scroller(southFrame, worldView, new Vector2(0, -1), Keys.Down);
            Scroller eastScroller = new Scroller(eastFrame, worldView, new Vector2(1, 0), Keys.Right);
            Scroller westScroller = new Scroller(westFrame, worldView, new Vector2(-1, 0), Keys.Left);
            mainWindow.rootView.Children.Add(northScroller);
            mainWindow.rootView.Children.Add(southScroller);
            mainWindow.rootView.Children.Add(eastScroller);
            mainWindow.rootView.Children.Add(westScroller);
            Rectangle ressourceDisplayFrame = new Rectangle(0, rootBounds.Height, rootBounds.Width, -rootBounds.Height/25);
            RessourceDisplay ressourceDisplay = new RessourceDisplay(ressourceDisplayFrame, userInputCommander.Faction);
            mainWindow.rootView.Children.Add(ressourceDisplay);
			
            worldView.MouseDown += WorldViewMouseDown;
            worldView.MouseMoved += WorldViewMouseMove;
            worldView.MouseUp += WorldViewMouseUp;
            worldView.KeyDown += WorldViewKeyDown;

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

        private void WorldViewKeyDown(Responder source, KeyboardEventArgs args)
        {
            userInputCommander.OnKeyDown(args.Key);
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
		/// Updates the window and its components state. 
		/// </summary>
		/// <param name="delta">
		/// A <see cref="System.Single"/> representing how many seconds elapsed since the last update event
		/// </param>
        public void Update(float delta)
        {
            mainWindow.rootView.Update(delta);
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
