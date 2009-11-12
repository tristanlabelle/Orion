using System;
using System.ComponentModel;
using System.Net;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Orion.Networking
{
    /// <summary>
    /// Represents an end point of an IPV4 connection.
    /// </summary>
    [Serializable]
    [ImmutableObject(true)]
    [StructLayout(LayoutKind.Sequential, Size = sizeof(uint) + sizeof(ushort))]
    public struct Ipv4EndPoint : IEquatable<Ipv4EndPoint>, IComparable<Ipv4EndPoint>
    {
        #region Instance
        #region Fields
        private readonly Ipv4Address address;
        private readonly ushort port;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="Ipv4EndPoint"/> from an <see cref="Ipv4Address"/> and a port number.
        /// </summary>
        /// <param name="address">The <see cref="Ipv4Address"/> of the end point.</param>
        /// <param name="port">The port of the end point.</param>
        public Ipv4EndPoint(Ipv4Address address, ushort port)
        {
            this.address = address;
            this.port = port;
        }

        /// <summary>
        /// Initializes a new <see cref="Ipv4EndPoint"/> from an <see cref="Ipv4Address"/> and a port number.
        /// </summary>
        /// <param name="address">The <see cref="Ipv4Address"/> of the end point.</param>
        /// <param name="port">The port of the end point.</param>
        public Ipv4EndPoint(Ipv4Address address, int port)
        {
            Argument.EnsureWithin(port, ushort.MinValue, ushort.MaxValue, "port");

            this.address = address;
            this.port = (ushort)port;
        }

        /// <summary>
        /// Initializes a new <see cref="Ipv4EndPoint"/> from the octets of its address and a port number.
        /// </summary>
        /// <param name="x">The first octet of the address.</param>
        /// <param name="y">The second octet of the address.</param>
        /// <param name="z">The third octet of the address.</param>
        /// <param name="w">The fourth octet of the address.</param>
        /// <param name="port">The port of the end point.</param>
        public Ipv4EndPoint(byte x, byte y, byte z, byte w, ushort port)
            : this(new Ipv4Address(x, y, z, w), port) { }

        /// <summary>
        /// Initializes a new <see cref="Ipv4EndPoint"/> from the octets of its address and a port number.
        /// </summary>
        /// <param name="x">The first octet of the address.</param>
        /// <param name="y">The second octet of the address.</param>
        /// <param name="z">The third octet of the address.</param>
        /// <param name="w">The fourth octet of the address.</param>
        /// <param name="port">The port of the end point.</param>
        public Ipv4EndPoint(byte x, byte y, byte z, byte w, int port)
            : this(new Ipv4Address(x, y, z, w), port) { }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the address of this <see cref="Ipv4EndPoint"/>.
        /// </summary>
        public Ipv4Address Address
        {
            get { return address; }
        }

        /// <summary>
        /// Gets the port of this <see cref="Ipv4EndPoint"/>.
        /// </summary>
        public ushort Port
        {
            get { return port; }
        }

        /// <summary>
        /// Gets the port of this <see cref="Ipv4EndPoint"/> as an <see cref="Int32"/>.
        /// </summary>
        public int IntPort
        {
            get { return port; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets an <see cref="IPEndPoint"/> corresponding to this <see cref="Ipv4EndPoint"/>.
        /// </summary>
        /// <returns>A corresponding <see cref="IPEndPoint"/>.</returns>
        public IPEndPoint ToIPEndPoint()
        {
            return new IPEndPoint(address, port);
        }

        #region Object Model
        /// <summary>
        /// Tests for equality with another <see cref="Ipv4EndPoint"/>.
        /// </summary>
        /// <param name="other">A <see cref="Ipv4EndPoint"/> to be tested with.</param>
        /// <returns>True this <see name="Ipv4EndPoint"/> is equal to <paramref name="other"/>, false if not.</returns>
        public bool Equals(Ipv4EndPoint other)
        {
            return address == other.address && port == other.port;
        }

        /// <summary>
        /// Compares this <see cref="Ipv4EndPoint"/> with another.
        /// </summary>
        /// <param name="other">An <see cref="Ipv4EndPoint"/> to compare with.</param>
        /// <returns>
        /// An ordering value indicating the order of this <see cref="Ipv4EndPoint"/> relative to <paramref name="other"/>.
        /// </returns>
        public int CompareTo(Ipv4EndPoint other)
        {
            int value = address.CompareTo(other.address);
            if (value == 0) value = port.CompareTo(other.port);
            return value;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Ipv4EndPoint)) return false;
            return Equals((Ipv4EndPoint)obj);
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
        public static readonly Ipv4EndPoint Any = new Ipv4EndPoint(Ipv4Address.Any, 0);
        #endregion

        #region Methods
        #region Factory Methods
        /// <summary>
        /// Creates a new <see cref="Ipv4EndPoint"/> from an <see cref="IPEndPoint"/>.
        /// </summary>
        /// <param name="endPoint">The <see cref="IPEndPoint"/> to be converted.</param>
        /// <returns>The resulting <see cref="Ipv4EndPoint"/>.</returns>
        public static Ipv4EndPoint FromIPEndPoint(IPEndPoint endPoint)
        {
            Argument.EnsureNotNull(endPoint, "endPoint");
            return new Ipv4EndPoint(Ipv4Address.FromIPAddress(endPoint.Address), endPoint.Port);
        }

        /// <summary>
        /// Parses an <see cref="Ipv4EndPoint"/> from a string representation of the form <c>#.#.#.#:#</c>.
        /// </summary>
        /// <param name="str">The string to be parsed.</param>
        /// <returns>The <see cref="Ipv4EndPoint"/> that was parsed.</returns>
        public static Ipv4EndPoint Parse(string str)
        {
            Argument.EnsureNotNull(str, "str");

            int colonIndex = str.IndexOf(':');
            if (colonIndex == -1) throw new FormatException("Invalid IPV4 EndPoint format, expected a colon");

            string addressString = str.Substring(0, colonIndex);
            Ipv4Address address = Ipv4Address.Parse(addressString);

            string portString = str.Substring(colonIndex + 1);
            ushort port;
            if (!ushort.TryParse(portString, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out port))
                throw new FormatException("Invalid IPV4 EndPoint format, a port number between 0 and 65535.");

            return new Ipv4EndPoint(address, port);
        }
        #endregion

        #region Equality/Comparison
        /// <summary>
        /// Tests two <see cref="Ipv4EndPoint"/> for equality.
        /// </summary>
        /// <param name="first">The first <see cref="Ipv4EndPoint"/>.</param>
        /// <param name="second">The second <see cref="Ipv4EndPoint"/>.</param>
        /// <returns>True if <paramref name="first"/> and <paramref name="second"/> are equal, false if not.</returns>
        public static bool Equals(Ipv4EndPoint first, Ipv4EndPoint second)
        {
            return first.Equals(second);
        }

        /// <summary>
        /// Compares two <see cref="Ipv4EndPoint"/>s.
        /// </summary>
        /// <param name="first">The first <see cref="Ipv4EndPoint"/>.</param>
        /// <param name="second">The second <see cref="Ipv4EndPoint"/>.</param>
        /// <returns>
        /// An ordering value indicating the order of <paramref name="first"/> relative to <paramref name="second"/>.
        /// </returns>
        public static int Compare(Ipv4EndPoint first, Ipv4EndPoint second)
        {
            return first.CompareTo(second);
        }
        #endregion
        #endregion

        #region Operators
        #region Equality
        /// <summary>
        /// Tests two <see cref="Ipv4EndPoint"/> for equality.
        /// </summary>
        /// <param name="lhs">The left hand side operand instance.</param>
        /// <param name="rhs">The right hand side operand instance.</param>
        /// <returns>
        /// True if <paramref name="lhs"/> and <paramref name="rhs"/> are equal, false if they are different.
        /// </returns>
        public static bool operator ==(Ipv4EndPoint lhs, Ipv4EndPoint rhs)
        {
            return Equals(lhs, rhs);
        }

        /// <summary>
        /// Tests two <see cref="Ipv4EndPoint"/> for inequality.
        /// </summary>
        /// <param name="lhs">The left hand side operand instance.</param>
        /// <param name="rhs">The right hand side operand instance.</param>
        /// <returns>
        /// True if <paramref name="lhs"/> and <paramref name="rhs"/> are different, false if they are equal.
        /// </returns>
        public static bool operator !=(Ipv4EndPoint lhs, Ipv4EndPoint rhs)
        {
            return !Equals(lhs, rhs);
        }
        #endregion

        #region Comparison
        public static bool operator <(Ipv4EndPoint lhs, Ipv4EndPoint rhs)
        {
            return Compare(lhs, rhs) < 0;
        }

        public static bool operator <=(Ipv4EndPoint lhs, Ipv4EndPoint rhs)
        {
            return Compare(lhs, rhs) <= 0;
        }

        public static bool operator >(Ipv4EndPoint lhs, Ipv4EndPoint rhs)
        {
            return Compare(lhs, rhs) > 0;
        }

        public static bool operator >=(Ipv4EndPoint lhs, Ipv4EndPoint rhs)
        {
            return Compare(lhs, rhs) >= 0;
        }
        #endregion

        #region Cast
        public static implicit operator IPEndPoint(Ipv4EndPoint endPoint)
        {
            return endPoint.ToIPEndPoint();
        }

        public static explicit operator Ipv4EndPoint(IPEndPoint endPoint)
        {
            return FromIPEndPoint(endPoint);
        }
        #endregion
        #endregion
        #endregion
    }
}
