using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Audio;
using OpenTK.Math;

namespace Orion.Engine.Audio.OpenAL
{
    /// <summary>
    /// An OpenAL sound context.
    /// </summary>
    public sealed class SoundContext : ISoundContext
    {
        #region Fields
        private readonly IntPtr deviceHandle;
        private readonly ContextHandle contextHandle;
        private bool isMuted;
        private float volume;
        #endregion

        #region Constructors
        public SoundContext()
        {
            try
            {
                AlcGetString defaultDeviceFlag = Alc.IsExtensionPresent(IntPtr.Zero, "ALC_ENUMERATE_ALL_EXT")
                    ? AlcGetString.DefaultAllDevicesSpecifier : AlcGetString.DefaultDeviceSpecifier;
                string defaultDeviceName = Alc.GetString(IntPtr.Zero, defaultDeviceFlag);

                deviceHandle = Alc.OpenDevice(defaultDeviceName);
                contextHandle = Alc.CreateContext(deviceHandle, (int[])null);
                Alc.MakeContextCurrent(contextHandle);
                if (Alc.GetError(deviceHandle) != AlcError.NoError) throw new NotSupportedException("OpenAL is fucked up.");
            }
            catch
            {
                if (contextHandle != ContextHandle.Zero)
                {
                    if (Alc.GetCurrentContext() == contextHandle)
                        Alc.MakeContextCurrent(ContextHandle.Zero);
                    Alc.DestroyContext(contextHandle);
                }

                if (deviceHandle != IntPtr.Zero) Alc.CloseDevice(deviceHandle);
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
                    position.X, position.Y, position.Z, 1);
            }
            set
            {
                AL.Listener(ALListener3f.Position, value.M41, value.M42, value.M43);
                Vector3 at = new Vector3(value.M31, value.M32, value.M33);
                Vector3 up = new Vector3(value.M21, value.M22, value.M23);
                AL.Listener(ALListenerfv.Orientation, ref at, ref up);
            }
        }
        #endregion

        #region Methods
        public ISound LoadSoundFromFile(string filePath)
        {
            throw new NotImplementedException();
        }

        public ISoundChannel CreateChannel()
        {
            return new SoundChannel();
        }

        public void Dispose()
        {
            Alc.DestroyContext(contextHandle);
            Alc.CloseDevice(deviceHandle);
        }
        #endregion
    }
}
