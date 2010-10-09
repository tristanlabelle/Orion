using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Orion.Engine.Audio.Null
{
    internal sealed class SoundChannel : ISoundChannel
    {
        #region Constructors
        public SoundChannel()
        {
            Volume = 1;
        }
        #endregion

        #region Properties
        public bool IsMuted { get; set; }

        public float Volume { get; set; }

        public bool IsPaused { get; set; }
        #endregion

        #region Methods
        public void Play(ISound sound, Vector3? position)
        {
            Argument.EnsureNotNull(sound, "sound");
        }

        public void StopAllSounds() {}

        public void Dispose() {}
        #endregion
    }
}
