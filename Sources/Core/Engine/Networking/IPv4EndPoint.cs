using System;
using System.ComponentModel;
using System.Net;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Orion.Engine.Networking
{
    /// <summary>
    /// Represents an end point of an IPV4 connection.
    /// </summary>
    [Serializable]
    [ImmutableObject(true)]
    [StructLayout(LayoutKind.Sequential, Size = sizeof(uint) + sizeof(ushort))]
    public struct IPv4EndPoint : IEquatable<IPv4EndPoint>, IComparable<IPv4EndPoint>
    {
        #region Instance
        #region Fields
        private readonly IPv4Address address;
        private readonly ushort port;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="IPv4EndPoint"/> from an <see cref="IPv4Address"/> and a port number.
        /// </summary>
        /// <param name="address">The <see cref="IPv4Address"/> of the end point.</param>
        /// <param name="port">The port of the end point.</param>
        public IPv4EndPoint(IPv4Address address, ushort port)
        {
            this.address = address;
            this.port = port;
        }

        /// <summary>
        /// Initializes a new <see cref="IPv4EndPoint"/> from an <see cref="IPv4Address"/> and a port number.
        /// </summary>
        /// <param name="address">The <see cref="IPv4Address"/> of the end point.</param>
        /// <param name="port">The port of the end point.</param>
        public IPv4EndPoint(IPv4Address address, int port)
        {
            Argument.EnsureWithin(port, ushort.MinValue, ushort.MaxValue, "port");

            this.address = address;
            this.port = (ushort)port;
        }

        /// <summary>
        /// Initializes a new <see cref="IPv4EndPoint"/> from the octets of its address and a port number.
        /// </summary>
        /// <param name="w">The first octet of the address.</param>
        /// <param name="x">The second octet of the address.</param>
        /// <param name="y">The third octet of the address.</param>
        /// <param name="z">The fourth octet of the address.</param>
        /// <param name="port">The port of the end point.</param>
        public IPv4EndPoint(byte w, byte x, byte y, byte z, ushort port)
            : this(new IPv4Address(x, y, z, w), port) { }

        /// <summary>
        /// Initializes a new <see cref="IPv4EndPoint"/> from the octets of its address and a port number.
        /// </summary>
        /// <param name="w">The first octet of the address.</param>
        /// <param name="x">The second octet of the address.</param>
        /// <param name="y">The third octet of the address.</param>
        /// <param name="z">The fourth octet of the address.</param>
        /// <param name="port">The port of the end point.</param>
        public IPv4EndPoint(byte w, byte x, byte y, byte z, int port)
            : this(new IPv4Address(x, y, z, w), port) { }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the address of this <see cref="IPv4EndPoint"/>.
        /// </summary>
        public IPv4Address Address
        {
            get { return address; }
        }

        /// <summary>
        /// Gets the port of this <see cref="IPv4EndPoint"/>.
        /// </summary>
        public ushort Port
        {
            get { return port; }
        }

        /// <summary>
        /// Gets the port of this <see cref="IPv4EndPoint"/> as an <see cref="Int32"/>.
        /// </summary>
        public int IntPort
        {
            get { return port; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets an <see cref="IPEndPoint"/> corresponding to this <see cref="IPv4EndPoint"/>.
        /// </summary>
        /// <returns>A corresponding <see cref="IPEndPoint"/>.</returns>
        public IPEndPoint ToIPEndPoint()
        {
            return new IPEndPoint(address, port);
        }

        /// <summary>
        /// Copies the bytes that form the binary representation of this <see cref="IPv4EndPoint"/> to a byte buffer.
        /// </summary>
        /// <param name="buffer">The buffer to which the bytes are to be written.</param>
        /// <param name="startIndex">The index where to start copying into the buffer.</param>
        /// <remarks>
        /// The serialization puts both the address octets and the port bytes in big endian byte ordering.
        /// </remarks>
        public void CopyBytes(byte[] buffer, int startIndex)
        {
            ValidateBufferSize(buffer, startIndex);

            buffer[startIndex] = address.W;
            buffer[startIndex + 1] = address.X;
            buffer[startIndex + 2] = address.Y;
            buffer[startIndex + 3] = address.Z;
            buffer[startIndex + 4] = (byte)(port >> 8);
            buffer[startIndex + 5] = (byte)port;
        }

        #region Object Model
        /// <summary>
        /// Tests for equality with another <see cref="IPv4EndPoint"/>.
        /// </summary>
        /// <param name="other">A <see cref="IPv4EndPoint"/> to be tested with.</param>
        /// <returns>True this <see name="IPv4EndPoint"/> is equal to <paramref name="other"/>, false if not.</returns>
        public bool Equals(IPv4EndPoint other)
        {
            return address == other.address && port == other.port;
        }

        /// <summary>
        /// Compares this <see cref="IPv4EndPoint"/> with another.
        /// </summary>
        /// <param name="other">An <see cref="IPv4EndPoint"/> to compare with.</param>
        /// <returns>
        /// An ordering value indicating the order of this <see cref="IPv4EndPoint"/> relative to <paramref name="other"/>.
        /// </returns>
        public int CompareTo(IPv4EndPoint other)
        {
            int value = address.CompareTo(other.address);
            if (value == 0) value = port.CompareTo(other.port);
            return value;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is IPv4EndPoint)) return false;
            return Equals((IPv4EndPoint)obj);
        }

        public override int GetHashCode()
        {
            return address.GetHashCode() ^ port.GetHashCode();
        }

        public override string ToString()
        {
            return "{0}:{1}".FormatInvariant(address, port);
        }
        #endregion
        #endregion
        #endregion

        #region Static
        #region Fields
        /// <summary>
        /// Represents any end point. Used when receiving from an unknown source.
        /// </summary>
        public static readonly IPv4EndPoint Any = new IPv4EndPoint(IPv4Address.Any, 0);
        #endregion

        #region Methods
        #region Factory Methods
        /// <summary>
        /// Creates a new <see cref="IPv4EndPoint"/> from an <see cref="IPEndPoint"/>.
        /// </summary>
        /// <param name="endPoint">The <see cref="IPEndPoint"/> to be converted.</param>
        /// <returns>The resulting <see cref="IPv4EndPoint"/>.</returns>
        public static IPv4EndPoint FromIPEndPoint(IPEndPoint endPoint)
        {
            Argument.EnsureNotNull(endPoint, "endPoint");
            return new IPv4EndPoint(IPv4Address.FromIPAddress(endPoint.Address), endPoint.Port);
        }

        /// <summary>
        /// Parses an <see cref="IPv4EndPoint"/> from a string representation of the form <c>#.#.#.#:#</c>.
        /// </summary>
        /// <param name="str">The string to be parsed.</param>
        /// <returns>The <see cref="IPv4EndPoint"/> that was parsed.</returns>
        public static IPv4EndPoint Parse(string str)
        {
            Argument.EnsureNotNull(str, "str");

            int colonIndex = str.IndexOf(':');
            if (colonIndex == -1) throw new FormatException("Invalid IPV4 EndPoint format, expected a colon");

            string addressString = str.Substring(0, colonIndex);
            IPv4Address address = IPv4Address.Parse(addressString);

            string portString = str.Substring(colonIndex + 1);
            ushort port;
            if (!ushort.TryParse(portString, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out port))
                throw new FormatException("Invalid IPV4 EndPoint format, a port number between 0 and 65535.");

            return new IPv4EndPoint(address, port);
        }

        /// <summary>
        /// Creates a new <see cref="IPv4EndPoint"/> from its binary representation serialized in an byte buffer.
        /// </summary>
        /// <param name="buffer">The buffer from which to read.</param>
        /// <param name="startIndex">The index at which the reading should begin.</param>
        /// <returns>The <see cref="IPv4EndPoint"/> that was deserialized.</returns>
        /// <remarks>
        /// The serialization expects both the address octets and the port bytes in big endian byte ordering.
        /// </remarks>
        public static IPv4EndPoint FromBytes(byte[] buffer, int startIndex)
        {
            ValidateBufferSize(buffer, startIndex);

            IPv4Address address = new IPv4Address(
                buffer[startIndex], buffer[startIndex + 1],
                buffer[startIndex + 2], buffer[startIndex + 3]);
            ushort port = (ushort)(((int)buffer[startIndex + 4] << 8) | (int)buffer[startIndex + 5]);
            return new IPv4EndPoint(address, port);
        }

        private static void ValidateBufferSize(byte[] buffer, int startIndex)
        {
            Argument.EnsureNotNull(buffer, "buffer");
            Argument.EnsureStrictlyPositive(startIndex, "startIndex");
            if (startIndex + 6 > buffer.Length) throw new ArgumentOutOfRangeException("startIndex");
        }
        #endregion

        #region Equality/Comparison
        /// <summary>
        /// Tests two <see cref="IPv4EndPoint"/> for equality.
        /// </summary>
        /// <param name="first">The first <see cref="IPv4EndPoint"/>.</param>
        /// <param name="second">The second <see cref="IPv4EndPoint"/>.</param>
        /// <returns>True if <paramref name="first"/> and <paramref name="second"/> are equal, false if not.</returns>
        public static bool Equals(IPv4EndPoint first, IPv4EndPoint second)
        {
            return first.Equals(second);
        }

        /// <summary>
        /// Compares two <see cref="IPv4EndPoint"/>s.
        /// </summary>
        /// <param name="first">The first <see cref="IPv4EndPoint"/>.</param>
        /// <param name="second">The second <see cref="IPv4EndPoint"/>.</param>
        /// <returns>
        /// An ordering value indicating the order of <paramref name="first"/> relative to <paramref name="second"/>.
        /// </returns>
        public static int Compare(IPv4EndPoint first, IPv4EndPoint second)
        {
            return first.CompareTo(second);
        }
        #endregion
        #endregion

        #region Operators
        #region Equality
        /// <summary>
        /// Tests two <see cref="IPv4EndPoint"/> for equality.
        /// </summary>
        /// <param name="lhs">The left hand side operand instance.</param>
        /// <param name="rhs">The right hand side operand instance.</param>
        /// <returns>
        /// True if <paramref name="lhs"/> and <paramref name="rhs"/> are equal, false if they are different.
        /// </returns>
        public static bool operator ==(IPv4EndPoint lhs, IPv4EndPoint rhs)
        {
            return Equals(lhs, rhs);
        }

        /// <summary>
        /// Tests two <see cref="IPv4EndPoint"/> for inequality.
        /// </summary>
        /// <param name="lhs">The left hand side operand instance.</param>
        /// <param name="rhs">The right hand side operand instance.</param>
        /// <returns>
        /// True if <paramref name="lhs"/> and <paramref name="rhs"/> are different, false if they are equal.
        /// </returns>
        public static bool operator !=(IPv4EndPoint lhs, IPv4EndPoint rhs)
        {
            return !Equals(lhs, rhs);
        }
        #endregion

        #region Comparison
        public static bool operator <(IPv4EndPoint lhs, IPv4EndPoint rhs)
        {
            return Compare(lhs, rhs) < 0;
        }

        public static bool operator <=(IPv4EndPoint lhs, IPv4EndPoint rhs)
        {
            return Compare(lhs, rhs) <= 0;
        }

        public static bool operator >(IPv4EndPoint lhs, IPv4EndPoint rhs)
        {
            return Compare(lhs, rhs) > 0;
        }

        public static bool operator >=(IPv4EndPoint lhs, IPv4EndPoint rhs)
        {
            return Compare(lhs, rhs) >= 0;
        }
        #endregion

        #region Cast
        public static implicit operator IPEndPoint(IPv4EndPoint endPoint)
        {
            return endPoint.ToIPEndPoint();
        }

        public static explicit operator IPv4EndPoint(IPEndPoint endPoint)
        {
            return FromIPEndPoint(endPoint);
        }
        #endregion
        #endregion
        #endregion
    }
}
