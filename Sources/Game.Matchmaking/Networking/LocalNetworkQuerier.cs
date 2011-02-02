using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Orion.Engine.Networking;
using Orion.Game.Matchmaking.Networking.Packets;

namespace Orion.Game.Matchmaking.Networking
{
    /// <summary>
    /// A match provider which detects match broadcasted on the local network.
    /// </summary>
    public sealed class LocalNetworkQuerier : IMatchQuerier
    {
        #region Fields
        private readonly TimeSpan timeBeforeReExploring;
        private readonly TimeSpan timeBeforeExpiring;

        private readonly GameNetworking networking;
        private readonly List<AdvertizedMatch> matches = new List<AdvertizedMatch>();
        private readonly ReadOnlyCollection<AdvertizedMatch> readOnlyMatches;

        private readonly Action<GameNetworking, GamePacketEventArgs> packetReceivedEventHandler;
        private readonly Action<GameNetworking, IPv4EndPoint> peerTimedOutEventHandler;

        private bool listingChanged = false;
        private bool isEnabled = true;
        private DateTime lastExploredTime = DateTime.MinValue;
        #endregion

        #region Constructors
        public LocalNetworkQuerier(GameNetworking networking, TimeSpan timeBeforeReExploring, TimeSpan timeBeforeExpiring)
        {
            this.timeBeforeExpiring = timeBeforeExpiring;
            this.timeBeforeReExploring = timeBeforeReExploring;
            this.networking = networking;
            this.readOnlyMatches = matches.AsReadOnly();

            packetReceivedEventHandler = OnPacketReceived;
            peerTimedOutEventHandler = OnPeerTimedOut;

            AttachEventHandlers();
        }
        #endregion

        #region Properties
        public string Tag { get { return "LAN"; } }

        public ICollection<AdvertizedMatch> Matches
        {
            get { return readOnlyMatches; }
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
        /// <summary>
        /// Sends a packet to discover if some matches exist on the local network.
        /// </summary>
        public bool Update()
        {
            if (!isEnabled) throw new InvalidOperationException("Cannot update a disabled IMatchFinder");
            DateTime now = DateTime.Now;

            // Removed timed out matches
            int removedMatchCount = matches
                .RemoveAll(m => now - m.LastUpdateTime > timeBeforeExpiring);

            // Explore matches
            if (now - lastExploredTime > timeBeforeReExploring)
                Explore();

            networking.Poll();
            bool updated = listingChanged || removedMatchCount > 0;
            listingChanged = false;
            return updated;
        }

        private void Explore()
        {
            networking.Broadcast(ExploreMatchesPacket.Instance);
            lastExploredTime = DateTime.Now;
        }

        public void Dispose()
        {
            IsEnabled = false;
        }

        #region Events Handling
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
                matches.RemoveAll(m => m.EndPoint == args.SenderEndPoint);
                listingChanged = true;
            }
        }

        private void OnPeerTimedOut(GameNetworking sender, IPv4EndPoint endPoint)
        {
            int removedMatchCount = matches.RemoveAll(m => m.EndPoint == endPoint);
            if (removedMatchCount > 0) listingChanged = true;
        }

        private void HandleAdvertizeMatchPacket(IPv4EndPoint senderEndPoint, AdvertizeMatchPacket packet)
        {
            AdvertizedMatch match = matches
                .FirstOrDefault(m => m.EndPoint == senderEndPoint);
            if (match == null)
            {
                match = new AdvertizedMatch(this, senderEndPoint, packet.Name, packet.OpenSlotCount);
                matches.Add(match);
            }
            else
            {
                match.OpenSlotCount = packet.OpenSlotCount;
                match.KeepAlive();
            }

            listingChanged = true;
        }
        #endregion
        #endregion
    }
}
