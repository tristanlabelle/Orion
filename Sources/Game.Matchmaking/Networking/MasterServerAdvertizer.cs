using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Networking.Http;
using System.Diagnostics;

namespace Orion.Game.Matchmaking.Networking
{
    public class MasterServerAdvertizer : IMatchAdvertizer
    {
        #region Fields
        private const string nameField = "name";
        private const string openSlotsField = "places";
        private const string timeToLiveField = "ttl";
        private const int defaultTimeToLive = 10;

        private readonly HttpRequest masterServerRequest;
        private readonly Uri masterServerUri;
        private readonly Dictionary<string, string> fields = new Dictionary<string, string>();
        #endregion

        #region Constructors
        public MasterServerAdvertizer(string masterServerUri)
            : this(new Uri(masterServerUri))
        { }

        public MasterServerAdvertizer(Uri masterServerUri)
        {
            this.masterServerUri = masterServerUri;
            masterServerRequest = new HttpRequest(masterServerUri.DnsSafeHost);
        }
        #endregion

        #region Methods
        public void Advertize(string name, int openSlotsCount)
        {
            fields[nameField] = name;
            fields[openSlotsField] = openSlotsCount.ToString();
            fields[timeToLiveField] = defaultTimeToLive.ToString();
            masterServerRequest.ExecuteAsync(HttpRequestMethod.Post, masterServerUri.AbsolutePath, fields,
                r => Debug.WriteLine(r.Body));
        }
        #endregion
    }
}
