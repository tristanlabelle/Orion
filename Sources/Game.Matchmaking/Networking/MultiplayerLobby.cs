using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Networking;
using Orion.Game.Matchmaking.Networking.Packets;

namespace Orion.Game.Matchmaking.Networking
{
    /// <summary>
    /// Provides the listing of multiplayer games through composited IMatchQueriers.
    /// </summary>
    public sealed class MultiplayerLobby : IDisposable
    {
        #region Fields
        private static readonly TimeSpan timeBeforeReExploring = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan matchListingTimeout = TimeSpan.FromSeconds(15);

        private readonly List<IMatchQuerier> matchFinders = new List<IMatchQuerier>();
        private readonly GameNetworking networking;

        private DateTime lastExploredTime = DateTime.Now;
        private IPv4EndPoint? joiningEndPoint;
        #endregion

        #region Constructors
        public MultiplayerLobby(GameNetworking networking)
        {
            Argument.EnsureNotNull(networking, "networking");
            this.networking = networking;
            matchFinders.Add(new LocalNetworkQuerier(networking, timeBeforeReExploring, matchListingTimeout));
            matchFinders.Add(new MasterServerQuerier("http://www.laissemoichercherca.com/ets/orion.php", matchListingTimeout));

            networking.PacketReceived += OnPacketReceived;
            networking.PeerTimedOut += OnPeerTimedOut;
        }
        #endregion

        #region Events
        public event Action<MultiplayerLobby> MatchesChanged;
        public event Action<MultiplayerLobby, JoinResponseEventArgs> JoinResponseReceived;
        #endregion

        #region Properties
        public IEnumerable<AdvertizedMatch> Matches
        {
            get { return matchFinders.SelectMany(f => f.Matches); }
        }

        public bool IsJoining
        {
            get { return joiningEndPoint.HasValue; }
        }
        #endregion

        #region Methods
        public void Enable()
        {
            matchFinders.ForEach(f => f.IsEnabled = true);
        }

        public void Disable()
        {
            matchFinders.ForEach(f => f.IsEnabled = false);
        }

        public void Update()
        {
            bool matchesChanged = false;
            foreach (IMatchQuerier finder in matchFinders)
            {
                if (finder.Update())
                    matchesChanged = true;
            }
            if (matchesChanged) MatchesChanged.Raise(this);
        }

        public void BeginJoining(IPv4EndPoint endPoint)
        {
            if (IsJoining) throw new InvalidOperationException("Cannot join when already in the process of joining.");

            joiningEndPoint = endPoint;
            networking.Send(JoinRequestPacket.Instance, endPoint);
        }

        public void Dispose()
        {
            matchFinders.ForEach(f => f.Dispose());
            matchFinders.Clear();
        }

        #region Event Handling
        private void OnPacketReceived(GameNetworking sender, GamePacketEventArgs args)
        {
            JoinResponsePacket joinPacket = args.Packet as JoinResponsePacket;
            if (joinPacket != null)
                HandleJoinResponsePacket(args.SenderEndPoint, joinPacket);
        }

        private void OnPeerTimedOut(GameNetworking sender, IPv4EndPoint endPoint)
        {
            if (endPoint == joiningEndPoint)
            {
                joiningEndPoint = null;
                JoinResponseEventArgs eventArgs = new JoinResponseEventArgs(endPoint, false);
                JoinResponseReceived.Raise(this, eventArgs);
            }
        }

        private void HandleJoinResponsePacket(IPv4EndPoint senderEndPoint, JoinResponsePacket packet)
        {
            if (senderEndPoint == joiningEndPoint)
            {
                joiningEndPoint = null;
                JoinResponseEventArgs eventArgs = new JoinResponseEventArgs(senderEndPoint, packet.WasAccepted);
                JoinResponseReceived.Raise(this, eventArgs);
            }
            else
            {
                Debug.Fail("Received an unexpected join response packet.");
            }
        }
        #endregion
        #endregion
    }
}
