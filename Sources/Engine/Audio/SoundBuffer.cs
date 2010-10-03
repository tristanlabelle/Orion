using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Collections;
using System.IO;
using Orion.Engine.Audio.OggVorbis;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Orion.Engine.Audio
{
    /// <summary>
    /// A buffer of sound samples.
    /// </summary>
    public sealed class SoundBuffer
    {
        #region Fields
        public readonly SoundSampleFormat SampleFormat;
        public readonly int ChannelCount;
        public readonly double Frequency;
        public readonly byte[] Buffer;
        #endregion

        #region Constructors
        public SoundBuffer(SoundSampleFormat sampleFormat, int channelCount, double frequency, int sampleCount)
        {
            Argument.EnsureStrictlyPositive(channelCount, "channelCount");
            Argument.EnsureStrictlyPositive(frequency, "frequency");
            Argument.EnsurePositive(sampleCount, "sampleCount");

            this.SampleFormat = sampleFormat;
            this.ChannelCount = channelCount;
            this.Frequency = frequency;
            this.Buffer = new byte[sampleFormat.SizeInBytes * channelCount * sampleCount];
        }
        #endregion

        #region Properties
        public int SampleCount
        {
            get { return Buffer.Length / ChannelCount / SampleFormat.SizeInBytes; }
        }
        #endregion

        #region Methods
        #endregion

        #region Ogg vorbis
        public static SoundBuffer FromOggVorbis(Stream stream, SoundSampleFormat sampleFormat)
        {
            Argument.EnsureNotNull(stream, "stream");
            Argument.EnsureEqual(stream.CanRead, true, "stream.CanRead");

            byte[] buffer = new byte[64];

            VorbisFile.ov_callbacks callbacks = new VorbisFile.ov_callbacks
            {
                read_func = (IntPtr ptr, IntPtr size, IntPtr nmemb, IntPtr datasource) =>
                {
                    int length = (int)size * (int)nmemb;
                    if (buffer.Length < length) buffer = new byte[length];
                    int bytesRead = stream.Read(buffer, 0, length);
                    Marshal.Copy(buffer, 0, ptr, bytesRead);
                    return (IntPtr)bytesRead;
                },
                tell_func = (IntPtr datasource) => checked((int)stream.Position),
                seek_func = (IntPtr datasource, long offset, int whence) =>
                {
                    if (whence == 0) return (int)stream.Seek(offset, SeekOrigin.Begin);
                    if (whence == 1) return (int)stream.Seek(offset, SeekOrigin.Current);
                    if (whence == 2) return (int)stream.Seek(offset, SeekOrigin.End);

                    // This is not a good place to throw a managed exception
                    Debug.Fail("Unexpected seek origin in Ogg Vorbis callback.");
                    return -1;
                },
            };

            VorbisFile.OggVorbis_File file = new VorbisFile.OggVorbis_File();
            int result = VorbisFile.ov_open_callbacks((IntPtr)1, ref file, IntPtr.Zero, 0, callbacks);
            if (result != 0) throw VorbisFile.GetErrorException(result, "ov_open_callbacks");

            try
            {
                long sampleCount = VorbisFile.ov_pcm_total(ref file, -1);
                double duration = VorbisFile.ov_time_total(ref file, -1);
                IntPtr infoPtr = VorbisFile.ov_info(ref file, -1);
                Vorbis.vorbis_info info = (Vorbis.vorbis_info)Marshal.PtrToStructure(infoPtr, typeof(Vorbis.vorbis_info));

                double frequency = sampleCount / duration;
                SoundBuffer soundBuffer = new SoundBuffer(sampleFormat, info.channels, frequency, (int)sampleCount);

                GCHandle pinningHandle = GCHandle.Alloc(soundBuffer.Buffer, GCHandleType.Pinned);
                try
                {
                    IntPtr ptr = pinningHandle.AddrOfPinnedObject();
                    int byteOffset = 0;
                    while (byteOffset < soundBuffer.Buffer.Length)
                    {
                        int bitstream = 0;
                        result = VorbisFile.ov_read(ref file, (IntPtr)((long)ptr + byteOffset),
                            soundBuffer.Buffer.Length - byteOffset, 0, sampleFormat.SizeInBytes,
                            sampleFormat.Type == SoundSampleType.Signed ? 1 : 0, ref bitstream);
                        if (result < 0) throw VorbisFile.GetErrorException(result, "ov_read");
                        if (result == 0)
                        {
                            Debug.Fail("The ogg vorbis file contains less stuff than expected.");
                            break;
                        }

                        byteOffset += result;
                    }
                }
                finally
                {
                    pinningHandle.Free();
                }

                return soundBuffer;
            }
            finally
            {
                VorbisFile.ov_clear(ref file);
            }
        }
        #endregion
    }
}
