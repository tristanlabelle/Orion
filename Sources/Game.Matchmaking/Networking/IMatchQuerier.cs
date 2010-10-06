using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Game.Matchmaking.Networking;
using System.Collections.ObjectModel;

namespace Orion.Game.Matchmaking.Networking
{
    public interface IMatchQuerier : IDisposable
    {
        #region Properties
        /// <summary>
        /// Gets the read-only list of matches offered by this IMatchFinder.
        /// </summary>
        ReadOnlyCollection<AdvertizedMatch> Matches { get; }

        /// <summary>
        /// Gets or sets the activity state of this IMatchFinder.
        /// </summary>
        /// <remarks>
        /// A disabled IMatchFinder does nothing to find matches, and throws an
        /// InvalidOperationException if its Update method is called.
        /// </remarks>
        bool IsEnabled { get; set; }

        /// <summary>
        /// Gets the tag of this IMatchFinder.
        /// </summary>
        /// <remarks>
        /// The tag is indented to differentiate the source of matches. It can be displayed
        /// with the games to mark if the game is, say, from a remote server, or from the local
        /// network.
        /// </remarks>
        string Tag { get; }
        #endregion

        #region Methods
        /// <summary>
        /// Updates this IMatchFinder so it refreshes its internal list of matches, or at
        /// least tries to do so.
        /// </summary>
        /// <returns></returns>
        bool Update();
        #endregion
    }
}
