using System;
using System.Net;
using System.Windows.Forms;

namespace Orion.UserInterface
{
    /// <summary>
    /// Describes the way a user decides to start a match.
    /// </summary>
    public enum MatchStartType
    {
        /// <summary>
        /// Specifies that no start type has been selected.
        /// </summary>
        None,

        /// <summary>
        /// Specifies that a solo match should be started.
        /// </summary>
        Solo,

        /// <summary>
        /// Specifies that a multiplayer match should be hosted.
        /// </summary>
        Host,

        /// <summary>
        /// Specifies that a multiplayer match should be joined.
        /// </summary>
        Join
    }
}
