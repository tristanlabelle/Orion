using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IrrKlang;

namespace Orion.Engine.Audio.IrrKlang
{
    /// <summary>
    /// Encapsulates an IrrKlang <see cref="ISoundSource"/> behind a <see cref="ISound"/>-compliant interface.
    /// </summary>
    internal sealed class Sound : ISound
    {
        #region Fields
        private readonly ISoundSource irrKlangSoundSource;
        #endregion

        #region Constructors
        internal Sound(ISoundSource irrKlangSoundSource)
        {
            Argument.EnsureNotNull(irrKlangSoundSource, "irrKlangSoundSource");

            this.irrKlangSoundSource = irrKlangSoundSource;
        }
        #endregion

        #region Properties
        public TimeSpan Duration
        {
            get { return TimeSpan.FromMilliseconds(irrKlangSoundSource.PlayLength); }
        }

        public int ChannelCount
        {
            get { return irrKlangSoundSource.AudioFormat.ChannelCount; }
        }

        internal ISoundSource IrrKlangSoundSource
        {
            get { return irrKlangSoundSource; }
        }
        #endregion

        #region Methods
        public void Dispose()
        {
            irrKlangSoundSource.__dtor();
        }
        #endregion
    }
}
