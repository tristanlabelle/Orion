using System;
using System.Diagnostics;
using Orion.Engine.Graphics;
using Orion.Graphics;
namespace Orion.UserInterface
{
    /// <summary>
    /// Objects of this class are used to render the game.
    /// They are capable of refreshing the OpenGL control.
    /// </summary>
    public sealed class GameUI : IDisposable
    {
        #region Fields
        private readonly Window mainWindow;
        private readonly GameGraphics graphics;
        #endregion

        #region Constructors
        public GameUI()
        {
            this.mainWindow = new Window();
            this.mainWindow.Show();
            this.mainWindow.HandleDestroyed += OnWindowHandleDestroyed;
            this.graphics = new GameGraphics(this.mainWindow.GraphicsContext);
        }
        #endregion

        #region Properties
        public Window Window
        {
            get { return mainWindow; }
        }

        /// <summary>
        /// Gets a value indicating if the window is still created.
        /// </summary>
        public bool IsWindowCreated
        {
            get { return mainWindow.Created; }
        }

        public string WindowTitle
        {
            get { return mainWindow.Text; }
            set { mainWindow.Text = value; }
        }

        public RootView RootView
        {
            get { return mainWindow.RootView; }
        }

        public GameGraphics Graphics
        {
            get { return graphics; }
        }
        #endregion

        #region Methods
        public void Display(UIDisplay display)
        {
            mainWindow.RootView.PushDisplay(display);
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
            mainWindow.RootView.Update(timeDeltaInSeconds);
        }

        /// <summary>
        /// Disposes this <see cref="GameUI"/>, releasing all used resources.
        /// </summary>
        public void Dispose()
        {
            mainWindow.Dispose();
        }

        private void OnWindowHandleDestroyed(object sender, EventArgs e)
        {
            graphics.Dispose();
        }
        #endregion
    }
}
