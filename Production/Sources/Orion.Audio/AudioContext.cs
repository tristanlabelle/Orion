using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Audio;
using System.IO;
using System.Diagnostics;
using OpenTK.Math;

namespace Orion.Audio
{
    public sealed class AudioContext : IDisposable
    {
        #region Fields
        private readonly IntPtr deviceHandle;
        private readonly ContextHandle contextHandle;
        private readonly AudioSource[] sources;
        #endregion

        #region Constructors
        public AudioContext()
        {
            deviceHandle = Alc.OpenDevice(null);
            if (deviceHandle == null) return;

            contextHandle = Alc.CreateContext(deviceHandle, (int[])null);
            if (contextHandle == null) return;

            Alc.MakeContextCurrent(contextHandle);
            Debug.Assert(AL.GetError() == ALError.NoError);

            Alut.InitWithoutContext();
            Debug.Assert(AL.GetError() == ALError.NoError);

            sources = new AudioSource[32];
            for (int i = 0; i < sources.Length; ++i)
                sources[i] = new AudioSource();
        }
        #endregion

        #region Properties
        public string DeviceName
        {
            get
            {
                return contextHandle.Handle == IntPtr.Zero
                    ? null
                    : Alc.GetString(deviceHandle, AlcGetString.DefaultDeviceSpecifier);
            }
        }

        public bool IsDummy
        {
            get { return contextHandle.Handle == IntPtr.Zero; }
        }

        public Vector2 ListenerPosition
        {
            get { return GetListenerVector(ALListener3f.Position); }
            set { SetListenerVector(ALListener3f.Position, value); }
        }

        public Vector2 ListenerVelocity
        {
            get { return GetListenerVector(ALListener3f.Velocity); }
            set { SetListenerVector(ALListener3f.Velocity, value); }
        }

        public float ListenerGain
        {
            get
            {
                float value;
                AL.GetListener(ALListenerf.Gain, out value);
                Debug.Assert(AL.GetError() != ALError.NoError);
                return value;
            }
            set
            {
                AL.Listener(ALListenerf.Gain, value);
                Debug.Assert(AL.GetError() != ALError.NoError);
            }
        }

        public float SpeedOfSound
        {
            get
            {
                float value = AL.Get(ALGetFloat.SpeedOfSound);
                Debug.Assert(AL.GetError() != ALError.NoError);
                return value;
            }
            set
            {
                AL.SpeedOfSound(value);
                Debug.Assert(AL.GetError() != ALError.NoError);
            }
        }
        #endregion

        #region Methods
        public bool Play(string name)
        {
            if (IsDummy) return false;

            string path = "../../../Assets/Sounds/" + name + ".wav";
            SoundBuffer soundBuffer = new SoundBuffer(path);

            AudioSource source = sources.FirstOrDefault(s => !s.IsInUse);
            if (source == null) return false;

            source.Buffer = soundBuffer;
            source.Play();

            return true;
        }

        public void StopAllSounds()
        {
            foreach (AudioSource source in sources)
                if (source.IsInUse)
                    source.Stop();
        }

        public void Dispose()
        {
            for (int i = 0; i < sources.Length; ++i)
            {
                sources[i].Dispose();
                sources[i] = null;
            }

            Alc.MakeContextCurrent(ContextHandle.Zero);
            if (contextHandle != null) Alc.DestroyContext(contextHandle);
            if (deviceHandle != null) Alc.CloseDevice(deviceHandle);
        }

        private Vector2 GetListenerVector(ALListener3f param)
        {
            Vector3 value;
            AL.GetListener(param, out value);
            Debug.Assert(AL.GetError() == ALError.NoError);
            return value.Xy;
        }

        private void SetListenerVector(ALListener3f param, Vector2 value)
        {
            AL.Listener(param, value.X, value.Y, 0);
            Debug.Assert(AL.GetError() == ALError.NoError);
        }
        #endregion
    }
}
