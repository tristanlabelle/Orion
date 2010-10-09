using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using IrrKlang;
using OpenTK;
using IrrKlangSound = IrrKlang.ISound;

namespace Orion.Engine.Audio.IrrKlang
{
    /// <summary>
    /// Implements the <see cref="ISoundChannel"/> interface for the IrrKlang sound driver.
    /// </summary>
    internal sealed class SoundChannel : ISoundChannel, ISoundStopEventReceiver
    {
        #region Fields
        private readonly ISoundEngine irrKlangSoundEngine;
        private readonly List<IrrKlangSound> sounds = new List<IrrKlangSound>();
        private bool isMuted;
        private float volume = 1;
        private bool isPaused;
        #endregion

        #region Constructors
        public SoundChannel(ISoundEngine irrKlangSoundEngine)
        {
            Argument.EnsureNotNull(irrKlangSoundEngine, "irrKlangSoundEngine");
            this.irrKlangSoundEngine = irrKlangSoundEngine;
        }
        #endregion

        #region Properties
        public bool IsMuted
        {
            get { return isMuted; }
            set
            {
                if (value == isMuted) return;
                isMuted = value;
                UpdateSoundVolumes();
            }
        }

        public float Volume
        {
            get { return volume; }
            set
            {
                Argument.EnsureWithin(value, 0, 1, "Volume");
                if (value == volume) return;
                volume = value;
                if (!isMuted) UpdateSoundVolumes();
            }
        }

        public bool IsPaused
        {
            get { return isPaused; }
            set
            {
                if (value == isPaused) return;

                isPaused = value;

                foreach (IrrKlangSound sound in sounds)
                    sound.Paused = isPaused;
            }
        }
        #endregion

        #region Methods
        public void Play(ISound sound, Vector3? position)
        {
            Argument.EnsureNotNull(sound, "sound");
            Argument.EnsureBaseType<Sound>(sound, "sound");

            ISoundSource irrKlangSoundSource = ((Sound)sound).IrrKlangSoundSource;

            IrrKlangSound irrKlangSound;
            if (position.HasValue)
            {
                irrKlangSound = irrKlangSoundEngine.Play3D(
                    irrKlangSoundSource,
                    position.Value.X, position.Value.Y, position.Value.Z,
                    false, true, false);
            }
            else
            {
                irrKlangSound = irrKlangSoundEngine.Play2D(irrKlangSoundSource, false, true, false);
            }

            irrKlangSound.setSoundStopEventReceiver(this);
            irrKlangSound.Volume = isMuted ? 0 : volume;
            if (!isPaused) irrKlangSound.Paused = false;
            
            sounds.Add(irrKlangSound);
        }

        public void StopAllSounds()
        {
            while (sounds.Count > 0)
            {
                IrrKlangSound sound = sounds[0];

                sound.Stop();
                sound.Dispose();

                sounds.Remove(sound);
            }
        }

        public void Dispose()
        {
            StopAllSounds();
        }

        private void UpdateSoundVolumes()
        {
            foreach (IrrKlangSound sound in sounds)
                sound.Volume = isMuted ? 0 : volume;
        }

        void ISoundStopEventReceiver.OnSoundStopped(IrrKlangSound sound, StopEventCause reason, object userData)
        {
            sounds.Remove(sound);
        }
        #endregion
    }
}
