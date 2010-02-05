using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using OpenTK.Audio;
using System.Diagnostics;

namespace Orion.Audio
{
    internal sealed class SoundBuffer : IDisposable
    {
        #region Fields
        private readonly uint handle;
        private readonly string name;
        #endregion

        #region Constructors
        public SoundBuffer(string filePath)
        {
            Argument.EnsureNotNull(filePath, "filePath");

            handle = Alut.CreateBufferFromFile(filePath);
            Debug.Assert(handle != 0);
            name = Path.GetFileNameWithoutExtension(filePath);
        }
        #endregion

        #region Events
        public event GenericEventHandler<SoundBuffer> Disposing;

        private void RaiseDisposing()
        {
            if (Disposing != null) Disposing(this);
        }
        #endregion

        #region Properties
        public uint Handle
        {
            get { return handle; }
        }

        public int BitsPerSample
        {
            get { return Get(ALGetBufferi.Bits); }
        }

        public int ChannelCount
        {
            get { return Get(ALGetBufferi.Channels); }
        }

        public int FrequencyInHertz
        {
            get { return Get(ALGetBufferi.Frequency); }
        }

        public int SizeInBytes
        {
            get { return Get(ALGetBufferi.Size); }
        }

        public int SizeInBits
        {
            get { return SizeInBytes * 8; }
        }

        public int SampleCount
        {
            get { return SizeInBits / BitsPerSample; }
        }

        public double DurationInSeconds
        {
            get { return SampleCount / (double)FrequencyInHertz; }
        }

        public TimeSpan Duration
        {
            get { return TimeSpan.FromSeconds(DurationInSeconds); }
        }
        #endregion

        #region Methods
        public void Dispose()
        {
            RaiseDisposing();

            uint handle = this.handle;
            AL.DeleteBuffer(ref handle);
            Debug.Assert(AL.GetError() == ALError.NoError);
        }

        private int Get(ALGetBufferi param)
        {
            int value;
            AL.GetBuffer(handle, param, out value);
            Debug.Assert(AL.GetError() == ALError.NoError);
            return value;
        }
        #endregion
    }
}
