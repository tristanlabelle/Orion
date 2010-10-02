using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Orion.Engine.Audio
{
    /// <summary>
    /// Describes the way sample bits encode their value.
    /// </summary>
    [Serializable]
    public enum SoundSampleType
    {
        /// <summary>
        /// Specifies that the values are unsigned values from 0 to 2^n-1.
        /// </summary>
        Unsigned,

        /// <summary>
        /// Specifies that the values are signed values in two's complement format, from -2^(n-1) to 2^(n-1)-1.
        /// </summary>
        Signed,

        /// <summary>
        /// Specifies that the values are IEEE floating-point numbers.
        /// </summary>
        FloatingPoint
    }

    /// <summary>
    /// Describes how individual sound samples are represented in memory.
    /// </summary>
    [Serializable]
    [ImmutableObject(true)]
    public struct SoundSampleFormat
    {
        #region Fields
        public static readonly SoundSampleFormat UnsignedByte = new SoundSampleFormat(SoundSampleType.Unsigned, 1);
        public static readonly SoundSampleFormat SignedByte = new SoundSampleFormat(SoundSampleType.Signed, 1);
        public static readonly SoundSampleFormat UnsignedShort = new SoundSampleFormat(SoundSampleType.Unsigned, 2);
        public static readonly SoundSampleFormat SignedShort = new SoundSampleFormat(SoundSampleType.Signed, 2);
        public static readonly SoundSampleFormat UnsignedInt = new SoundSampleFormat(SoundSampleType.Unsigned, 4);
        public static readonly SoundSampleFormat SignedInt = new SoundSampleFormat(SoundSampleType.Signed, 4);

        private readonly SoundSampleType type;
        private readonly int sizeInBytesMinusOne;
        #endregion

        #region Constructors
        public SoundSampleFormat(SoundSampleType type, int sizeInBytes)
        {
            Argument.EnsureDefined(type, "type");
            Argument.EnsureStrictlyPositive(sizeInBytes, "sizeInBytes");

            this.type = type;
            this.sizeInBytesMinusOne = sizeInBytes - 1;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the type of the sound samples.
        /// </summary>
        public SoundSampleType Type
        {
            get { return type; }
        }

        /// <summary>
        /// Gets the size, in bytes, of a sample with this type.
        /// </summary>
        public int SizeInBytes
        {
            get { return sizeInBytesMinusOne + 1; }
        }

        /// <summary>
        /// Gets the size, in bits, of a sample with this type.
        /// </summary>
        public int SizeInBits
        {
            get { return SizeInBytes * 8; }
        }
        #endregion
    }
}
