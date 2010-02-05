using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Audio;
using System.IO;
using System.Diagnostics;

namespace Orion.Audio
{
    public sealed class AudioContext : IDisposable
    {
        #region Fields
        private readonly IntPtr deviceHandle;
        private readonly ContextHandle contextHandle;
        #endregion

        #region Constructors
        public AudioContext(string directory)
        {
            Argument.EnsureNotNull(directory, "directory");

            deviceHandle = Alc.OpenDevice(null);
            if (deviceHandle == null) return;

            contextHandle = Alc.CreateContext(deviceHandle, (int[])null);
            if (contextHandle == null) return;

            Alc.MakeContextCurrent(contextHandle);
            Debug.Assert(AL.GetError() == ALError.NoError);

            Alut.InitWithoutContext();
            Debug.Assert(AL.GetError() == ALError.NoError);
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
        #endregion

        #region Methods
        public void Dispose()
        {
            if (contextHandle != null) Alc.DestroyContext(contextHandle);
            if (deviceHandle != null) Alc.CloseDevice(deviceHandle);
        }
        #endregion
    }
}
