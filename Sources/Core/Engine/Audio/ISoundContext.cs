using System;
using OpenTK.Math;
namespace Orion.Engine.Audio
{
    /// <summary>
    /// Allows the creation and playback of sound primitives.
    /// </summary>
    public interface ISoundContext : IDisposable
    {
        #region Events
        /// <summary>
        /// Raised when a spatial property of the listener has changed.
        /// </summary>
        event Action<SoundContext> ListenerChanged;
        #endregion

        #region Properties
        /// <summary>
        /// Accesses a value indicating if this sound context is muted.
        /// </summary>
        bool IsMuted { get; set; }

        /// <summary>
        /// Accesses the global volume multiplier of this sound context, in range [0, 1].
        /// </summary>
        float Volume { get; set; }

        /// <summary>
        /// Gets a value indicating if this sound context supports 3D sound.
        /// </summary>
        bool Supports3D { get; }

        /// <summary>
        /// Accesses the listener's spatial matrix.
        /// </summary>
        Matrix4 ListenerMatrix { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// Loads a sound from a given file path.
        /// </summary>
        /// <param name="filePath">The path to the sound file to be loaded.</param>
        /// <returns>The sound that was loaded.</returns>
        ISound LoadSoundFromFile(string filePath);

        SoundSource CreateSource();
        #endregion
    }
}
