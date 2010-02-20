using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Math;
using IrrKlang;
using System.Diagnostics;

namespace Orion.Audio
{
    public sealed class SoundSource : IDisposable
    {
        #region Fields
        private readonly AudioContext context;
        private readonly GenericEventHandler<AudioContext> listenerPositionChangedHandler;
        private ISound irrKlangSound;
        private Sound sound;
        private Vector3? position;
        private float volume = 1;
        #endregion

        #region Constructors
        internal SoundSource(AudioContext context)
        {
            Debug.Assert(context != null);
            this.context = context;
            this.listenerPositionChangedHandler = OnListenerPositionChanged;
            this.context.ListenerPositionChanged += listenerPositionChangedHandler;
        }
        #endregion

        #region Properties
        public Sound Sound
        {
            get { return sound; }
            set
            {
                if (value == sound) return;
                DeleteIrrKlangSound();
                sound = value;
            }
        }

        public Vector3? Position
        {
            get { return position; }
            set
            {
                position = value;
                if (irrKlangSound != null) irrKlangSound.Position = IrrKlangPosition;
            }
        }

        public float Volume
        {
            get { return volume; }
            set
            {
                volume = value;
                if (irrKlangSound != null) irrKlangSound.Volume = volume;
            }
        }

        public bool IsPlaying
        {
            get { return irrKlangSound != null && !irrKlangSound.Paused && !irrKlangSound.Finished; }
        }

        private ISoundEngine Engine
        {
            get { return context.IrrKlangEngine; }
        }

        private Vector3D IrrKlangPosition
        {
            get
            {
                if (position.HasValue) return new Vector3D(position.Value.X, position.Value.Y, position.Value.Z);
                return context.IrrKlangListenerPosition;
            }
        }
        #endregion

        #region Methods
        public void Play()
        {
            if (sound == null) throw new InvalidOperationException("Cannot play a sound source without a sound.");

            if (irrKlangSound != null)
            {
                if (irrKlangSound.Paused)
                {
                    irrKlangSound.Paused = false;
                    return;
                }
                
                DeleteIrrKlangSound();
            }

            Vector3D irrKlangPosition = IrrKlangPosition;
            irrKlangSound = Engine.Play3D(sound.IrrKlangSource, irrKlangPosition.X, irrKlangPosition.Y, irrKlangPosition.Z, false, false, false);
            if (irrKlangSound != null) irrKlangSound.Volume = volume;
        }

        public void Play(Sound sound)
        {
            Argument.EnsureNotNull(sound, "sound");
            
            Sound = sound;
            Play();
        }

        public void Pause()
        {
            if (sound == null) throw new InvalidOperationException("Cannot pause a sound source without a sound.");
            if (irrKlangSound != null) irrKlangSound.Paused = true;
        }

        public void Stop()
        {
            if (sound == null) throw new InvalidOperationException("Cannot stop a sound source without a sound.");
            DeleteIrrKlangSound();
        }

        public void Dispose()
        {
            context.ListenerPositionChanged -= listenerPositionChangedHandler;
            Sound = null;
        }

        private void OnListenerPositionChanged(AudioContext context)
        {
            Debug.Assert(context == this.context);
            if (!position.HasValue && irrKlangSound != null)
                irrKlangSound.Position = context.IrrKlangListenerPosition;
        }

        private void DeleteIrrKlangSound()
        {
            if (irrKlangSound == null) return;
            
            irrKlangSound.Stop();
            irrKlangSound.__dtor();
            irrKlangSound = null;
        }
        #endregion
    }
}
