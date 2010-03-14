using System;

namespace Orion.Engine.Audio
{
    /// <summary>
    /// Represents a sound which can be played back by a sound source.
    /// </summary>
    public interface ISound : IDisposable
    {
        #region Properties
        /// <summary>
        /// Gets the duration of this sound.
        /// </summary>
        TimeSpan Duration { get; }

        /// <summary>
        /// Gets the number of channels this sound has. One for monaural, two for stereo.
        /// </summary>
        int ChannelCount { get; }
        #endregion
    }
}
