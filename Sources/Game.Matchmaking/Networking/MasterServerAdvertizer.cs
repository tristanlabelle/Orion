using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Orion.Engine.Networking.Http;

namespace Orion.Game.Matchmaking.Networking
{
    /// <summary>
    /// A match advertizer which register matches to a central server.
    /// </summary>
    public sealed class MasterServerAdvertizer : IMatchAdvertizer
    {
        #region Fields
        private const string nameField = "name";
        private const string openSlotsField = "places";
        private const string timeToLiveField = "ttl";
        private const string cancelMatch = "cancel";
        private const int defaultTimeToLive = 10;

        private readonly HttpRequest masterServerRequest;
        private readonly Uri masterServerUri;
        #endregion

        #region Constructors
        public MasterServerAdvertizer(string masterServerUri)
            : this(new Uri(masterServerUri))
        { }

        public MasterServerAdvertizer(Uri masterServerUri)
        {
            this.masterServerUri = masterServerUri;

            try
            {
                masterServerRequest = new HttpRequest(masterServerUri.DnsSafeHost);
            }
            catch (SocketException)
            {
                // The server is not reacheable, this object will act as a null object.
            }
        }
        #endregion

        #region Methods
        public void Advertize(string name, int openSlotsCount)
        {
            if (masterServerRequest == null) return;

            Dictionary<string, string> fields = new Dictionary<string, string>();
            fields[nameField] = name;
            fields[openSlotsField] = openSlotsCount.ToString();
            fields[timeToLiveField] = defaultTimeToLive.ToString();
            masterServerRequest.ExecuteAsync(HttpRequestMethod.Post, masterServerUri.AbsolutePath, fields);
        }

        public void Delist(string name)
        {
            if (masterServerRequest == null) return;

            Dictionary<string, string> fields = new Dictionary<string, string>();
            fields[nameField] = name;
            fields[cancelMatch] = "true";
            masterServerRequest.ExecuteAsync(HttpRequestMethod.Post, masterServerUri.AbsolutePath, fields);
        }
        #endregion
    }
}
