using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;

using ogg_int16_t = System.Int16;
using ogg_int32_t = System.Int32;
using ogg_int64_t = System.Int64;
using ogg_uint16_t = System.Int16;
using ogg_uint32_t = System.Int32;
using size_t = System.IntPtr;

namespace Orion.Engine.Audio.OggVorbis
{
    /// <summary>
    /// Provides structures, delegates, constants and methods defined in the
    /// "vorbisfile.h" C header file and exported in the "vorbisfile.dll" library.
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    public static class VorbisFile
    {
        #region Nested Types
        #region Structures
        #region ov_callbacks
        [StructLayout(LayoutKind.Sequential)]
        public struct ov_callbacks
        {
            public read_func read_func;
            public seek_func seek_func;
            public close_func close_func;
            public tell_func tell_func;
        }
        #endregion

        #region OggVorbis_File
        [StructLayout(LayoutKind.Sequential)]
        public struct OggVorbis_File
        {
            /// <summary>
            /// Pointer to a FILE *, etc.
            /// </summary>
            public IntPtr datasource;
            public int seekable;
            public ogg_int64_t offset;
            public ogg_int64_t end;
            public Ogg.ogg_sync_state oy;

            /* If the FILE handle isn't seekable (eg, a pipe), only the current
               stream appears */
            public int links;
            public IntPtr offsets;
            public IntPtr dataoffsets;
            public IntPtr serialnos;
            public IntPtr pcmlengths; /* overloaded to maintain binary
                  compatability; x2 size, stores both
                  beginning and end values */
            public IntPtr vi;
            public IntPtr vc;

            /* Decoding working state local storage */
            public ogg_int64_t pcm_offset;
            public int ready_state;
            public int current_serialno;
            public int current_link;

            public double bittrack;
            public double samptrack;

            public Ogg.ogg_stream_state os; /* take physical pages, weld into a logical
                          stream of packets */
            public Vorbis.vorbis_dsp_state vd; /* central working state for the packet->PCM decoder */
            public Vorbis.vorbis_block vb; /* local working space for packet->PCM decode */

            public ov_callbacks callbacks;
        }
        #endregion
        #endregion

        #region Delegates
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate size_t read_func(IntPtr ptr, size_t size, size_t nmemb, IntPtr datasource);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int seek_func(IntPtr datasource, ogg_int64_t offset, int whence);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int close_func(IntPtr datasource);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int tell_func(IntPtr datasource);
        #endregion
        #endregion

        #region Fields
        private const string libraryName = "vorbisfile.dll";
        private static readonly Dictionary<int, string> errorMessages;

        #region Constants
        public const int NOTOPEN = 0;
        public const int PARTOPEN = 1;
        public const int OPENED = 2;
        public const int STREAMSET = 3;
        public const int INITSET = 4;
        #endregion
        #endregion

        #region Constructors
        static VorbisFile()
        {
            errorMessages = new Dictionary<int, string>();
            errorMessages.Add(Vorbis.OV_EBADHEADER, "invalid header");
            errorMessages.Add(Vorbis.OV_EBADLINK, "invalid link");
            errorMessages.Add(Vorbis.OV_EBADPACKET, "invalid packet");
            errorMessages.Add(Vorbis.OV_EFAULT, "segmentation fault");
            errorMessages.Add(Vorbis.OV_EIMPL, "not implemented");
            errorMessages.Add(Vorbis.OV_EINVAL, "invalid file headers");
            errorMessages.Add(Vorbis.OV_ENOSEEK, "stream not seekable");
            errorMessages.Add(Vorbis.OV_ENOTAUDIO, "ogg file does not contain audio");
            errorMessages.Add(Vorbis.OV_ENOTVORBIS, "audio data is not in vorbis format");
            errorMessages.Add(Vorbis.OV_EOF, "end of file streached prematurely");
            errorMessages.Add(Vorbis.OV_EREAD, "read error");
            errorMessages.Add(Vorbis.OV_EVERSION, "invalid version");
            errorMessages.Add(Vorbis.OV_HOLE, "data was interrupted");
        }
        #endregion

        #region Methods
        #region External
        [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ov_clear(ref OggVorbis_File vf);
        [DllImport(libraryName, CharSet = CharSet.Ansi)]
        public static extern int ov_fopen(string path, ref OggVorbis_File vf);
        [DllImport(libraryName, CharSet = CharSet.Ansi)]
        public static extern int ov_open(IntPtr f, ref OggVorbis_File vf, IntPtr initial, int ibytes);
        [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ov_open_callbacks(IntPtr datasource, ref OggVorbis_File vf,
            IntPtr initial, int ibytes, ov_callbacks callbacks);

        [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ov_test(IntPtr f, ref OggVorbis_File vf, IntPtr initial, int ibytes);
        [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ov_test_callbacks(IntPtr datasource, ref OggVorbis_File vf,
                IntPtr initial, int ibytes, ov_callbacks callbacks);
        [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ov_test_open(ref OggVorbis_File vf);

        [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ov_bitrate(ref OggVorbis_File vf, int i);
        [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ov_bitrate_instant(ref OggVorbis_File vf);
        [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ov_streams(ref OggVorbis_File vf);
        [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ov_seekable(ref OggVorbis_File vf);
        [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ov_serialnumber(ref OggVorbis_File vf, int i);

        [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern ogg_int64_t ov_raw_total(ref OggVorbis_File vf, int i);
        [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern ogg_int64_t ov_pcm_total(ref OggVorbis_File vf, int i);
        [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern double ov_time_total(ref OggVorbis_File vf, int i);

        [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ov_raw_seek(ref OggVorbis_File vf, ogg_int64_t pos);
        [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ov_pcm_seek(ref OggVorbis_File vf, ogg_int64_t pos);
        [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ov_pcm_seek_page(ref OggVorbis_File vf, ogg_int64_t pos);
        [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ov_time_seek(ref OggVorbis_File vf, double pos);
        [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ov_time_seek_page(ref OggVorbis_File vf, double pos);

        [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ov_raw_seek_lap(ref OggVorbis_File vf, ogg_int64_t pos);
        [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ov_pcm_seek_lap(ref OggVorbis_File vf, ogg_int64_t pos);
        [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ov_pcm_seek_page_lap(ref OggVorbis_File vf, ogg_int64_t pos);
        [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ov_time_seek_lap(ref OggVorbis_File vf, double pos);
        [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ov_time_seek_page_lap(ref OggVorbis_File vf, double pos);

        [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern ogg_int64_t ov_raw_tell(ref OggVorbis_File vf);
        [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern ogg_int64_t ov_pcm_tell(ref OggVorbis_File vf);
        [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern double ov_time_tell(ref OggVorbis_File vf);

        [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ov_info(ref OggVorbis_File vf, int link);
        [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ov_comment(ref OggVorbis_File vf, int link);

        [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ov_read_float(ref OggVorbis_File vf, IntPtr pcm_channels, int samples,
                      IntPtr bitstream);
        [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ov_read(ref OggVorbis_File vf, IntPtr buffer, int length,
                    int bigendianp, int word, int sgned, ref int bitstream);
        [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ov_crosslap(ref OggVorbis_File vf1, ref OggVorbis_File vf2);

        [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ov_halfrate(ref OggVorbis_File vf, int flag);
        [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ov_halfrate_p(ref OggVorbis_File vf);
        #endregion

        #region Helpers
        #region Errors
        #region GetErrorString
        /// <summary>
        /// Gets a string describing an error.
        /// </summary>
        /// <param name="errorCode">The error code to be described.</param>
        /// <returns>A string describing that error, or null if the error code is unknown.</returns>
        public static string GetErrorString(int errorCode)
        {
            string message;
            errorMessages.TryGetValue(errorCode, out message);
            return message;
        }
        #endregion

        #region GetErrorException
        /// <summary>
        /// Gets an exception describing an error.
        /// </summary>
        /// <param name="errorCode">The error code to be wrapped in an exception.</param>
        /// <param name="methodName">The name of the method which caused the exception.</param>
        /// <returns>The resulting exception.</returns>
        public static NotSupportedException GetErrorException(int errorCode, string methodName)
        {
            string message = "Error #{0} in OggVorbisFile.{1}: {2}."
                .FormatInvariant(errorCode, methodName, GetErrorString(errorCode));

            return new NotSupportedException(message);
        }
        #endregion
        #endregion
        #endregion
        #endregion
    }
}
