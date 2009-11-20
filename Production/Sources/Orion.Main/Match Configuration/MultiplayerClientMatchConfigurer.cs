using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Orion.Commandment;
using Orion.GameLogic;
using Orion.Networking;
using Orion.UserInterface;
using Orion.UserInterface.Widgets;
using Color = System.Drawing.Color;

namespace Orion.Main
{
    sealed class MultiplayerClientMatchConfigurer : MultiplayerMatchConfigurer
    {
        #region Fields
        private IPv4EndPoint gameHost;
        private MultiplayerClientMatchConfigurationUI ui;
        #endregion

        #region Constructors
        public MultiplayerClientMatchConfigurer(SafeTransporter transporter, IPv4EndPoint host)
            : base(transporter)
        {
            this.gameHost = host;
            ui = new MultiplayerClientMatchConfigurationUI(transporter);
            ui.PressedExit += ExitGame;
            ui.Entered += EnterRootView;
        }
        #endregion

        #region Properties

        public new MultiplayerClientMatchConfigurationUI UserInterface
        {
            get { return ui; }
        }

        protected override MatchConfigurationUI AbstractUserInterface
        {
            get { return ui; }
        }
        
        #endregion

        #region Methods
        protected override void Received(SafeTransporter source, NetworkEventArgs args)
        {
            if (args.Host != gameHost) return;

            byte[] data = args.Data;
            switch ((SetupMessageType)data[0])
            {
                case SetupMessageType.SetPeer: SetPeer(data); break;
                case SetupMessageType.SetSlot: SetSlot(data); break;
                case SetupMessageType.SetSeed: SetSeed(data); break;
                case SetupMessageType.StartGame: StartGame(); break;
                case SetupMessageType.Exit: ForceExit(); break;
            }
        }

        protected override void TimedOut(SafeTransporter source, IPv4EndPoint host)
        {
            if(host == gameHost) ForceExit();
        }

        private void EnterRootView(UIDisplay uiDisplay, RootView root)
        {
            ui.UsePlayerForSlot(0, gameHost);
        }

        protected override void ExitGame(MatchConfigurationUI ui)
        {
            byte[] quitMessage = new byte[1];
            quitMessage[0] = (byte)SetupMessageType.LeaveGame;
            transporter.SendTo(quitMessage, gameHost);
        }

        private void SetPeer(byte[] bytes)
        {
            uint address = BitConverter.ToUInt32(bytes, 2);
            ushort port = BitConverter.ToUInt16(bytes, 2 + sizeof(uint));
            IPv4EndPoint peer = new IPv4EndPoint(new IPv4Address(address), port);
            UserInterface.UsePlayerForSlot(bytes[1], peer);
        }

        private void SetSlot(byte[] bytes)
        {
            switch ((SlotType)bytes[2])
            {
                case SlotType.Closed: UserInterface.CloseSlot(bytes[1]); break;
                case SlotType.Open: UserInterface.OpenSlot(bytes[1]); break;
                case SlotType.AI: UserInterface.UseAIForSlot(bytes[1]); break;
                case SlotType.Local: UserInterface.SetLocalPlayerForSlot(bytes[1]); break;
            }
        }

        private void SetSeed(byte[] bytes)
        {
            Seed = BitConverter.ToInt32(bytes, 1);
        }

        private void ForceExit()
        {
            RootView root = ui.Root;
            ui.Root.PopDisplay(ui);
            ui.Dispose();
            Dispose();
            Instant.DisplayAlert(root.TopmostDisplay, "You've been disconnected.");
        }
        #endregion
    }
}
