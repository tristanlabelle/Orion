using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using System.IO;

namespace Orion.Engine.Audio.Null
{
    /// <summary>
    /// A null sound context which mimics real behavior but does nothing.
    /// </summary>
    public sealed class SoundContext : ISoundContext
    {
        #region Constructors
        public SoundContext()
        {
            Volume = 1;
            ListenerMatrix = Matrix4.Identity;
        }
        #endregion

        #region Properties
        public bool IsMuted { get; set; }

        public float Volume { get; set; }

        public Matrix4 ListenerMatrix { get; set; }
        #endregion

        #region Methods
        public ISound LoadSoundFromFile(string filePath)
        {
            throw new IOException("Could not load sound from a null sound context.");
        }

        public ISoundChannel CreateChannel()
        {
            return new SoundChannel();
        }

        public void Dispose() { }
        #endregion
    }
}
