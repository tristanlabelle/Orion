using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using OpenTK;
using OpenTK.Audio.OpenAL;

namespace Orion.Engine.Audio.OpenAL
{
    /// <summary>
    /// An OpenAL sound context.
    /// </summary>
    public sealed class SoundContext : ISoundContext
    {
        #region Fields
        private readonly ALDevice deviceHandle;
        private readonly ALContext contextHandle;
        private bool isMuted;
        private float volume = 1;
        #endregion

        #region Constructors
        public SoundContext()
        {
            try
            {
                deviceHandle = ALC.OpenDevice(null);
                contextHandle = ALC.CreateContext(deviceHandle, (int[])null);
                ALC.MakeContextCurrent(contextHandle);
                if (ALC.GetError(deviceHandle) != AlcError.NoError) throw new NotSupportedException("OpenAL is fucked up.");
            }
            catch
            {
                if (contextHandle != ALContext.Null)
                {
                    if (ALC.GetCurrentContext() == contextHandle)
                        ALC.MakeContextCurrent(ALContext.Null);
                    ALC.DestroyContext(contextHandle);
                }

                if (deviceHandle != IntPtr.Zero) ALC.CloseDevice(deviceHandle);
                throw;
            }
        }
        #endregion

        #region Properties
        public bool IsMuted
        {
            get { return isMuted; }
            set
            {
                if (value == isMuted) return;

                AL.Listener(ALListenerf.Gain, isMuted ? 0 : volume);
                isMuted = value;
            }
        }

        public float Volume
        {
            get { return volume; }
            set
            {
                Argument.EnsureWithin(value, 0, 1, "Volume");
                volume = value;
                if (!isMuted) AL.Listener(ALListenerf.Gain, volume);
            }
        }

        public Matrix4 ListenerMatrix
        {
            get
            {
                Vector3 position, at, up;
                AL.GetListener(ALListener3f.Position, out position);
                AL.GetListener(ALListenerfv.Orientation, out at, out up);
                Vector3 right = Vector3.Cross(at, up);
                return new Matrix4(
                    right.X, right.Y, right.Z, 0,
                    up.X, up.Y, up.Z, 0,
                    at.X, at.Y, at.Z, 0,
                    -position.X, position.Y, position.Z, 1);
            }
            set
            {
                AL.Listener(ALListener3f.Position, -value.M41, value.M42, value.M43);
                Vector3 at = new Vector3(value.M31, value.M32, value.M33);
                Vector3 up = new Vector3(value.M21, value.M22, value.M23);
                AL.Listener(ALListenerfv.Orientation, ref at, ref up);
            }
        }

        public bool IsSoundLoadingThreadSafe
        {
            get { return true; } // OpenAL is supposed to be thread-safe from version 1.1
        }
        #endregion

        #region Methods
        public ISound LoadSoundFromFile(string filePath)
        {
            using (Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                SoundBuffer soundBuffer = SoundBuffer.FromOggVorbis(stream, SoundSampleFormat.SignedShort);
                int bufferHandle = AL.GenBuffer();

                ALFormat format = soundBuffer.ChannelCount == 2 ? ALFormat.Stereo16 : ALFormat.Mono16;
                GCHandle pinningHandle = GCHandle.Alloc(soundBuffer.Buffer, GCHandleType.Pinned);
                try
                {
                    AL.BufferData(bufferHandle, format, pinningHandle.AddrOfPinnedObject(),
                        soundBuffer.Buffer.Length, (int)soundBuffer.Frequency);
                }
                finally
                {
                    pinningHandle.Free();
                }

                return new Sound(bufferHandle);
            }
        }

        public ISoundChannel CreateChannel()
        {
            return new SoundChannel();
        }

        public void Dispose()
        {
            ALC.DestroyContext(contextHandle);
            ALC.CloseDevice(deviceHandle);
        }
        #endregion
    }
}
