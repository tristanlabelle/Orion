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
            ui.PressedStartGame += PressStartGame;
            ui.PressedExit += ExitGame;
            ui.SlotOccupationChanged += SlotChanged;
            ui.KickedPlayer += KickedPlayer;
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

        protected override void TimedOut(SafeTransporter source, IPv4EndPoint host)
        {
            if (UserInterface.PlayerAddresses.Contains(host)) TryLeave(host);
        }

        private void SlotChanged(int slotNumber, PlayerSlot newValue)
        {
            byte[] setSlotMessage = new byte[3];
            setSlotMessage[0] = (byte)SetupMessageType.SetSlot;
            setSlotMessage[1] = (byte)slotNumber;
            if (newValue is RemotePlayerSlot) setSlotMessage[2] = (byte)SlotType.Open;
            else if (newValue is AIPlayerSlot) setSlotMessage[2] = (byte)SlotType.AI;
            else if (newValue is ClosedPlayerSlot) setSlotMessage[2] = (byte)SlotType.Closed;
            else throw new InvalidOperationException("Unknown slot type selected");

            transporter.SendTo(setSlotMessage, ui.PlayerAddresses);
        }

        private void KickedPlayer(IPv4EndPoint peer)
        {
            byte[] kickMessage = new byte[1];
            kickMessage[0] = (byte)SetupMessageType.Exit;
            transporter.SendTo(kickMessage, peer);
        }

        protected override void ExitGame(MatchConfigurationUI ui)
        {
            byte[] exitMessage = new byte[1];
            exitMessage[0] = (byte)SetupMessageType.Exit;
            foreach (IPv4EndPoint peer in UserInterface.PlayerAddresses)
            {
                transporter.SendTo(exitMessage, peer);
            }
        }

        private void PressStartGame(MatchConfigurationUI ui)
        {
            byte[] startGameMessage = new byte[1];
            startGameMessage[0] = (byte)SetupMessageType.StartGame;
            transporter.SendTo(startGameMessage, UserInterface.PlayerAddresses);
            StartGame();
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

            byte[] accept = new byte[1];
            accept[0] = (byte)SetupMessageType.AcceptJoinRequest;
            transporter.SendTo(accept, host);

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
            foreach (IPv4EndPoint peer in ui.PlayerAddresses)
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

            ui.UsePlayerForSlot(newPeerSlotNumber, host);
        }

        private void TryLeave(IPv4EndPoint host)
        {
            if (!UserInterface.PlayerAddresses.Contains(host)) return;

            int slotNumber = ui.Players.IndexOf(delegate(PlayerSlot slot)
            {
                if (!(slot is RemotePlayerSlot)) return false;
                RemotePlayerSlot remote = (RemotePlayerSlot)slot;
                return remote.RemoteHost.HasValue && remote.RemoteHost.Value == host;
            });

            byte[] setSlotMessage = new byte[3];
            setSlotMessage[0] = (byte)SetupMessageType.SetSlot;
            setSlotMessage[1] = (byte)slotNumber;
            setSlotMessage[2] = (byte)SlotType.Open;
            foreach (IPv4EndPoint peer in UserInterface.PlayerAddresses)
            {
                transporter.SendTo(setSlotMessage, peer);
            }

            ui.OpenSlot(slotNumber);
        }
    }
}
