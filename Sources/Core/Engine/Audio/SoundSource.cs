using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Math;
using IrrKlang;
using System.Diagnostics;

namespace Orion.Engine.Audio
{
    /// <summary>
    /// Represents a location in 3D space that emits sounds to be heard by the listener.
    /// </summary>
    public sealed class SoundSource : IDisposable
    {
        #region Fields
        private readonly SoundContext context;
        private readonly Action<SoundContext> listenerPositionChangedHandler;
        private ISound irrKlangSound;
        private Sound sound;
        private Vector3? position;
        private float volume = 1;
        #endregion

        #region Constructors
        internal SoundSource(SoundContext context)
        {
            Debug.Assert(context != null);
            this.context = context;
            this.listenerPositionChangedHandler = OnListenerPositionChanged;
            this.context.ListenerPositionChanged += listenerPositionChangedHandler;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the sound currently bound to this source.
        /// </summary>
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

        /// <summary>
        /// Accesses the 3D position of sound played through this source.
        /// A value of <c>null</c> indicates that the sound is played in 2D.
        /// </summary>
        public Vector3? Position
        {
            get { return position; }
            set
            {
                position = value;
                if (irrKlangSound != null) irrKlangSound.Position = IrrKlangPosition;
            }
        }

        /// <summary>
        /// Accesses the volume at which this source plays sounds.
        /// </summary>
        public float Volume
        {
            get { return volume; }
            set
            {
                volume = value;
                if (irrKlangSound != null) irrKlangSound.Volume = volume;
            }
        }

        /// <summary>
        /// Gets a value indicating if this source is currently playing a sound.
        /// </summary>
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
        /// <summary>
        /// Starts or resumes playing the sound currently attached to this source.
        /// </summary>
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

        /// <summary>
        /// Attaches a sound to this source and starts playing it.
        /// </summary>
        /// <param name="sound">The sound to be attached and played.</param>
        public void Play(Sound sound)
        {
            Argument.EnsureNotNull(sound, "sound");
            
            Sound = sound;
            Play();
        }

        /// <summary>
        /// Pauses a sound that is currently playing.
        /// </summary>
        public void Pause()
        {
            if (sound == null) throw new InvalidOperationException("Cannot pause a sound source without a sound.");
            if (irrKlangSound != null) irrKlangSound.Paused = true;
        }

        /// <summary>
        /// Stops playing a sound and rewinds to its beginning.
        /// </summary>
        public void Stop()
        {
            if (sound == null) throw new InvalidOperationException("Cannot stop a sound source without a sound.");
            DeleteIrrKlangSound();
        }

        /// <summary>
        /// Releases all resources used by this <see cref="SoundSource"/>.
        /// </summary>
        public void Dispose()
        {
            context.ListenerPositionChanged -= listenerPositionChangedHandler;
            Sound = null;
        }

        private void OnListenerPositionChanged(SoundContext context)
        {
            Debug.Assert(context == this.context);
            if (!position.HasValue && irrKlangSound != null)
                irrKlangSound.Position = context.IrrKlangListenerPosition;
        }

        private void DeleteIrrKlangSound()
        {
            if (irrKlangSound == null) return;
            
            irrKlangSound.Stop();
            irrKlangSound.Dispose();
            irrKlangSound = null;
        }
        #endregion
    }
}
