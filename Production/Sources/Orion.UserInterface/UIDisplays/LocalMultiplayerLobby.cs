using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Orion.UserInterface.Widgets;
using Orion.Networking;
using Orion.Geometry;

namespace Orion.UserInterface
{
    public sealed class LocalMultiplayerLobby : UIDisplay
    {
        #region Fields
        private static readonly byte[] explorePacket = new byte[] { (byte)SetupMessageType.Explore };
        private const int repollAfterFrames = 250;

        private readonly GenericEventHandler<SafeTransporter, NetworkEventArgs> receptionDelegate;
        private readonly GenericEventHandler<SafeTransporter, IPv4EndPoint> timeoutDelegate;
        private readonly Dictionary<IPv4EndPoint, int> hostedGames = new Dictionary<IPv4EndPoint, int>();
        private readonly int port;
        private SafeTransporter transporter;
        private IPv4EndPoint requestedJoin;
        private int frameCounter;

        private readonly Frame gamesFrame;
        #endregion

        #region Constructors
        public LocalMultiplayerLobby(SafeTransporter transporter)
        {
            port = transporter.Port;
            this.transporter = transporter;
            receptionDelegate = OnReceive;
            timeoutDelegate = OnTimeout;

            Rectangle gamesFrameRect = Bounds.TranslatedBy(10, 10).ResizedBy(-230, -20);
            gamesFrame = new Frame(gamesFrameRect);
            Children.Add(gamesFrame);

            Rectangle hostFrame = new Rectangle(gamesFrameRect.MaxX + 10, gamesFrameRect.MaxY, 200, -50);
            Button hostButton = new Button(hostFrame, "Host");
            hostButton.Pressed += PressHostGame;
            Children.Add(hostButton);

            Rectangle joinRemoteFrame = hostFrame.TranslatedBy(0, -hostFrame.Height - 10);
            Button joinRemoteButton = new Button(joinRemoteFrame, "Join with IP...");
            joinRemoteButton.Pressed += PressJoinRemoteGame;
            Children.Add(joinRemoteButton);
        }
        #endregion

        #region Events
        public event GenericEventHandler<LocalMultiplayerLobby> HostedGame;
        public event GenericEventHandler<LocalMultiplayerLobby, IPv4EndPoint> JoinedGame;
        #endregion

        internal override void OnEnter(RootView enterOn)
        {
            transporter.Received += receptionDelegate;
            transporter.TimedOut += timeoutDelegate;
            transporter.Broadcast(explorePacket, port);
        }

        internal override void OnShadow(RootView hiddenOf)
        { }

        protected override void OnUpdate(UpdateEventArgs args)
        {
            transporter.Poll();
            frameCounter++;
            if (frameCounter % repollAfterFrames == 0)
                transporter.Broadcast(explorePacket, port);

            base.OnUpdate(args);
        }

        private void OnReceive(SafeTransporter source, NetworkEventArgs args)
        {
            switch ((SetupMessageType)args.Data[0])
            {
                case SetupMessageType.Advertise:
                    hostedGames[args.Host] = args.Data[1];
                    UpdateGamesList();
                    break;

                case SetupMessageType.StartGame:
                    if (hostedGames.ContainsKey(args.Host))
                        hostedGames.Remove(args.Host);
                    UpdateGamesList();
                    break;

                case SetupMessageType.AcceptJoinRequest:
                    if (args.Host == requestedJoin)
                    {
                        JoinGame(requestedJoin);
                    }
                    break;

                case SetupMessageType.RefuseJoinRequest:
                    if (args.Host == requestedJoin)
                    {
                        Instant.DisplayAlert(this, "The host refused your request to join the game.");
                    }
                    break;
            }
        }

        private void OnTimeout(SafeTransporter transporter, IPv4EndPoint host)
        {
        }

        private void UpdateGamesList()
        {
            while(Children.Count > 0) Children[0].Dispose();

            Rectangle buttonFrame = new Rectangle(10, gamesFrame.Bounds.MaxY - 10, gamesFrame.Bounds.MaxX - 20, 30);
            foreach (KeyValuePair<IPv4EndPoint, int> game in hostedGames)
            {
                Button button = new Button(buttonFrame, "{0} ({1} players)".FormatInvariant(game.Key, game.Value));
                button.Pressed += delegate(Button source) { AskJoinGame(game.Key); };
                gamesFrame.Children.Add(button);
                buttonFrame = buttonFrame.TranslatedBy(0, -(buttonFrame.Height + 10));
            }
        }

        private void AskJoinGame(IPv4EndPoint host)
        {
            Console.WriteLine("Asking {0} to join the game", host);

            byte[] packet = new byte[1];
            packet[0] = (byte)SetupMessageType.JoinRequest;
            transporter.SendTo(packet, host);
            requestedJoin = host;
        }

        private void JoinGame(IPv4EndPoint host)
        {
            GenericEventHandler<LocalMultiplayerLobby, IPv4EndPoint> handler = JoinedGame;
            if (handler != null)
            {
                handler(this, host);
            }
        }

        private void PressJoinRemoteGame(Button sender)
        {
            Instant.Prompt(this, "What is the address:port of the game you want to join?", JoinAddress);
        }

        private void PressHostGame(Button sender)
        {
            GenericEventHandler<LocalMultiplayerLobby> handler = HostedGame;
            if (handler != null) handler(this);
        }

        private void JoinAddress(string address)
        {
            try
            {
                IPv4EndPoint host = IPv4EndPoint.Parse(address);
                AskJoinGame(host);
            }
            catch (FormatException)
            {
                string message = "\"{0}\" is not a valid IP address representation. Please use the format [IP address]:[port].".FormatInvariant(address);
                Instant.DisplayAlert(this, message);
            }
        }

        public override void Dispose()
        {
            transporter.Received -= receptionDelegate;
            HostedGame = null;
            JoinedGame = null;
            base.Dispose();
        }
    }
}
