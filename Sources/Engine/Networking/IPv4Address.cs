using System;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Orion.Engine.Networking
{
    /// <summary>
    /// Represents an IP address in the IPV4 protocol.
    /// </summary>
    [Serializable]
    [ImmutableObject(true)]
    [StructLayout(LayoutKind.Sequential, Size = sizeof(uint))]
    public struct IPv4Address : IEquatable<IPv4Address>, IComparable<IPv4Address>
    {
        #region Instance
        #region Fields
        /// <summary>
        /// The 32-bit packed value of the address, in big endian format (MSB is first octet).
        /// </summary>
        private readonly uint value;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="IPv4Address"/> from its four octets.
        /// </summary>
        /// <param name="w">The first octet of the address.</param>
        /// <param name="x">The second octet of the address.</param>
        /// <param name="y">The third octet of the address.</param>
        /// <param name="z">The fourth octet of the address.</param>
        public IPv4Address(byte w, byte x, byte y, byte z)
        {
            this.value = ((uint)w << 24)
                | ((uint)x << 16)
                | ((uint)y << 8)
                | ((uint)z << 0);
        }

        /// <summary>
        /// Initializes a new <see cref="IPv4Address"/> from its value in packed big endian format.
        /// </summary>
        /// <param name="value">The value of the <see cref="IPv4Address"/>.</param>
        public IPv4Address(uint value)
        {
            this.value = value;
        }

        /// <summary>
        /// Initializes a new <see cref="IPv4Address"/> from its value
        /// in packed big endian format, as an <see cref="Int32"/>.
        /// </summary>
        /// <param name="value">The value of the <see cref="IPv4Address"/>.</param>
        public IPv4Address(long value)
        {
            Argument.EnsureWithin(value, uint.MinValue, uint.MaxValue, "value");
            this.value = (uint)value;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the value of this <see cref="IPv4Address"/> in packed big endian format (MSB is first octet).
        /// </summary>
        public uint Value
        {
            get { return value; }
        }

        /// <summary>
        /// Gets the value of this <see cref="IPv4Address"/> in packed big endian format as an <see cref="Int64"/>.
        /// </summary>
        public long LongValue
        {
            get { return value; }
        }

        #region Octets
        /// <summary>
        /// Gets the first octet of this <see cref="IPv4Address"/>.
        /// </summary>
        public byte W
        {
            get { return unchecked((byte)(value >> 24)); }
        }

        /// <summary>
        /// Gets the second octet of this <see cref="IPv4Address"/>.
        /// </summary>
        public byte X
        {
            get { return unchecked((byte)(value >> 16)); }
        }

        /// <summary>
        /// Gets the third octet of this <see cref="IPv4Address"/>.
        /// </summary>
        public byte Y
        {
            get { return unchecked((byte)(value >> 8)); }
        }

        /// <summary>
        /// Gets the fourth octet of this <see cref="IPv4Address"/>.
        /// </summary>
        public byte Z
        {
            get { return unchecked((byte)(value >> 0)); }
        }
        #endregion
        #endregion

        #region Methods
        /// <summary>
        /// Gets the octets of this <see cref="IPv4Address"/> as an array.
        /// </summary>
        /// <returns>An array containing the octets of this <see cref="IPv4Address"/>.</returns>
        public byte[] GetOctets()
        {
            return new[] { W, X, Y, Z };
        }

        /// <summary>
        /// Creates a new <see cref="IPAddress"/> from this <see cref="IPv4Address"/>.
        /// </summary>
        /// <returns>The resulting <see cref="IPAddress"/> corresponding to this.</returns>
        public IPAddress ToIPAddress()
        {
            return new IPAddress(((long)Z << 24) | ((long)Y << 16) | ((long)X << 8) | ((long)W << 0));
        }

        #region Object Model
        /// <summary>
        /// Tests for equality with another <see cref="IPv4Address"/>.
        /// </summary>
        /// <param name="other">A <see cref="IPv4Address"/> to be tested with.</param>
        /// <returns>True this <see name="IPv4Address"/> is equal to <paramref name="other"/>, false if not.</returns>
        public bool Equals(IPv4Address other)
        {
            return value == other.value;
        }

        /// <summary>
        /// Compares this <see cref="IPv4Address"/> with another.
        /// </summary>
        /// <param name="other">An <see cref="IPv4Address"/> to compare with.</param>
        /// <returns>
        /// An ordering value indicating the order of this <see cref="IPv4Address"/> relative to <paramref name="other"/>.
        /// </returns>
        public int CompareTo(IPv4Address other)
        {
            return value.CompareTo(other.value);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is IPv4Address)) return false;
            return Equals((IPv4Address)obj);
        }

        public override int GetHashCode()
        {
            return (int)value;
        }

        public override string ToString()
        {
            return "{0}.{1}.{2}.{3}".FormatInvariant(W, X, Y, Z);
        }
        #endregion
        #endregion
        #endregion

        #region Static
        #region Fields
        /// <summary>
        /// An IPV4 address that indicates that the server must listen for client
        /// activity on all network interfaces.
        /// </summary>
        public static readonly IPv4Address Any = new IPv4Address(0, 0, 0, 0);

        /// <summary>
        /// Provides the IPV4 loopback address.
        /// </summary>
        public static readonly IPv4Address Loopback = new IPv4Address(127, 0, 0, 1);

        /// <summary>
        /// Provides the IPV4 broadcast address.
        /// </summary>
        public static readonly IPv4Address Broadcast = new IPv4Address(255, 255, 255, 255);

        /// <summary>
        /// Provides an IP address that indicates that no network interface should be used.
        /// </summary>
        public static readonly IPv4Address None = Broadcast;

        private static readonly Regex parsingRegex = new Regex(
            @"\A(\d+)\.(\d+)\.(\d+)\.(\d+)\Z",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);
        #endregion

        #region Methods
        #region Factory Methods
        /// <summary>
        /// Creates a new <see cref="IPv4Address"/> from an <see cref="IPAddress"/>.
        /// </summary>
        /// <param name="address">An <see cref="IPAddress"/> to be converted.</param>
        /// <returns>The corresponding <see cref="IPv4Address"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="address"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="address"/> is not an IPV4 address.</exception>
        public static IPv4Address FromIPAddress(IPAddress address)
        {
            Argument.EnsureNotNull(address, "address");
            Argument.EnsureEqual(address.AddressFamily, AddressFamily.InterNetwork, "address.AddressFamily");

            byte[] addressBytes = address.GetAddressBytes();
            return new IPv4Address(addressBytes[0], addressBytes[1], addressBytes[2], addressBytes[3]);
        }

        /// <summary>
        /// Creates a new <see cref="IPv4Address"/> by parsing it from a string in format '#.#.#.#'.
        /// </summary>
        /// <param name="addressString">An address string to be parsed.</param>
        /// <returns>A corresponding <see cref="IPv4Address"/>.</returns>
        public static IPv4Address Parse(string addressString)
        {
            Argument.EnsureNotNull(addressString, "addressString");

            Match match = parsingRegex.Match(addressString);
            if (!match.Success) throw new FormatException("Invalid IPV4 address format, expected '#.#.#.#'.");

            byte x, y, z, w;
            if (!byte.TryParse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out x)
                || !byte.TryParse(match.Groups[2].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out y)
                || !byte.TryParse(match.Groups[3].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out z)
                || !byte.TryParse(match.Groups[4].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out w))
            {
                throw new FormatException("Invalid IPV4 address octet. Expected a value between 0 and 255.");
            }

            return new IPv4Address(x, y, z, w);
        }

        /// <summary>
        /// Attempts to create a new <see cref="IPv4Address"/> by parsing it from a string in format '#.#.#.#'.
        /// </summary>
        /// <param name="addressString">An address string to be parsed.</param>
        /// <returns>The corresponding <see cref="IPv4Address"/>, or <c>null</c> if the string failed to be parsed.</returns>
        public static IPv4Address? TryParse(string addressString)
        {
            try { return Parse(addressString); }
            catch (FormatException) { return null; }
        }

        public static bool TryParse(string addressString, out IPv4Address address)
        {
            try
            {
                address = Parse(addressString);
                return true;
            }
            catch (FormatException)
            {
                address = default(IPv4Address);
                return false;
            }
        }
        #endregion

        #region Equality/Comparison
        /// <summary>
        /// Tests two <see cref="IPv4Address"/> for equality.
        /// </summary>
        /// <param name="first">The first <see cref="IPv4Address"/>.</param>
        /// <param name="second">The second <see cref="IPv4Address"/>.</param>
        /// <returns>True if <paramref name="first"/> and <paramref name="second"/> are equal, false if not.</returns>
        public static bool Equals(IPv4Address first, IPv4Address second)
        {
            return first.Equals(second);
        }

        /// <summary>
        /// Compares two <see cref="IPv4Address"/>es.
        /// </summary>
        /// <param name="first">The first <see cref="IPv4Address"/>.</param>
        /// <param name="second">The second <see cref="IPv4Address"/>.</param>
        /// <returns>
        /// An ordering value indicating the order of <paramref name="first"/> relative to <paramref name="second"/>.
        /// </returns>
        public static int Compare(IPv4Address first, IPv4Address second)
        {
            return first.CompareTo(second);
        }
        #endregion
        #endregion

        #region Operators
        #region Equality
        /// <summary>
        /// Tests two <see cref="IPv4Address"/> for equality.
        /// </summary>
        /// <param name="lhs">The left hand side operand instance.</param>
        /// <param name="rhs">The right hand side operand instance.</param>
        /// <returns>
        /// True if <paramref name="lhs"/> and <paramref name="rhs"/> are equal, false if they are different.
        /// </returns>
        public static bool operator ==(IPv4Address lhs, IPv4Address rhs)
        {
            return Equals(lhs, rhs);
        }

        /// <summary>
        /// Tests two <see cref="IPv4Address"/> for inequality.
        /// </summary>
        /// <param name="lhs">The left hand side operand instance.</param>
        /// <param name="rhs">The right hand side operand instance.</param>
        /// <returns>
        /// True if <paramref name="lhs"/> and <paramref name="rhs"/> are different, false if they are equal.
        /// </returns>
        public static bool operator !=(IPv4Address lhs, IPv4Address rhs)
        {
            return !Equals(lhs, rhs);
        }
        #endregion

        #region Comparison
        public static bool operator <(IPv4Address lhs, IPv4Address rhs)
        {
            return Compare(lhs, rhs) < 0;
        }

        public static bool operator <=(IPv4Address lhs, IPv4Address rhs)
        {
            return Compare(lhs, rhs) <= 0;
        }

        public static bool operator >(IPv4Address lhs, IPv4Address rhs)
        {
            return Compare(lhs, rhs) > 0;
        }

        public static bool operator >=(IPv4Address lhs, IPv4Address rhs)
        {
            return Compare(lhs, rhs) >= 0;
        }
        #endregion

        #region Cast
        public static implicit operator IPAddress(IPv4Address address)
        {
            return address.ToIPAddress();
        }

        public static explicit operator IPv4Address(IPAddress address)
        {
            return FromIPAddress(address);
        }
        #endregion
        #endregion
        #endregion
    }
}
