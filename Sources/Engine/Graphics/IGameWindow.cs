using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Gui;

namespace Orion.Engine.Graphics
{
    /// <summary>
    /// Provides basic window functionality exposing the <see cref="GraphicsContext"/>
    /// drawing interface.
    /// </summary>
    public interface IGameWindow : IDisposable
    {
        #region Properties
        /// <summary>
        /// Accesses the text shown in the title bar of the window.
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// Gets the size of the client area of the window.
        /// </summary>
        Size ClientAreaSize { get; }

        /// <summary>
        /// Gets a value indicating the current window mode.
        /// </summary>
        WindowMode Mode { get; }

        /// <summary>
        /// Gets the graphics context that can be used to draw to this window.
        /// </summary>
        GraphicsContext GraphicsContext { get; }

        /// <summary>
        /// Gets a value indicating if an input event is available for dequeuing.
        /// </summary>
        bool IsInputEventAvailable { get; }

        /// <summary>
        /// Gets a value indicating if this window has been closed.
        /// </summary>
        bool WasClosed { get; }
        #endregion

        #region Events
        /// <summary>
        /// Raised when the window has been or is being resized.
        /// </summary>
        event Action<IGameWindow> Resized;

        /// <summary>
        /// Raised when the window is being asked to close.
        /// </summary>
        event Action<IGameWindow> Closing;
        #endregion

        #region Methods
        /// <summary>
        /// Gets the next input event that was enqueued by this window.
        /// </summary>
        /// <returns>The input event that was enqueued.</returns>
        InputEvent DequeueInputEvent();

        /// <summary>
        /// Switches to windowed mode with a given client size.
        /// </summary>
        /// <param name="clientAreaSize">The size of the client area of the window.</param>
        void SetWindowed(Size clientAreaSize);

        /// <summary>
        /// Switches to fullscreen mode with a given screen resolution.
        /// </summary>
        /// <param name="resolution">The resolution to which to switch.</param>
        void SetFullscreen(Size resolution);

        /// <summary>
        /// Updates this window, giving it the chance to handle OS events.
        /// </summary>
        void Update();
        #endregion
    }
}
