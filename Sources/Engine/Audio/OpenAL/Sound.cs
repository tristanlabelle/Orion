using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Audio.OpenAL;

namespace Orion.Engine.Audio.OpenAL
{
    /// <summary>
    /// OpenAL implementation of the <see cref="ISound"/> interface.
    /// </summary>
    internal sealed class Sound : ISound
    {
        #region Fields
        private readonly int handle;
        #endregion

        #region Constructors
        public Sound(int handle)
        {
            this.handle = handle;
        }
        #endregion

        #region Properties
        public int SizeInBytes
        {
            get
            {
                int value;
                AL.GetBuffer(handle, ALGetBufferi.Size, out value);
                return value;
            }
        }

        public int BitDepth
        {
            get
            {
                int value;
                AL.GetBuffer(handle, ALGetBufferi.Bits, out value);
                return value;
            }
        }

        public int Frequency
        {
            get
            {
                int value;
                AL.GetBuffer(handle, ALGetBufferi.Frequency, out value);
                return value;
            }
        }

        public int SampleCount
        {
            get { return SizeInBytes / ChannelCount / (BitDepth / 8); }
        }

        public TimeSpan Duration
        {
            get
            {
                double durationInSeconds = SampleCount / (double)Frequency;
                return TimeSpan.FromSeconds(durationInSeconds);
            }
        }

        public int ChannelCount
        {

            get
            {
                int value;
                AL.GetBuffer(handle, ALGetBufferi.Channels, out value);
                return value;
            }
        }

        public int Handle
        {
            get { return handle; }
        }
        #endregion

        #region Methods
        public void Dispose()
        {
            AL.DeleteBuffer(handle);
        }
        #endregion
    }
}
