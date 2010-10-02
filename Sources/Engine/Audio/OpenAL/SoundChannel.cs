using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Audio;
using OpenTK.Math;

namespace Orion.Engine.Audio.OpenAL
{
    /// <summary>
    /// OpenAL implementation of the <see cref="ISoundChannel"/> interface.
    /// </summary>
    internal sealed class SoundChannel : ISoundChannel
    {
        #region Fields
        private readonly List<int> sourceHandles = new List<int>();
        private bool isMuted;
        private float volume;
        private bool isPaused;
        #endregion

        #region Constructors
        #endregion

        #region Properties
        public bool IsMuted
        {
            get { return isMuted; }
            set
            {
                if (value == isMuted) return;

                isMuted = value;
                foreach (int sourceHandle in sourceHandles)
                    AL.Source(sourceHandle, ALSourcef.Gain, isMuted ? 0 : volume);
            }
        }

        public float Volume
        {
            get { return volume; }
            set
            {
                Argument.EnsureWithin(value, 0, 1, "Volume");

                volume = value;
                if (!isMuted)
                {
                    foreach (int sourceHandle in sourceHandles)
                        AL.Source(sourceHandle, ALSourcef.Gain, isMuted ? 0 : volume);
                }
            }
        }

        public bool IsPaused
        {
            get { return isPaused; }
            set
            {
                if (value == isPaused) return;

                isPaused = value;

                foreach (int sourceHandle in sourceHandles)
                {
                    if (isPaused) AL.SourcePause(sourceHandle);
                    else AL.SourcePlay(sourceHandle);
                }
            }
        }
        #endregion

        #region Methods
        public void Play(ISound sound, Vector3? position)
        {
            throw new NotImplementedException();
        }

        public void StopAllSounds()
        {
            foreach (int sourceHandle in sourceHandles)
            {
                AL.SourceStop(sourceHandle);
                AL.DeleteSource(sourceHandle);
            }
            sourceHandles.Clear();
        }

        public void Dispose()
        {
            StopAllSounds();
        }
        #endregion
    }
}
