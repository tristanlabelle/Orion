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
    /// "codec.h" C header file and exported in the "vorbis.dll" library.
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    public static class Vorbis
    {
        #region Structures
        #region vorbis_info
        [StructLayout(LayoutKind.Sequential)]
        public struct vorbis_info
        {
            public int version;
            public int channels;
            public int rate;

            /* The below bitrate declarations are *hints*.
               Combinations of the three values carry the following implications:

               all three set to the same value:
                 implies a fixed rate bitstream
               only nominal set:
                 implies a VBR stream that averages the nominal bitrate.  No hard
                 upper/lower limit
               upper and or lower set:
                 implies a VBR bitstream that obeys the bitrate limits. nominal
                 may also be set to give a nominal rate.
               none set:
                 the coder does not care to speculate.
            */

            public int bitrate_upper;
            public int bitrate_nominal;
            public int bitrate_lower;
            public int bitrate_window;

            public IntPtr codec_setup;
        }
        #endregion

        #region vorbis_dsp_state
        /// <summary>
        /// vorbis_dsp_state buffers the current vorbis audio analysis/synthesis state.
        /// The DSP state belongs to a specific logical bitstream
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct vorbis_dsp_state
        {
            public int analysisp;
            public IntPtr vi;

            public IntPtr pcm;
            public IntPtr pcmret;
            public int pcm_storage;
            public int pcm_current;
            public int pcm_returned;

            public int preextrapolate;
            public int eofflag;

            public int lW;
            public int W;
            public int nW;
            public int centerW;

            public ogg_int64_t granulepos;
            public ogg_int64_t sequence;

            public ogg_int64_t glue_bits;
            public ogg_int64_t time_bits;
            public ogg_int64_t floor_bits;
            public ogg_int64_t res_bits;

            public IntPtr backend_state;
        }
        #endregion

        #region vorbis_block
        [StructLayout(LayoutKind.Sequential)]
        public struct vorbis_block
        {
            /* necessary stream state for linking to the framing abstraction */
            public IntPtr pcm;       /* this is a pointer into local storage */
            public Ogg.oggpack_buffer opb;

            public int lW;
            public int W;
            public int nW;
            public int pcmend;
            public int mode;

            public int eofflag;
            public ogg_int64_t granulepos;
            public ogg_int64_t sequence;
            public IntPtr vd; /* For read-only access of configuration */

            /* local storage to avoid remallocing; it's up to the mapping to
               structure it */
            public IntPtr localstore;
            public int localtop;
            public int localalloc;
            public int totaluse;
            public IntPtr reap;

            /* bitmetrics for the frame */
            public int glue_bits;
            public int time_bits;
            public int floor_bits;
            public int res_bits;

            public IntPtr @internal;
        }
        #endregion

        #region alloc_chain
        /// <summary>
        /// vorbis_block is a single block of data to be processed as part of
        /// the analysis/synthesis stream; it belongs to a specific logical
        /// bitstream, but is independant from other vorbis_blocks belonging to
        /// that logical bitstream.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct alloc_chain
        {
            public IntPtr ptr;
            public IntPtr next;
        }
        #endregion

        #region vorbis_comment
        /// <summary>
        /// vorbis_info contains all the setup information specific to the
        /// specific compression/decompression mode in progress (eg,
        /// psychoacoustic settings, channel setup, options, codebook
        /// etc). vorbis_info and substructures are in backends.h.
        /// </summary>
        /// <remarks>
        /// the comments are not part of vorbis_info so that vorbis_info
        /// can be static storage
        /// </remarks>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct vorbis_comment
        {
            /// <summary>
            /// unlimited user comment fields.  libvorbis writes 'libvorbis'
            /// whatever vendor is set to in encode
            /// </summary>
            public IntPtr user_comments;
            public IntPtr comment_lengths;
            public int comments;
            public string vendor;
        }
        #endregion
        #endregion

        #region Fields
        private const string libraryName = "vorbis.dll";

        #region Constants
        public const int OV_FALSE = -1;
        public const int OV_EOF = -2;
        public const int OV_HOLE = -3;

        public const int OV_EREAD = -128;
        public const int OV_EFAULT = -129;
        public const int OV_EIMPL = -130;
        public const int OV_EINVAL = -131;
        public const int OV_ENOTVORBIS = -132;
        public const int OV_EBADHEADER = -133;
        public const int OV_EVERSION = -134;
        public const int OV_ENOTAUDIO = -135;
        public const int OV_EBADPACKET = -136;
        public const int OV_EBADLINK = -137;
        public const int OV_ENOSEEK = -138;
        #endregion
        #endregion

        #region Methods
        #region External
        #region Vorbis PRIMITIVES: general
        [DllImport(libraryName)]
        public static extern void vorbis_info_init(ref vorbis_info vi);
        [DllImport(libraryName)]
        public static extern void vorbis_info_clear(ref vorbis_info vi);
        [DllImport(libraryName)]
        public static extern int vorbis_info_blocksize(ref vorbis_info vi, int zo);
        [DllImport(libraryName)]
        public static extern void vorbis_comment_init(ref vorbis_comment vc);
        [DllImport(libraryName, CharSet=CharSet.Ansi)]
        public static extern void vorbis_comment_add(ref vorbis_comment vc, string comment);
        [DllImport(libraryName, CharSet=CharSet.Ansi)]
        public static extern void vorbis_comment_add_tag(ref vorbis_comment vc, string tag, string contents);
        [DllImport(libraryName, CharSet=CharSet.Ansi)]
        public static extern string vorbis_comment_query(ref vorbis_comment vc, string tag, int count);
        [DllImport(libraryName, CharSet=CharSet.Ansi)]
        public static extern int vorbis_comment_query_count(ref vorbis_comment vc, string tag);
        [DllImport(libraryName)]
        public static extern void vorbis_comment_clear(ref vorbis_comment vc);

        [DllImport(libraryName)]
        public static extern int vorbis_block_init(ref vorbis_dsp_state v, ref vorbis_block vb);
        [DllImport(libraryName)]
        public static extern int vorbis_block_clear(ref vorbis_block vb);
        [DllImport(libraryName)]
        public static extern void vorbis_dsp_clear(ref vorbis_dsp_state v);
        [DllImport(libraryName)]
        public static extern double vorbis_granule_time(ref vorbis_dsp_state v, ogg_int64_t granulepos);
        #endregion
        
        #region Vorbis PRIMITIVES: analysis/DSP layer
        [DllImport(libraryName)]
        public static extern int vorbis_analysis_init(ref vorbis_dsp_state v, ref vorbis_info vi);
        [DllImport(libraryName)]
        public static extern int vorbis_commentheader_out(ref vorbis_comment vc, ref Ogg.ogg_packet op);
        [DllImport(libraryName)]
        public static extern int vorbis_analysis_headerout(ref vorbis_dsp_state v, ref vorbis_comment vc,
            ref Ogg.ogg_packet op, ref Ogg.ogg_packet op_comm, ref Ogg.ogg_packet op_code);
        [DllImport(libraryName)]
        public static extern IntPtr vorbis_analysis_buffer(ref vorbis_dsp_state v, int vals);
        [DllImport(libraryName)]
        public static extern int vorbis_analysis_wrote(ref vorbis_dsp_state v, int vals);
        [DllImport(libraryName)]
        public static extern int vorbis_analysis_blockout(ref vorbis_dsp_state v, ref vorbis_block vb);
        [DllImport(libraryName)]
        public static extern int vorbis_analysis(ref vorbis_block vb, ref Ogg.ogg_packet op);

        [DllImport(libraryName)]
        public static extern int vorbis_bitrate_addblock(ref vorbis_block vb);
        [DllImport(libraryName)]
        public static extern int vorbis_bitrate_flushpacket(ref vorbis_dsp_state vd, ref Ogg.ogg_packet op);
        #endregion

        #region Vorbis PRIMITIVES: synthesis layer
        [DllImport(libraryName)]
        public static extern int vorbis_synthesis_idheader(ref Ogg.ogg_packet op);
        [DllImport(libraryName)]
        public static extern int vorbis_synthesis_headerin(ref vorbis_info vi, ref vorbis_comment vc,
            ref Ogg.ogg_packet op);

        [DllImport(libraryName)]
        public static extern int vorbis_synthesis_init(ref vorbis_dsp_state v, ref vorbis_info vi);
        [DllImport(libraryName)]
        public static extern int vorbis_synthesis_restart(ref vorbis_dsp_state v);
        [DllImport(libraryName)]
        public static extern int vorbis_synthesis(ref vorbis_block vb, ref Ogg.ogg_packet op);
        [DllImport(libraryName)]
        public static extern int vorbis_synthesis_trackonly(ref vorbis_block vb, ref Ogg.ogg_packet op);
        [DllImport(libraryName)]
        public static extern int vorbis_synthesis_blockin(ref vorbis_dsp_state v, ref vorbis_block vb);
        [DllImport(libraryName)]
        public static extern int vorbis_synthesis_pcmout(ref vorbis_dsp_state v, IntPtr pcm);
        [DllImport(libraryName)]
        public static extern int vorbis_synthesis_lapout(ref vorbis_dsp_state v, IntPtr pcm);
        [DllImport(libraryName)]
        public static extern int vorbis_synthesis_read(ref vorbis_dsp_state v, int samples);
        [DllImport(libraryName)]
        public static extern int vorbis_packet_blocksize(ref vorbis_info vi, ref Ogg.ogg_packet op);

        [DllImport(libraryName)]
        public static extern int vorbis_synthesis_halfrate(ref vorbis_info v, int flag);
        [DllImport(libraryName)]
        public static extern int vorbis_synthesis_halfrate_p(ref vorbis_info v);
        #endregion
        #endregion
        #endregion
    }
}
