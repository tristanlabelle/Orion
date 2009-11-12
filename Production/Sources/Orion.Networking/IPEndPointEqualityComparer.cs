using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.ComponentModel;

namespace Orion.Networking
{
    /// <summary>
    /// Provides an equality implementation for <see cref="IPEndPoint"/>s.
    /// </summary>
    [Serializable]
    [ImmutableObject(true)]
    public sealed class IPEndPointEqualityComparer : IEqualityComparer<IPEndPoint>
    {
        #region Instance
        #region Methods
        public bool Equals(IPEndPoint x, IPEndPoint y)
        {
            if (object.ReferenceEquals(x, y)) return true;
            if (object.ReferenceEquals(x, null) || object.ReferenceEquals(y, null))
                return false;

            return x.Address.Equals(y.Address) && x.Port == y.Port;
        }

        public int GetHashCode(IPEndPoint obj)
        {
            if (object.ReferenceEquals(obj, null)) return int.MinValue;
            return obj.Port;
        }
        #endregion
        #endregion

        #region Static
        #region Fields
        public static readonly IPEndPointEqualityComparer Instance = new IPEndPointEqualityComparer();
        #endregion
        #endregion
    }
}
