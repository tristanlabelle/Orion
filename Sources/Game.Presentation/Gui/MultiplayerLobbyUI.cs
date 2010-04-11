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
using System.Globalization;

namespace Orion.Game.Presentation.Gui
{
    public sealed class MultiplayerLobbyUI : MaximizedPanel
    {
        #region Fields
        private readonly ListPanel matchListPanel;
        private readonly Rectangle matchButtonFrame;
        #endregion

        #region Constructors
        public MultiplayerLobbyUI()
        {
            Rectangle matchListFrame = Bounds.TranslatedBy(10, 10).ResizedBy(-230, -20);
            matchButtonFrame = new Rectangle(matchListFrame.Width - 20, 30);
            matchListPanel = new ListPanel(matchListFrame, new Vector2(10, 10));
            Children.Add(matchListPanel);

            Rectangle hostFrame = new Rectangle(matchListFrame.MaxX + 10, matchListFrame.MaxY, 200, -50);
            Button hostButton = new Button(hostFrame, "Héberger");
            hostButton.Triggered += (sender) => HostPressed.Raise(this);
            Children.Add(hostButton);

            Rectangle joinRemoteFrame = hostFrame.TranslatedBy(0, -hostFrame.Height - 10);
            Button joinRemoteButton = new Button(joinRemoteFrame, "Jointer par IP");
            joinRemoteButton.Triggered += OnJoinByIPPressed;
            Children.Add(joinRemoteButton);

            Rectangle backButtonFrame = joinRemoteFrame.TranslatedTo(joinRemoteFrame.MinX, 10);
            Button backButton = new Button(backButtonFrame, "Retour");
            backButton.Triggered += (sender) => BackPressed.Raise(this);
            Children.Add(backButton);

            Rectangle pingButtonFrame = joinRemoteFrame.TranslatedBy(0, -joinRemoteFrame.Height - 10);
            Button pingButton = new Button(pingButtonFrame, "Ping");
            pingButton.Triggered += OnPingButtonPressed;
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
        /// Raised when the user decided to join a game through the UI.
        /// </summary>
        public event Action<MultiplayerLobbyUI, AdvertizedMatch> JoinPressed;

        /// <summary>
        /// Raised when the user decided to join a game by entering its IP end point.
        /// </summary>
        public event Action<MultiplayerLobbyUI, IPv4EndPoint> JoinByIpPressed;

        /// <summary>
        /// Raised when the user has entered an address to be pinged.
        /// </summary>
        public event Action<MultiplayerLobbyUI, IPv4EndPoint> PingPressed;
        #endregion

        #region Properties
        public bool IsEnabled
        {
            get { return Children.OfType<Button>().First().Enabled; }
            set
            {
                if (value == IsEnabled) return;

                foreach (Button button in Children.OfType<Button>())
                    button.Enabled = value;
                foreach (Button button in matchListPanel.Children.OfType<Button>())
                    button.Enabled = value;
            }
        }
        #endregion

        #region Methods
        public void ClearMatches()
        {
            while (Children.Count > 0)
            {
                ViewContainer child = Children.First();
                Children.Remove(child);
                child.Dispose();
            }
        }

        public void AddMatch(AdvertizedMatch match)
        {
            Argument.EnsureNotNull(match, "match");

            string caption = "{0} ({1} places libres)".FormatInvariant(match.Name, match.OpenSlotCount);
            Button button = new Button(matchButtonFrame, caption);
            button.Triggered += b => JoinPressed.Raise(this, match);
            matchListPanel.Children.Add(button);
        }

        private void OnJoinByIPPressed(Button sender)
        {
            Instant.Prompt(this, "Quelle adresse voulez-vous jointer?", JoinAddress);
        }

        private void JoinAddress(string endPointString)
        {
            IPv4EndPoint? endPoint = ParseEndPointWithAlerts(endPointString);
            if (!endPoint.HasValue) return;

            JoinByIpPressed.Raise(this, endPoint.Value);
        }

        private IPv4EndPoint? ParseEndPointWithAlerts(string endPointString)
        {
            string[] parts = endPointString.Split(':');
            if (parts.Length > 2)
            {
                Instant.DisplayAlert(this, "Adresse invalide, trop de ':'.");
                return null;
            }

            IPv4Address address;
            if (!IPv4Address.TryParse(parts[0], out address))
            {
                // Attempt to resolve as a host name
                try
                {
                    IPAddress resolvedAddress = Dns.GetHostAddresses(parts[0])
                        .FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
                    if (resolvedAddress == null)
                    {
                        Instant.DisplayAlert(this, "L'hôte {0} n'a pas d'addresse IPv4.".FormatInvariant(parts[0]));
                        return null;
                    }

                    address = (IPv4Address)resolvedAddress;
                }
                catch (SocketException)
                {
                    Instant.DisplayAlert(this, "Impossible de résoudre le nom d'hôte {0}.".FormatInvariant(parts[0]));
                    return null;
                }
            }

            ushort port = 0;
            if (parts.Length == 2 && !ushort.TryParse(parts[1], NumberStyles.None, NumberFormatInfo.InvariantInfo, out port))
            {
                Instant.DisplayAlert(this, "Format de port invalide.".FormatInvariant(address));
                return null;
            }

            return new IPv4EndPoint(address, port);
        }

        private void OnPingButtonPressed(Button sender)
        {
            Instant.Prompt(this, "Entrez l'adresse IP à pinger:", Ping);
        }

        private void Ping(string endPointString)
        {
            IPv4EndPoint? endPoint = ParseEndPointWithAlerts(endPointString);
            if (!endPoint.HasValue) return;

            PingPressed.Raise(this, endPoint.Value);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                BackPressed = null;
                HostPressed = null;
                JoinPressed = null;
                JoinByIpPressed = null;
                PingPressed = null;
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}