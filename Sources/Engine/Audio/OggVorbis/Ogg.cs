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

namespace Orion.Engine.Audio.OggVorbis
{
    /// <summary>
    /// Provides structures, delegates, constants and methods defined in the
    /// "ogg.h" C header file and exported in the "ogg.dll" library.
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    public static class Ogg
    {
        #region Structures
        #region oggpack_buffer
        [StructLayout(LayoutKind.Sequential)]
        public struct oggpack_buffer
        {
            public int endbyte;
            public int endbit;
            public IntPtr buffer;
            public IntPtr ptr;
            public int storage;
        }
        #endregion

        #region ogg_page
        /// <summary>
        /// ogg_page is used to encapsulate the data in one Ogg bitstream page
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct ogg_page
        {
            public IntPtr header;
            public int header_len;
            public IntPtr body;
            public int body_len;
        }
        #endregion

        #region ogg_stream_state
        /// <summary>
        /// ogg_stream_state contains the current encode/decode state of a logical Ogg bitstream
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct ogg_stream_state
        {
            /// <summary>
            /// bytes from packet bodies
            /// </summary>
            public IntPtr body_data;

            /// <summary>
            /// storage elements allocated
            /// </summary>
            public int body_storage;

            /// <summary>
            /// elements stored; fill mark
            /// </summary>
            public int body_fill;

            /// <summary>
            /// elements of fill returned
            /// </summary>
            public int body_returned;

            /// <summary>
            /// The values that will go to the segment table
            /// </summary>
            public IntPtr lacing_vals;

            /// <summary>
            /// granulepos values for headers. Not compact this way,
            /// but it is simple coupled to the lacing fifo
            /// </summary>
            public IntPtr granule_vals;
            public int lacing_storage;
            public int lacing_fill;
            public int lacing_packet;
            public int lacing_returned;

            /// <summary>
            /// working space for header encode
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 282)]
            public byte[] header;
            public int header_fill;

            /// <summary>
            /// set when we have buffered the last packet in the logical bitstream
            /// </summary>
            public int e_o_s;

            /// <summary>
            /// set after we've written the initial page of a logical bitstream
            /// </summary>
            public int b_o_s;
            public int serialno;
            public int pageno;

            /// <summary>
            /// sequence number for decode; the framing knows where there's a hole in the data,
            /// but we need coupling so that the codec (which is in a seperate abstraction layer)
            /// also knows about the gap
            /// </summary>
            public ogg_int64_t packetno;
            public ogg_int64_t granulepos;
        }
        #endregion

        #region ogg_packet
        /// <summary>
        /// ogg_packet is used to encapsulate the data and metadata belonging
        /// to a single raw Ogg/Vorbis packet
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct ogg_packet
        {
            public IntPtr packet;
            public int bytes;
            public int b_o_s;
            public int e_o_s;
            public ogg_int64_t granulepos;

            /// <summary>
            /// sequence number for decode; the framing knows where there's a hole in the data,
            /// but we need coupling so that the codec (which is in a seperate abstraction layer)
            /// also knows about the gap
            /// </summary>
            public ogg_int64_t packetno;
        }
        #endregion

        #region ogg_sync_state
        [StructLayout(LayoutKind.Sequential)]
        public struct ogg_sync_state
        {
            public IntPtr data;
            public int storage;
            public int fill;
            public int returned;
            public int unsynced;
            public int headerbytes;
            public int bodybytes;
        }
        #endregion
        #endregion

        #region Fields
        private const string libraryName = "ogg.dll";
        #endregion

        #region Methods
        #region External
        #region oggpack
        [DllImport(libraryName)]
        public static extern void oggpack_writeinit(ref oggpack_buffer b);
        [DllImport(libraryName)]
        public static extern void oggpack_writetrunc(ref oggpack_buffer b, long bits);
        [DllImport(libraryName)]
        public static extern void oggpack_writealign(ref oggpack_buffer b);
        [DllImport(libraryName)]
        public static extern void oggpack_writecopy(ref oggpack_buffer b, IntPtr source, long bits);
        [DllImport(libraryName)]
        public static extern void oggpack_reset(ref oggpack_buffer b);
        [DllImport(libraryName)]
        public static extern void oggpack_writeclear(ref oggpack_buffer b);
        [DllImport(libraryName)]
        public static extern void oggpack_readinit(ref oggpack_buffer b, IntPtr buf,int bytes);
        [DllImport(libraryName)]
        public static extern void oggpack_write(ref oggpack_buffer b, int value, int bits);
        [DllImport(libraryName)]
        public static extern long oggpack_look(ref oggpack_buffer b, int bits);
        [DllImport(libraryName)]
        public static extern long oggpack_look1(ref oggpack_buffer b);
        [DllImport(libraryName)]
        public static extern void oggpack_adv(ref oggpack_buffer b, int bits);
        [DllImport(libraryName)]
        public static extern void oggpack_adv1(ref oggpack_buffer b);
        [DllImport(libraryName)]
        public static extern long oggpack_read(ref oggpack_buffer b, int bits);
        [DllImport(libraryName)]
        public static extern long oggpack_read1(ref oggpack_buffer b);
        [DllImport(libraryName)]
        public static extern long oggpack_bytes(ref oggpack_buffer b);
        [DllImport(libraryName)]
        public static extern long oggpack_bits(ref oggpack_buffer b);
        [DllImport(libraryName)]
        public static extern IntPtr oggpack_get_buffer(ref oggpack_buffer b);
        #endregion

        #region oggpackB
        [DllImport(libraryName)]
        public static extern void oggpackB_writeinit(ref oggpack_buffer b);
        [DllImport(libraryName)]
        public static extern void oggpackB_writetrunc(ref oggpack_buffer b, long bits);
        [DllImport(libraryName)]
        public static extern void oggpackB_writealign(ref oggpack_buffer b);
        [DllImport(libraryName)]
        public static extern void oggpackB_writecopy(ref oggpack_buffer b, IntPtr source,long bits);
        [DllImport(libraryName)]
        public static extern void oggpackB_reset(ref oggpack_buffer b);
        [DllImport(libraryName)]
        public static extern void oggpackB_writeclear(ref oggpack_buffer b);
        [DllImport(libraryName)]
        public static extern void oggpackB_readinit(ref oggpack_buffer b, IntPtr buf, int bytes);
        [DllImport(libraryName)]
        public static extern void oggpackB_write(ref oggpack_buffer b, int value, int bits);
        [DllImport(libraryName)]
        public static extern long oggpackB_look(ref oggpack_buffer b, int bits);
        [DllImport(libraryName)]
        public static extern long oggpackB_look1(ref oggpack_buffer b);
        [DllImport(libraryName)]
        public static extern void oggpackB_adv(ref oggpack_buffer b, int bits);
        [DllImport(libraryName)]
        public static extern void oggpackB_adv1(ref oggpack_buffer b);
        [DllImport(libraryName)]
        public static extern long oggpackB_read(ref oggpack_buffer b,int bits);
        [DllImport(libraryName)]
        public static extern long oggpackB_read1(ref oggpack_buffer b);
        [DllImport(libraryName)]
        public static extern long oggpackB_bytes(ref oggpack_buffer b);
        [DllImport(libraryName)]
        public static extern long oggpackB_bits(ref oggpack_buffer b);
        [DllImport(libraryName)]
        public static extern IntPtr oggpackB_get_buffer(ref oggpack_buffer b);
        #endregion

        #region BITSTREAM PRIMITIVES: encoding
        [DllImport(libraryName)]
        public static extern int ogg_stream_packetin(ref ogg_stream_state os, ref ogg_packet op);
        [DllImport(libraryName)]
        public static extern int ogg_stream_pageout(ref ogg_stream_state os, ref ogg_page og);
        [DllImport(libraryName)]
        public static extern int ogg_stream_flush(ref ogg_stream_state os, ref ogg_page og);
        #endregion

        #region Ogg BITSTREAM PRIMITIVES: decoding
        [DllImport(libraryName)]
        public static extern int ogg_sync_init(ref ogg_sync_state oy);
        [DllImport(libraryName)]
        public static extern int ogg_sync_clear(ref ogg_sync_state oy);
        [DllImport(libraryName)]
        public static extern int ogg_sync_reset(ref ogg_sync_state oy);
        [DllImport(libraryName)]
        public static extern int ogg_sync_destroy(ref ogg_sync_state oy);
        [DllImport(libraryName)]
        public static extern IntPtr ogg_sync_buffer(ref ogg_sync_state oy, long size);
        [DllImport(libraryName)]
        public static extern int ogg_sync_wrote(ref ogg_sync_state oy, long bytes);
        [DllImport(libraryName)]
        public static extern long ogg_sync_pageseek(ref ogg_sync_state oy, ref ogg_page og);
        [DllImport(libraryName)]
        public static extern int ogg_sync_pageout(ref ogg_sync_state oy, ref ogg_page og);
        [DllImport(libraryName)]
        public static extern int ogg_stream_pagein(ref ogg_stream_state os, ref ogg_page og);
        [DllImport(libraryName)]
        public static extern int ogg_stream_packetout(ref ogg_stream_state os, ref ogg_packet op);
        [DllImport(libraryName)]
        public static extern int ogg_stream_packetpeek(ref ogg_stream_state os, ref ogg_packet op);
        #endregion

        #region Ogg BITSTREAM PRIMITIVES: general
        [DllImport(libraryName)]
        public static extern int ogg_stream_init(ref ogg_stream_state os, int serialno);
        [DllImport(libraryName)]
        public static extern int ogg_stream_clear(ref ogg_stream_state os);
        [DllImport(libraryName)]
        public static extern int ogg_stream_reset(ref ogg_stream_state os);
        [DllImport(libraryName)]
        public static extern int ogg_stream_reset_serialno(ref ogg_stream_state os, int serialno);
        [DllImport(libraryName)]
        public static extern int ogg_stream_destroy(ref ogg_stream_state os);
        [DllImport(libraryName)]
        public static extern int ogg_stream_eos(ref ogg_stream_state os);

        [DllImport(libraryName)]
        public static extern void ogg_page_checksum_set(ref ogg_page og);

        [DllImport(libraryName)]
        public static extern int ogg_page_version(ref ogg_page og);
        [DllImport(libraryName)]
        public static extern int ogg_page_continued(ref ogg_page og);
        [DllImport(libraryName)]
        public static extern int ogg_page_bos(ref ogg_page og);
        [DllImport(libraryName)]
        public static extern int ogg_page_eos(ref ogg_page og);
        [DllImport(libraryName)]
        public static extern ogg_int64_t ogg_page_granulepos(ref ogg_page og);
        [DllImport(libraryName)]
        public static extern int ogg_page_serialno(ref ogg_page og);
        [DllImport(libraryName)]
        public static extern long ogg_page_pageno(ref ogg_page og);
        [DllImport(libraryName)]
        public static extern int ogg_page_packets(ref ogg_page og);

        [DllImport(libraryName)]
        public static extern void ogg_packet_clear(ref ogg_packet op);
        #endregion
        #endregion
        #endregion
    }
}
