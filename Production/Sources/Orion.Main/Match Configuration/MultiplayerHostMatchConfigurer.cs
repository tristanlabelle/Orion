using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Orion.Commandment;
using Orion.GameLogic;
using Orion.Networking;
using Orion.UserInterface;
using Color = System.Drawing.Color;

namespace Orion.Main
{
    sealed class MultiplayerHostMatchConfigurer : MultiplayerMatchConfigurer
    {
        private static readonly byte[] advertizeGameMessage = new byte[2] { (byte)SetupMessageType.Advertise, 12 };
        private static readonly byte[] refuseJoinGameMessage = new byte[] { (byte)SetupMessageType.RefuseJoinRequest };

        private MultiplayerHostMatchConfigurationUI ui;

        public MultiplayerHostMatchConfigurer(SafeTransporter transporter)
            : base(transporter)
        {
            Seed = (int)Environment.TickCount;
            ui = new MultiplayerHostMatchConfigurationUI(transporter);
            ui.PressedExit += ExitGame;
        }

        public new MultiplayerHostMatchConfigurationUI UserInterface
        {
            get { return ui; }
        }

        protected override MatchConfigurationUI AbstractUserInterface
        {
            get { return ui; }
        }

        protected override void Received(SafeTransporter source, NetworkEventArgs args)
        {
            byte[] message = args.Data;
            switch ((SetupMessageType)message[0])
            {
                case SetupMessageType.Explore:
                    Advertise(args.Host);
                    break;

                case SetupMessageType.JoinRequest:
                    TryJoin(args.Host);
                    break;

                case SetupMessageType.LeaveGame:
                    TryLeave(args.Host);
                    break;
            }
        }

        protected override void ExitGame(MatchConfigurationUI ui)
        {
            byte[] exitMessage = new byte[1];
            exitMessage[0] = (byte)SetupMessageType.Exit;
            foreach (IPv4EndPoint peer in peers)
            {
                transporter.SendTo(exitMessage, peer);
            }
        }

        private void Advertise(IPv4EndPoint host)
        {
            int leftSlots = ui.Players.OfType<RemotePlayerSlot>().Count(slot => !slot.RemoteHost.HasValue);
            advertizeGameMessage[1] = (byte)leftSlots;
            transporter.SendTo(advertizeGameMessage, host);
        }

        private void TryJoin(IPv4EndPoint host)
        {
            if (ui.NumberOfPlayers >= ui.MaxNumberOfPlayers)
            {
                transporter.SendTo(refuseJoinGameMessage, host);
                return;
            }

            byte[] seedMessage = new byte[5];
            seedMessage[0] = (byte)SetupMessageType.SetSeed;
            BitConverter.GetBytes(Seed).CopyTo(seedMessage, 1);
            transporter.SendTo(seedMessage, host);

            int newPeerSlotNumber = (byte)ui.NextAvailableSlot;
            
            byte[] setSlotMessage = new byte[3];
            setSlotMessage[0] = (byte)SetupMessageType.SetSlot;
            byte[] addPeerMessage = new byte[8];
            addPeerMessage[0] = (byte)SetupMessageType.SetPeer;
            addPeerMessage[1] = (byte)newPeerSlotNumber;
            BitConverter.GetBytes(host.Address.Value).CopyTo(addPeerMessage, 2);
            BitConverter.GetBytes(host.Port).CopyTo(addPeerMessage, 2 + sizeof(uint));
            foreach (IPv4EndPoint peer in peers)
            {
                transporter.SendTo(addPeerMessage, peer);
            }

            int slotNumber = -1;
            foreach (PlayerSlot slot in ui.Players)
            {
                slotNumber++;
                if (slotNumber == 0) continue;

                if (slot is RemotePlayerSlot)
                {
                    RemotePlayerSlot remotePlayer = (RemotePlayerSlot)slot;
                    if (!remotePlayer.RemoteHost.HasValue)
                    {
                        setSlotMessage[1] = (byte)slotNumber;
                        setSlotMessage[2] = (byte)SlotType.Open;
                        transporter.SendTo(setSlotMessage, host);
                        continue;
                    }

                    IPv4EndPoint peer = ((RemotePlayerSlot)slot).RemoteHost.Value;
                    addPeerMessage[1] = (byte)slotNumber;
                    BitConverter.GetBytes(peer.Address.Value).CopyTo(addPeerMessage, 2);
                    BitConverter.GetBytes(peer.Port).CopyTo(addPeerMessage, 2 + sizeof(uint));
                    transporter.SendTo(addPeerMessage, host);
                    continue;
                }

                if (slot is ClosedPlayerSlot)
                {
                    setSlotMessage[1] = (byte)slotNumber;
                    setSlotMessage[2] = (byte)SlotType.Closed;
                    transporter.SendTo(setSlotMessage, host);
                    continue;
                }

                if (slot is AIPlayerSlot)
                {
                    setSlotMessage[1] = (byte)slotNumber;
                    setSlotMessage[2] = (byte)SlotType.AI;
                    transporter.SendTo(setSlotMessage, host);
                    continue;
                }
            }

            setSlotMessage[1] = (byte)newPeerSlotNumber;
            setSlotMessage[2] = (byte)SlotType.Local;
            transporter.SendTo(setSlotMessage, host);

            ui.UsePlayerForSlot(slotNumber, host);
            peers.Add(host);
        }

        private void TryLeave(IPv4EndPoint host)
        {
            if(!peers.Contains(host)) return;

            int slotNumber = ui.Players.IndexOf(delegate(PlayerSlot slot)
            {
                if(!(slot is RemotePlayerSlot)) return false;
                RemotePlayerSlot remote = (RemotePlayerSlot)slot;
                return remote.RemoteHost.HasValue && remote.RemoteHost.Value == host;
            });

            byte[] setSlotMessage = new byte[3];
            setSlotMessage[0] = (byte)SetupMessageType.SetSlot;
            setSlotMessage[1] = (byte)slotNumber;
            foreach(IPv4EndPoint peer in peers)
            {
                transporter.SendTo(setSlotMessage, peer);
            }
        }
    }
}
