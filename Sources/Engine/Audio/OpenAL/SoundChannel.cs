using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Audio.OpenAL;

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
        private float volume = 1;
        private bool isPaused;
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
            Argument.EnsureBaseType<Sound>(sound, "sound");

            RemoveCompletedSounds();

            int sourceHandle = AL.GenSource();
            AL.BindBufferToSource(sourceHandle, ((Sound)sound).Handle);
            Debug.Assert(AL.GetError() == ALError.NoError, "Non-critical error while creating OpenAL audio source.");

            AL.Source(sourceHandle, ALSourceb.SourceRelative, !position.HasValue);
            if (position.HasValue) AL.Source(sourceHandle, ALSource3f.Position, -position.Value.X, position.Value.Y, position.Value.Z);
            else AL.Source(sourceHandle, ALSource3f.Position, 0, 0, 0);

            AL.Source(sourceHandle, ALSourcef.Gain, isMuted ? 0 : volume);
            if (!isPaused) AL.SourcePlay(sourceHandle);

            sourceHandles.Add(sourceHandle);
        }

        private void RemoveCompletedSounds()
        {
            for (int i = sourceHandles.Count - 1; i >= 0; --i)
            {
                int sourceHandle = sourceHandles[i];
                if (AL.GetSourceState(sourceHandle) == ALSourceState.Stopped)
                {
                    AL.DeleteSource(sourceHandle);
                    Debug.Assert(AL.GetError() == ALError.NoError, "Non-critical error while deleting OpenAL audio source.");
                    sourceHandles.RemoveAt(i);
                }
            }
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
