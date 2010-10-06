using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Game.Matchmaking;
using Orion.Game.Matchmaking.Networking;
using System.Collections.ObjectModel;
using System.Net;
using System.IO;

namespace Orion.Game.Main
{
    public class MasterServerQuerier : IMatchQuerier
    {
        #region Fields
        private readonly Uri serverUri;
        private readonly TimeSpan timeBeforeReExploring;
        private DateTime lastPoll;
        private bool isEnabled;

        private readonly List<AdvertizedMatch> matches = new List<AdvertizedMatch>();
        private readonly ReadOnlyCollection<AdvertizedMatch> readOnlyMatches;
        #endregion

        #region Constructors
        public MasterServerQuerier(Uri serverUri, TimeSpan timeBeforeReExploring)
        {
            this.timeBeforeReExploring = timeBeforeReExploring;
            this.serverUri = serverUri;
            this.readOnlyMatches = matches.AsReadOnly();
        }

        public MasterServerQuerier(string serverUri, TimeSpan timeBeforeReExploring)
            : this(new Uri(serverUri), timeBeforeReExploring)
        { }
        #endregion

        #region Properties
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

            if (now - lastPoll > timeBeforeReExploring)
            {
            }
            return false;
        }
        #endregion
    }
}
