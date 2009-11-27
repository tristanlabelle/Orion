using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using OpenTK.Math;
using Orion.Geometry;
using Orion.Networking;
using Orion.UserInterface.Widgets;

namespace Orion.UserInterface
{
    public sealed class LocalMultiplayerLobby : UIDisplay
    {
        #region Fields
        private static readonly byte[] explorePacket = new byte[] { (byte)SetupMessageType.Explore };
        private const int repollAfterFrames = 250;

        private readonly GenericEventHandler<SafeTransporter, NetworkEventArgs> receptionDelegate;
        private readonly GenericEventHandler<SafeTransporter, IPv4EndPoint> timeoutDelegate;
        private readonly Dictionary<IPv4EndPoint, Button> hostedGames = new Dictionary<IPv4EndPoint, Button>();
        private readonly int port;
        private SafeTransporter transporter;
        private IPv4EndPoint? requestedJoin;
        private int frameCounter;

        private readonly ListFrame gamesFrame;
        #endregion

        #region Constructors
        public LocalMultiplayerLobby(SafeTransporter transporter)
        {
            port = transporter.Port;
            this.transporter = transporter;
            receptionDelegate = OnReceive;
            timeoutDelegate = OnTimeout;

            Rectangle gamesFrameRect = Bounds.TranslatedBy(10, 10).ResizedBy(-230, -20);
            gamesFrame = new ListFrame(gamesFrameRect, new Rectangle(gamesFrameRect.Width - 20, 30), new Vector2(10, 10));
            Children.Add(gamesFrame);

            Rectangle hostFrame = new Rectangle(gamesFrameRect.MaxX + 10, gamesFrameRect.MaxY, 200, -50);
            Button hostButton = new Button(hostFrame, "Host");
            hostButton.Triggered += PressHostGame;
            Children.Add(hostButton);

            Rectangle joinRemoteFrame = hostFrame.TranslatedBy(0, -hostFrame.Height - 10);
            Button joinRemoteButton = new Button(joinRemoteFrame, "Join with IP...");
            joinRemoteButton.Triggered += PressJoinRemoteGame;
            Children.Add(joinRemoteButton);

            Rectangle backButtonFrame = joinRemoteFrame.TranslatedTo(joinRemoteFrame.MinX, 10);
            Button backButton = new Button(backButtonFrame, "Back");
            backButton.Triggered += PressBack;
            Children.Add(backButton);

            Rectangle pingButtonFrame = joinRemoteFrame.TranslatedBy(0, -joinRemoteFrame.Height - 10);
            Button pingButton = new Button(pingButtonFrame, "Ping");
            pingButton.Triggered += PressPing;
            Children.Add(pingButton);
        }
        #endregion

        #region Events
        public event GenericEventHandler<LocalMultiplayerLobby> HostedGame;
        public event GenericEventHandler<LocalMultiplayerLobby, IPv4EndPoint> JoinedGame;
        #endregion

        #region Methods

        internal override void OnEnter(RootView enterOn)
        {
            transporter.Received += receptionDelegate;
            transporter.TimedOut += timeoutDelegate;
            transporter.Broadcast(explorePacket, port);
            base.OnEnter(enterOn);
        }

        internal override void OnShadow(RootView hiddenOf)
        {
            transporter.Received -= receptionDelegate;
            transporter.TimedOut -= timeoutDelegate;
            base.OnShadow(hiddenOf);
        }

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
                    ShowGame(args.Host, args.Data[1]);
                    break;

                case SetupMessageType.RemoveGame:
                case SetupMessageType.Exit:
                    RemoveGame(args.Host);
                    break;

                case SetupMessageType.AcceptJoinRequest:
                    if (requestedJoin.HasValue && args.Host == requestedJoin.Value)
                    {
                        JoinGame(requestedJoin.Value);
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
            Instant.DisplayAlert(this, "Unable to reach {0}.".FormatInvariant(ResolveHostAddress(host)));
            requestedJoin = null;
        }

        private void ShowGame(IPv4EndPoint host, int numberOfPlacesLeft)
        {
            string caption = "{0} ({1} places left)".FormatInvariant(ResolveHostAddress(host), numberOfPlacesLeft);
            if (!hostedGames.ContainsKey(host))
            {
                Button button = new Button(gamesFrame.ItemFrame, caption);
                button.Triggered += b => { if (!requestedJoin.HasValue) AskJoinGame(host); };
                gamesFrame.Children.Add(button);
                hostedGames[host] = button;
            }
            else
            {
                hostedGames[host].Caption = caption;
            }
        }

        private void RemoveGame(IPv4EndPoint host)
        {
            if (hostedGames.ContainsKey(host))
            {
                hostedGames[host].Dispose();
                hostedGames.Remove(host);
            }
        }

        private void AskJoinGame(IPv4EndPoint host)
        {
            Debug.WriteLine("Asking {0} to join the game.".FormatInvariant(host));

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
            if (!requestedJoin.HasValue)
                Instant.Prompt(this, "What is the address of the game you want to join?", JoinAddress);
        }

        private void PressHostGame(Button sender)
        {
            GenericEventHandler<LocalMultiplayerLobby> handler = HostedGame;
            if (handler != null && !requestedJoin.HasValue) handler(this);
        }

        private void PressBack(Button sender)
        {
            if (!requestedJoin.HasValue)
                Parent.PopDisplay(this);
        }

        private void JoinAddress(string address)
        {
            IPv4Address hostAddress;
            ushort port;

            if (address.Contains(':'))
            {
                string[] parts = address.Split(':');
                try
                {
                    hostAddress = (IPv4Address)Dns.GetHostAddresses(parts[0])
                        .First(a => a.AddressFamily == AddressFamily.InterNetwork);
                    port = ushort.Parse(parts[1]);
                }
                catch (FormatException)
                {
                    Instant.DisplayAlert(this, "Value {0} is an invalid port number.".FormatInvariant(parts[1]));
                    return;
                }
                catch (SocketException)
                {
                    Instant.DisplayAlert(this, "Cannot resolve name or address {0}.".FormatInvariant(parts[0]));
                    return;
                }
            }
            else
            {
                try
                {
                    hostAddress = (IPv4Address)Dns.GetHostAddresses(address)
                        .First(a => a.AddressFamily == AddressFamily.InterNetwork);
                    port = transporter.Port;
                }
                catch (SocketException)
                {
                    Instant.DisplayAlert(this, "Cannot resolve name or address {0}.".FormatInvariant(address));
                    return;
                }
                catch (InvalidOperationException)
                {
                    Instant.DisplayAlert(this, "Could not find an IPv4 address for host {0}".FormatInvariant(address));
                    return;
                }
            }

            AskJoinGame(new IPv4EndPoint(hostAddress, port));
        }

        private string ResolveHostAddress(IPEndPoint endPoint)
        {
            string hostName;
            try
            {
                IPHostEntry hostEntry = Dns.GetHostEntry((IPAddress)endPoint.Address);
                hostName = hostEntry.HostName;
            }
            catch (SocketException)
            {
                hostName = endPoint.Address.ToString();
            }
            if (endPoint.Port != transporter.Port)
            {
                hostName += ":{0}".FormatInvariant(endPoint.Port);
            }
            return hostName;
        }

        private IPv4EndPoint? TryParseAddress(string address)
        {
            IPv4Address hostAddress;
            ushort port;

            if (address.Contains(':'))
            {
                string[] parts = address.Split(':');
                try
                {
                    hostAddress = (IPv4Address)Dns.GetHostAddresses(parts[0])
                        .First(a => a.AddressFamily == AddressFamily.InterNetwork);
                    port = ushort.Parse(parts[1]);
                }
                catch (FormatException)
                {
                    return null;
                }
                catch (SocketException)
                {
                    return null;
                }
            }
            else
            {
                try
                {
                    hostAddress = (IPv4Address)Dns.GetHostAddresses(address)
                        .First(a => a.AddressFamily == AddressFamily.InterNetwork);
                    port = transporter.Port;
                }
                catch (SocketException)
                {
                    return null;
                }
                catch (InvalidOperationException)
                {
                    return null;
                }
            }

            return new IPv4EndPoint(hostAddress, port);
        }

        private void PressPing(Button sender)
        {
            Instant.Prompt(this, "Enter the IP address to ping:", Ping);
        }

        private void Ping(string address)
        {
            IPv4EndPoint? endPoint = TryParseAddress(address);
            if (!endPoint.HasValue)
            {
                Instant.DisplayAlert(this, "Unable to resolve {0}.".FormatInvariant(address));
            }

            transporter.Ping(endPoint.Value);
            Instant.DisplayAlert(this, "{0} was successfully ping'ed.".FormatInvariant(endPoint.Value));
        }

        public override void Dispose()
        {
            transporter.Received -= receptionDelegate;
            HostedGame = null;
            JoinedGame = null;
            base.Dispose();
        }

        #endregion
    }
}