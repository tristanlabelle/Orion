using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Orion.Engine.Audio
{
    /// <summary>
    /// Provides means to play back sounds and apply group volume transformations.
    /// </summary>
    public interface ISoundChannel : IDisposable
    {
        #region Properties
        /// <summary>
        /// Accesses a value indicating if this sound channel is currently muted.
        /// </summary>
        bool IsMuted { get; set; }

        /// <summary>
        /// Accesses the volume transformation of this channel, in range [0, 1].
        /// Affects all sounds played with this channel.
        /// </summary>
        float Volume { get; set; }

        /// <summary>
        /// Accesses a value indicating if this channel and its sounds are currently paused.
        /// </summary>
        bool IsPaused { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// Starts playing a sound on this channel with the given transform.
        /// </summary>
        /// <param name="sound">The sound to be played.</param>
        /// <param name="position">
        /// The 3D position from which to play the sound, or <c>null</c> if the sound is not 3D positioned.
        /// </param>
        void Play(ISound sound, Vector3? position);

        /// <summary>
        /// Stops all the sounds currently playing in this sound channel.
        /// </summary>
        void StopAllSounds();
        #endregion
    }
}
