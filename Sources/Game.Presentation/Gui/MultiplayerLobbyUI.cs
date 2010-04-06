using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using OpenTK.Math;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Engine.Networking;
using Orion.Engine.Gui;
using Orion.Game.Matchmaking.Networking;

namespace Orion.Game.Presentation
{
    public sealed class MultiplayerLobbyUI : MaximizedPanel
    {
        #region Fields
        private static readonly byte[] explorePacket = new byte[] { (byte)SetupMessageType.Explore };
        private const int repollAfterFrames = 250;

        private readonly Action<SafeTransporter, NetworkEventArgs> packetReceivedEventHandler;
        private readonly Action<SafeTransporter, IPv4EndPoint> peerTimedOutEventHandler;
        private readonly Dictionary<IPv4EndPoint, Button> hostedGames = new Dictionary<IPv4EndPoint, Button>();
        private readonly int port;
        private SafeTransporter transporter;
        private IPv4EndPoint? requestedJoin;
        private int frameCounter;

        private readonly ListPanel gameListPanel;
        private readonly Rectangle gameButtonRectangle;
        #endregion

        #region Constructors
        public MultiplayerLobbyUI(SafeTransporter transporter)
        {
            port = transporter.Port;
            this.transporter = transporter;
            packetReceivedEventHandler = OnPacketReceived;
            peerTimedOutEventHandler = OnPeerTimedOut;

            Rectangle gameListFrame = Bounds.TranslatedBy(10, 10).ResizedBy(-230, -20);
            gameButtonRectangle = new Rectangle(gameListFrame.Width - 20, 30);
            gameListPanel = new ListPanel(gameListFrame, new Vector2(10, 10));
            Children.Add(gameListPanel);

            Rectangle hostFrame = new Rectangle(gameListFrame.MaxX + 10, gameListFrame.MaxY, 200, -50);
            Button hostButton = new Button(hostFrame, "Héberger");
            hostButton.Triggered += (sender) => { if (!requestedJoin.HasValue) HostPressed.Raise(this); };
            Children.Add(hostButton);

            Rectangle joinRemoteFrame = hostFrame.TranslatedBy(0, -hostFrame.Height - 10);
            Button joinRemoteButton = new Button(joinRemoteFrame, "Jointer par IP");
            joinRemoteButton.Triggered += PressJoinRemoteGame;
            Children.Add(joinRemoteButton);

            Rectangle backButtonFrame = joinRemoteFrame.TranslatedTo(joinRemoteFrame.MinX, 10);
            Button backButton = new Button(backButtonFrame, "Retour");
            backButton.Triggered += (sender) => BackPressed.Raise(this);
            Children.Add(backButton);

            Rectangle pingButtonFrame = joinRemoteFrame.TranslatedBy(0, -joinRemoteFrame.Height - 10);
            Button pingButton = new Button(pingButtonFrame, "Ping");
            pingButton.Triggered += PressPing;
            Children.Add(pingButton);
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when the user decided to go back through the UI.
        /// </summary>
        public event Action<MultiplayerLobbyUI> BackPressed;

        /// <summary>
        /// Raised when the user decided to host a game through the UI.
        /// </summary>
        public event Action<MultiplayerLobbyUI> HostPressed;

        /// <summary>
        /// Raised when the user decided to go join a game through the UI.
        /// </summary>
        public event Action<MultiplayerLobbyUI, IPv4EndPoint> JoinPressed;
        #endregion

        #region Methods
#warning HACK: This should by handled by the MultiplayerLobbyGameState
        protected override void OnAddToParent(ViewContainer parent)
        {
            transporter.Received += packetReceivedEventHandler;
            transporter.TimedOut += peerTimedOutEventHandler;
            transporter.Broadcast(explorePacket, port);
        }

        protected override void OnRemovedFromParent(ViewContainer parent)
        {
            transporter.Received -= packetReceivedEventHandler;
            transporter.TimedOut -= peerTimedOutEventHandler;
        }

        protected override void Update(float timeDeltaInSeconds)
        {
            transporter.Poll();
            frameCounter++;
            if (frameCounter % repollAfterFrames == 0)
                transporter.Broadcast(explorePacket, port);

            base.Update(timeDeltaInSeconds);
        }

        private void OnPacketReceived(SafeTransporter source, NetworkEventArgs args)
        {
            SetupMessageType messageType = (SetupMessageType)args.Data[0];
            switch (messageType)
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
                        JoinPressed.Raise(this, requestedJoin.Value);
                    }
                    break;

                case SetupMessageType.RefuseJoinRequest:
                    if (args.Host == requestedJoin)
                    {
                        Instant.DisplayAlert(this, "L'hôte a refusé votre demande.");
                    }
                    break;

                case SetupMessageType.Explore:
                    break;

                default:
                    //Debug.Fail("Unexpected packet message type: {0}.".FormatInvariant(messageType));
                    break;
            }
        }

        private void OnPeerTimedOut(SafeTransporter transporter, IPv4EndPoint host)
        {
            Instant.DisplayAlert(this, "Impossible de rejointer {0}.".FormatInvariant(ResolveHostAddress(host)));
            requestedJoin = null;
        }

        private void ShowGame(IPv4EndPoint host, int numberOfPlacesLeft)
        {
            string caption = "{0} ({1} places libres)".FormatInvariant(ResolveHostAddress(host), numberOfPlacesLeft);
            if (!hostedGames.ContainsKey(host))
            {
                Button button = new Button(gameButtonRectangle, caption);
                button.Triggered += b => { if (!requestedJoin.HasValue) AskJoinGame(host); };
                gameListPanel.Children.Add(button);
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

        private void PressJoinRemoteGame(Button sender)
        {
            if (!requestedJoin.HasValue)
                Instant.Prompt(this, "Quelle adresse voulez-vous jointer?", JoinAddress);
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
                    Instant.DisplayAlert(this, "La valeur {0} est un nom de port invalide.".FormatInvariant(parts[1]));
                    return;
                }
                catch (SocketException)
                {
                    Instant.DisplayAlert(this, "Impossible de résoudre le nom ou l'adresse {0}.".FormatInvariant(parts[0]));
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
                    Instant.DisplayAlert(this, "Impossible de résoudre le nom ou l'adresse {0}.".FormatInvariant(address));
                    return;
                }
                catch (InvalidOperationException)
                {
                    Instant.DisplayAlert(this, "Impossible de trouver une adresse IPv4 pour l'hôte {0}".FormatInvariant(address));
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
            Instant.Prompt(this, "Entrez l'adresse IP à pinger:", Ping);
        }

        private void Ping(string address)
        {
            IPv4EndPoint? endPoint = TryParseAddress(address);
            if (!endPoint.HasValue)
            {
                Instant.DisplayAlert(this, "Impossible de résoudre {0}.".FormatInvariant(address));
            }

            transporter.Ping(endPoint.Value);
            Instant.DisplayAlert(this, "{0} a été pingé avec succès.".FormatInvariant(endPoint.Value));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                transporter.Received -= packetReceivedEventHandler;
                BackPressed = null;
                HostPressed = null;
                JoinPressed = null;
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}