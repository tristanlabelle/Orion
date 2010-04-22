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
    /// Provides the listing of multiplayer games.
    /// </summary>
    public sealed class MultiplayerLobby : IDisposable
    {
        #region Fields
        private static readonly TimeSpan timeBeforeReExploring = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan matchListingTimeout = TimeSpan.FromSeconds(15);

        private readonly GameNetworking networking;
        private readonly List<AdvertizedMatch> matches = new List<AdvertizedMatch>();
        private readonly ReadOnlyCollection<AdvertizedMatch> readOnlyMatches;

        private readonly Action<GameNetworking, GamePacketEventArgs> packetReceivedEventHandler;
        private readonly Action<GameNetworking, IPv4EndPoint> peerTimedOutEventHandler;

        private bool isEnabled = true;
        private DateTime lastExploredTime = DateTime.Now;
        private IPv4EndPoint? joiningEndPoint;
        #endregion

        #region Constructors
        public MultiplayerLobby(GameNetworking networking)
        {
            Argument.EnsureNotNull(networking, "networking");

            this.networking = networking;
            this.readOnlyMatches = matches.AsReadOnly();
            this.packetReceivedEventHandler = OnPacketReceived;
            this.peerTimedOutEventHandler = OnPeerTimedOut;

            AttachEventHandlers();
        }
        #endregion

        #region Events
        public event Action<MultiplayerLobby> MatchesChanged;
        public event Action<MultiplayerLobby, JoinResponseEventArgs> JoinResponseReceived;
        #endregion

        #region Properties
        public ReadOnlyCollection<AdvertizedMatch> Matches
        {
            get { return readOnlyMatches; }
        }

        public bool IsJoining
        {
            get { return joiningEndPoint.HasValue; }
        }

        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                if (value == isEnabled) return;

                isEnabled = value;
                if (isEnabled)
                {
                    AttachEventHandlers();
                    Explore();
                }
                else
                {
                    DetachEventHandlers();
                    matches.Clear();
                }
            }
        }
        #endregion

        #region Methods
        public void Update()
        {
            DateTime now = DateTime.Now;
            
            // Removed timed out matches
            int removedMatchCount = matches
                .RemoveAll(m => now - m.LastUpdateTime > matchListingTimeout);
            if (removedMatchCount > 0) MatchesChanged.Raise(this);

            // Explore matches
            if (now - lastExploredTime > timeBeforeReExploring)
            {
                networking.Broadcast(ExploreMatchesPacket.Instance);
                lastExploredTime = now;
            }

            networking.Poll();
        }

        /// <summary>
        /// Sends a packet to discover if some matches exist on the local network.
        /// </summary>
        public void Explore()
        {
            networking.Broadcast(ExploreMatchesPacket.Instance);
            lastExploredTime = DateTime.Now;
        }

        public void BeginJoining(IPv4EndPoint endPoint)
        {
            if (IsJoining) throw new InvalidOperationException("Cannot join when already in the process of joining.");

            joiningEndPoint = endPoint;
            networking.Send(JoinRequestPacket.Instance, endPoint);
        }

        public void Dispose()
        {
            DetachEventHandlers();
            MatchesChanged = null;
            matches.Clear();
        }

        #region Event Handling
        private void AttachEventHandlers()
        {
            networking.PacketReceived += packetReceivedEventHandler;
            networking.PeerTimedOut += peerTimedOutEventHandler;
        }

        private void DetachEventHandlers()
        {
            networking.PacketReceived -= packetReceivedEventHandler;
            networking.PeerTimedOut -= peerTimedOutEventHandler;
        }

        private void OnPacketReceived(GameNetworking sender, GamePacketEventArgs args)
        {
            if (args.Packet is AdvertizeMatchPacket)
            {
                HandleAdvertizeMatchPacket(args.SenderEndPoint, (AdvertizeMatchPacket)args.Packet);
            }
            else if (args.Packet is DelistMatchPacket)
            {
                int removedMatchCount = matches.RemoveAll(m => m.EndPoint == args.SenderEndPoint);
                MatchesChanged.Raise(this);
            }
            else if (args.Packet is JoinResponsePacket)
            {
                HandleJoinResponsePacket(args.SenderEndPoint, (JoinResponsePacket)args.Packet);
            }
        }

        private void OnPeerTimedOut(GameNetworking sender, IPv4EndPoint endPoint)
        {
            if (endPoint == joiningEndPoint)
            {
                joiningEndPoint = null;
                JoinResponseEventArgs eventArgs = new JoinResponseEventArgs(endPoint, false);
                JoinResponseReceived.Raise(this, eventArgs);
            }

            int removedMatchCount = matches.RemoveAll(m => m.EndPoint == endPoint);
            if (removedMatchCount > 0) MatchesChanged.Raise(this);
        }

        private void HandleAdvertizeMatchPacket(IPv4EndPoint senderEndPoint, AdvertizeMatchPacket packet)
        {
            AdvertizedMatch match = matches
                .FirstOrDefault(m => m.EndPoint == senderEndPoint);
            if (match == null)
            {
                match = new AdvertizedMatch(senderEndPoint, packet.Name, packet.OpenSlotCount);
                matches.Add(match);
            }
            else
            {
                match.OpenSlotCount = packet.OpenSlotCount;
                match.KeepAlive();
            }

            MatchesChanged.Raise(this);
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
