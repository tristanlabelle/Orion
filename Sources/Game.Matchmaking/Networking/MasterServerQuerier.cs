using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using Orion.Engine.Networking;
using Orion.Engine.Networking.Http;
using Orion.Game.Matchmaking;
using Orion.Game.Matchmaking.Networking;

namespace Orion.Game.Matchmaking.Networking
{
    /// <summary>
    /// Provides matches by checking those advertized on a master server.
    /// </summary>
    public sealed class MasterServerQuerier : IMatchQuerier
    {
        #region Fields
        private static readonly Regex returnLineRegex = new Regex(@"^(\d+)\s+(\d+)\s+(.+)$", RegexOptions.Compiled);

        private readonly Uri serverUri;
        private readonly TimeSpan timeBeforeReExploring;
        private readonly HttpRequest httpClient;
        private DateTime lastPoll = DateTime.MinValue;
        private bool isEnabled;

        private readonly List<AdvertizedMatch> matches = new List<AdvertizedMatch>();
        private readonly List<AdvertizedMatch> pendingMatches = new List<AdvertizedMatch>();
        private readonly ReadOnlyCollection<AdvertizedMatch> readOnlyMatches;
        #endregion

        #region Constructors
        public MasterServerQuerier(Uri serverUri, TimeSpan timeBeforeReExploring)
        {
            this.timeBeforeReExploring = timeBeforeReExploring;
            this.serverUri = serverUri;
            this.readOnlyMatches = matches.AsReadOnly();

            try
            {
                httpClient = new HttpRequest(serverUri.DnsSafeHost);
            }
            catch (SocketException)
            {
                // The master server was not found, this object will act as a null object.
            }
        }

        public MasterServerQuerier(string serverUri, TimeSpan timeBeforeReExploring)
            : this(new Uri(serverUri), timeBeforeReExploring)
        { }
        #endregion

        #region Properties
        public string Tag { get { return "WAN"; } }

        public ICollection<AdvertizedMatch> Matches
        {
            get { return readOnlyMatches; }
        }

        public Uri ServerUri
        {
            get { return serverUri; }
        }

        public bool IsEnabled
        {
            get { return isEnabled; }
            set { isEnabled = value; }
        }
        #endregion

        #region Methods
        public bool Update()
        {
            if (httpClient == null || !isEnabled) return false;

            DateTime now = DateTime.Now;
            bool updated = false;

            lock (pendingMatches)
            {
                updated = !matches.SequenceEqual(pendingMatches);
                matches.Clear();
                matches.AddRange(pendingMatches);
            }

            if (now - lastPoll > timeBeforeReExploring)
            {
                lastPoll = now;
                httpClient.ExecuteAsync(HttpRequestMethod.Get, serverUri.AbsolutePath, OnReadCompleted);
            }

            return updated;
        }

        public void Dispose() { }

        private void OnReadCompleted(HttpResponse response)
        {
            List<AdvertizedMatch> matches = new List<AdvertizedMatch>();
            System.Text.RegularExpressions.Match match = returnLineRegex.Match(response.Body);
            lock (pendingMatches)
            {
                pendingMatches.Clear();
                while (match.Success)
                {
                    uint ipAddress = uint.Parse(match.Groups[1].Value);
                    int placesLeft = int.Parse(match.Groups[2].Value);
#warning Port number was hard-coded here
                    IPv4EndPoint endPoint = new IPv4EndPoint(ipAddress, 41223);
                    AdvertizedMatch advertisement = new AdvertizedMatch(this, endPoint, match.Groups[3].Value, placesLeft);
                    pendingMatches.Add(advertisement);
                    match = match.NextMatch();
                }
            }
        }
        #endregion
    }
}
