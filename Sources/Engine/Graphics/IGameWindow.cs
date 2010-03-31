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
        /// Gets the size of the viewport which can be drawn into within the window.
        /// </summary>
        Size ViewportSize { get; }

        /// <summary>
        /// Gets a value indicating if the window is running fullscreen.
        /// </summary>
        bool IsFullScreen { get; }

        /// <summary>
        /// Gets the graphics context that can be used to draw to this window.
        /// </summary>
        GraphicsContext GraphicsContext { get; }

        /// <summary>
        /// Gets a value indicating if an input event is available for dequeuing.
        /// </summary>
        bool IsInputEventAvailable { get; }
        #endregion

        #region Events
        /// <summary>
        /// Raised when the window is being resized.
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
        /// Updates this window, giving it the chance to handle OS events.
        /// </summary>
        void Update();
        #endregion
    }
}
