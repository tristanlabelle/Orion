using System;
using System.Diagnostics;



namespace Orion.UserInterface
{
    /// <summary>
    /// Objects of this class are used to render the game. They encapsulate a game window and a world renderer.
    /// They are capable of refreshing the OpenGL control.
    /// </summary>
    public sealed class GameUI : IDisposable
    {
        #region Fields
        private readonly Window mainWindow;
        private readonly FrameRateCounter frameRateCounter = new FrameRateCounter();
        #endregion

        #region Constructors
        /// <summary>
        /// Constructs a GameUI from a passed World object.
        /// </summary>
        /// <param name="world">The World object containing the game data</param>
        /// <param name="userInputCommander">The user input commander that will receive UI events</param>
        public GameUI()
        {
            mainWindow = new Window();
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

        public ViewContainer RootView
        {
            get { return mainWindow.rootView; }
        }

        #endregion

        #region Methods
        public void Display(UIDisplay display)
        {
            mainWindow.rootView.PushDisplay(display);
        }

        /// <summary>
        /// Causes the game to render itself.
        /// </summary>
        public void Refresh()
        {
            mainWindow.Refresh();
        }

        /// <summary>
        /// Updates the window and its components state. 
        /// </summary>
        /// <param name="delta">
        /// A <see cref="System.Single"/> representing how many seconds elapsed since the last update event
        /// </param>
        public void Update(float timeDeltaInSeconds)
        {
            mainWindow.rootView.Update(timeDeltaInSeconds);
            frameRateCounter.Update();
            mainWindow.Text = frameRateCounter.ToString();
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
