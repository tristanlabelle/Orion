using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Game.Matchmaking;
using Orion.Game.Matchmaking.Networking;
using System.Collections.ObjectModel;
using System.Net;
using System.IO;
using Orion.Engine.Networking;
using System.Text.RegularExpressions;

namespace Orion.Game.Main
{
    public class MasterServerQuerier : IMatchQuerier
    {
        #region Fields
        private static readonly Regex returnLineRegex = new Regex(@"^(\d+)\s+(\d+)\s(.+)$", RegexOptions.Compiled);

        private readonly Uri serverUri;
        private readonly TimeSpan timeBeforeReExploring;
        private readonly WebClient httpClient = new WebClient();
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
            httpClient.OpenReadCompleted += OnReadCompleted;
        }

        public MasterServerQuerier(string serverUri, TimeSpan timeBeforeReExploring)
            : this(new Uri(serverUri), timeBeforeReExploring)
        { }
        #endregion

        #region Properties
        public string Tag { get { return "WAN"; } }

        public ReadOnlyCollection<AdvertizedMatch> Matches
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
            DateTime now = DateTime.Now;
            bool updated = false;

            lock (pendingMatches)
            {
                if (pendingMatches.Count > 0)
                {
                    updated = matches.SequenceEqual(pendingMatches);
                    matches.Clear();
                    matches.AddRange(pendingMatches);
                }
            }

            if (now - lastPoll > timeBeforeReExploring)
            {
                lastPoll = now;
                httpClient.OpenReadAsync(serverUri);
            }
            return updated;
        }

        public void Dispose()
        {
            httpClient.Dispose();
        }

        private void OnReadCompleted(object sender, OpenReadCompletedEventArgs args)
        {
            List<AdvertizedMatch> matches = new List<AdvertizedMatch>();
            Stream stream = args.Result;
            using (StreamReader reader = new StreamReader(stream))
            lock (pendingMatches)
            {
                System.Text.RegularExpressions.Match match = returnLineRegex.Match(reader.ReadLine());
                int ipAddress = int.Parse(match.Groups[0].Value);
                int placesLeft = int.Parse(match.Groups[1].Value);
                IPv4EndPoint endPoint = new IPv4EndPoint((uint)ipAddress, 41223);
                AdvertizedMatch advertisement = new AdvertizedMatch(this, endPoint, match.Groups[2].Value, placesLeft);
                matches.Add(advertisement);
            }
            stream.Close(); // documentation says *we* have to close it
        }
        #endregion
    }
}
