using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Audio;
using System.Diagnostics;
using OpenTK.Math;

namespace Orion.Audio
{
    internal sealed class AudioSource : IDisposable
    {
        #region Fields
        private readonly uint handle;
        private readonly GenericEventHandler<SoundBuffer> bufferDisposingHandler;
        private SoundBuffer buffer;
        #endregion

        #region Constructors
        public AudioSource()
        {
            AL.GenSource(out this.handle);
            Debug.Assert(handle != 0 && AL.GetError() == ALError.NoError);

            this.bufferDisposingHandler = OnBufferDisposing;
        }
        #endregion

        #region Properties
        public uint Handle
        {
            get { return handle; }
        }

        #region Spatial
        public bool IsRelativeToListener
        {
            get
            {
                bool value;
                AL.GetSource(handle, ALSourceb.SourceRelative, out value);
                Debug.Assert(AL.GetError() == ALError.NoError);
                return value;
            }
            set
            {
                AL.Source(handle, ALSourceb.SourceRelative, value);
                Debug.Assert(AL.GetError() == ALError.NoError);
            }
        }

        public Vector2 Position
        {
            get { return Get(ALSource3f.Position); }
            set { Set(ALSource3f.Position, value); }
        }

        public Vector2 Velocity
        {
            get { return Get(ALSource3f.Velocity); }
            set { Set(ALSource3f.Velocity, value); }
        }

        public Vector2 Direction
        {
            get { return Get(ALSource3f.Direction); }
            set { Set(ALSource3f.Direction, value); }
        }
        #endregion

        #region State
        public SoundBuffer Buffer
        {
            get { return buffer; }
            set
            {
                if (value == buffer) return;

                if (buffer != null) Stop();

                if (value == null)
                    AL.BindBufferToSource(handle, 0);
                else
                    AL.BindBufferToSource(handle, value.Handle);
                
                Debug.Assert(AL.GetError() == ALError.NoError);

                buffer = value;
            }
        }

        public ALSourceState State
        {
            get { return AL.GetSourceState(handle); }
        }

        public bool IsInUse
        {
            get
            {
                ALSourceState state = State;
                return state == ALSourceState.Playing || state == ALSourceState.Paused;
            }
        }
        #endregion

        public float Gain
        {
            get { return Get(ALSourcef.Gain); }
            set { Set(ALSourcef.Gain, value); }
        }

        #region Offset
        public int OffsetInBytes
        {
            get
            {
                if (buffer == null) return 0;
                return Get(ALGetSourcei.ByteOffset);
            }
        }

        public int OffsetInSamples
        {
            get
            {
                if (buffer == null) return 0;
                return Get(ALGetSourcei.SampleOffset);
            }
        }

        public double OffsetInSeconds
        {
            get
            {
                if (buffer == null) return 0;
                return Get(ALGetSourcei.SampleOffset) / (double)buffer.SampleCount
                    * buffer.DurationInSeconds;
            }
        }

        public TimeSpan Offset
        {
            get { return TimeSpan.FromSeconds(OffsetInSeconds); }
        }
        #endregion
        #endregion

        #region Methods
        public void Play()
        {
            AL.SourcePlay(handle);
            Debug.Assert(AL.GetError() == ALError.NoError);
        }

        public void Pause()
        {
            AL.SourcePause(handle);
            Debug.Assert(AL.GetError() == ALError.NoError);
        }

        public void Stop()
        {
            AL.SourceStop(handle);
            Debug.Assert(AL.GetError() == ALError.NoError);
        }

        public void Rewind()
        {
            AL.SourceRewind(handle);
            Debug.Assert(AL.GetError() == ALError.NoError);
        }

        public void SetVelocityAndDirection(Vector2 velocity)
        {
            this.Velocity = velocity;
            this.Direction = Vector2.NormalizeFast(velocity);
        }

        public void Dispose()
        {
            if (IsInUse) Stop();

            uint handle = this.handle;
            AL.DeleteSource(ref handle);
            Debug.Assert(AL.GetError() == ALError.NoError);
        }

        private void OnBufferDisposing(SoundBuffer buffer)
        {
            Debug.Assert(buffer == this.buffer);
            this.Buffer = null;
        }

        private Vector2 Get(ALSource3f param)
        {
            Vector3 value;
            AL.GetSource(handle, param, out value);
            Debug.Assert(AL.GetError() == ALError.NoError);
            return value.Xy;
        }

        private void Set(ALSource3f param, Vector2 value)
        {
            AL.Source(handle, param, value.X, value.Y, 0);
            Debug.Assert(AL.GetError() == ALError.NoError);
        }

        private float Get(ALSourcef param)
        {
            float value;
            AL.GetSource(handle, param, out value);
            Debug.Assert(AL.GetError() == ALError.NoError);
            return value;
        }

        private void Set(ALSourcef param, float value)
        {
            AL.Source(handle, param, value);
            Debug.Assert(AL.GetError() == ALError.NoError);
        }

        private int Get(ALGetSourcei param)
        {
            int value;
            AL.GetSource(handle, param, out value);
            Debug.Assert(AL.GetError() == ALError.NoError);
            return value;
        }

        private void Set(ALSourcei param, int value)
        {
            AL.Source(handle, param, value);
            Debug.Assert(AL.GetError() == ALError.NoError);
        }
        #endregion
    }
}
