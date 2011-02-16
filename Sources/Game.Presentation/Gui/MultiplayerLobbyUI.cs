using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Gui;
using Orion.Engine;
using Orion.Engine.Gui.Adornments;
using Orion.Engine.Networking;
using Orion.Game.Matchmaking.Networking;
using Orion.Engine.Localization;

namespace Orion.Game.Presentation.Gui
{
    public sealed class MultiplayerLobbyUI : ContentControl
    {
        #region Fields
        private readonly GameGraphics graphics;
        private readonly Localizer localizer;
        private TextField playerNameTextField;
        private ListBox matchListBox;
        private TextField ipEndPointTextField;
        private Button joinIPButton, pingIPButton;
        private TextField createdGameNameTextField;
        #endregion

        #region Constructors
        public MultiplayerLobbyUI(GameGraphics graphics, Localizer localizer)
        {
            Argument.EnsureNotNull(graphics, "graphics");
            Argument.EnsureNotNull(localizer, "localizer");

            this.graphics = graphics;
            this.localizer = localizer;

            Adornment = new TextureAdornment(graphics.GetGuiTexture("Granite")) { IsTiling = true };
            Padding = 5;

            DockLayout dock = new DockLayout()
            {
                LastChildFill = true
            };

            dock.Dock(CreatePlayerNameControls(), Direction.NegativeY);

            Label matchesLabel = graphics.GuiStyle.CreateLabel(localizer.GetNoun("Matches"));
            matchesLabel.MaxYMargin = 5;
            dock.Dock(matchesLabel, Direction.NegativeY);

            Button backButton = graphics.GuiStyle.CreateTextButton(localizer.GetNoun("Back"));
            backButton.HorizontalAlignment = Alignment.Negative;
            backButton.MinSize = new Size(150, 40);
            backButton.MinYMargin = 5;
            backButton.Clicked += (sender, @event) => Exited.Raise(this);
            dock.Dock(backButton, Direction.PositiveY);

            dock.Dock(CreateHostControls(), Direction.PositiveY);
            dock.Dock(CreateIPControls(), Direction.PositiveY);

            matchListBox = graphics.GuiStyle.Create<ListBox>();
            matchListBox.Adornment = new ColorAdornment(Colors.Black.ToRgba(0.2f));
            matchListBox.SelectionChanged += sender => LaunchSelectedMatch();
            dock.Dock(matchListBox, Direction.NegativeY);

            Content = dock;
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when the user exits the screen.
        /// </summary>
        public event Action<MultiplayerLobbyUI> Exited;

        /// <summary>
        /// Raised when the user joins an advertized match.
        /// </summary>
        public event Action<MultiplayerLobbyUI, AdvertizedMatch> Joined;

        /// <summary>
        /// Raised when the user chooses to host a game.
        /// </summary>
        public event Action<MultiplayerLobbyUI, string> Hosted;

        /// <summary>
        /// Raised when the user joins a game at a specific IP end point.
        /// </summary>
        public event Action<MultiplayerLobbyUI, IPv4EndPoint> JoinedByIP;

        /// <summary>
        /// Raised when the user pings a specific IP end point.
        /// </summary>
        public event Action<MultiplayerLobbyUI, IPv4EndPoint> PingedIP;
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the name the user has chosen for his player.
        /// </summary>
        public string PlayerName
        {
            get { return playerNameTextField.Text; }
            set { playerNameTextField.Text = value; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Clears all displayed matches.
        /// </summary>
        public void ClearMatches()
        {
            matchListBox.Items.Clear();
        }

        /// <summary>
        /// Adds an advertized match to the list of displayed matches.
        /// </summary>
        /// <param name="match">The match to be added.</param>
        public void AddMatch(AdvertizedMatch match)
        {
            Argument.EnsureNotNull(match, "match");

#warning TODO: Localize this properly:
            string text = "[{0}] {1} ({2} places restantes)".FormatInvariant(
                match.Source.Tag, match.Name, match.OpenSlotCount);
            Label label = graphics.GuiStyle.CreateLabel(text);
            label.Tag = match;
            matchListBox.Items.Add(label);
        }

        private Control CreatePlayerNameControls()
        {
            StackLayout stack = new StackLayout()
            {
                Direction = Direction.PositiveX,
                ChildGap = 5,
                MaxYMargin = 5
            };

            Label label = graphics.GuiStyle.CreateLabel(localizer.GetNoun("PlayerName"));
            label.VerticalAlignment = Alignment.Center;
            stack.Stack(label);

            playerNameTextField = graphics.GuiStyle.Create<TextField>();
            playerNameTextField.Width = 300;
            stack.Stack(playerNameTextField);

            return stack;
        }

        private Control CreateHostControls()
        {
            StackLayout stack = new StackLayout()
            {
                Direction = Direction.PositiveX,
                ChildGap = 5,
                MinYMargin = 5
            };

            Label label = graphics.GuiStyle.CreateLabel(localizer.GetNoun("HostGameNamed"));
            label.VerticalAlignment = Alignment.Center;
            stack.Stack(label);

            createdGameNameTextField = graphics.GuiStyle.Create<TextField>();
            createdGameNameTextField.Width = 300;
            createdGameNameTextField.VerticalAlignment = Alignment.Center;
            stack.Stack(createdGameNameTextField);

            Button createGameButton = graphics.GuiStyle.CreateTextButton(localizer.GetNoun("Create"));
            createGameButton.Clicked += (sender, @event) => Hosted.Raise(this, createdGameNameTextField.Text);
            stack.Stack(createGameButton);

            return stack;
        }

        private Control CreateIPControls()
        {
            StackLayout stack = new StackLayout()
            {
                Direction = Direction.PositiveX,
                ChildGap = 5,
                MinYMargin = 5
            };

            Label label = graphics.GuiStyle.CreateLabel(localizer.GetNoun("JoinByIP"));
            label.VerticalAlignment = Alignment.Center;
            stack.Stack(label);

            ipEndPointTextField = graphics.GuiStyle.Create<TextField>();
            ipEndPointTextField.Width = 200;
            ipEndPointTextField.VerticalAlignment = Alignment.Center;
            ipEndPointTextField.TextChanged += OnIPEndPointTextChanged;
            stack.Stack(ipEndPointTextField);

            joinIPButton = graphics.GuiStyle.CreateTextButton(localizer.GetNoun("Join"));
            joinIPButton.HasEnabledFlag = false;
            joinIPButton.Clicked += (sender, @event) => JoinByIP();
            stack.Stack(joinIPButton);

            pingIPButton = graphics.GuiStyle.CreateTextButton(localizer.GetNoun("Ping"));
            pingIPButton.HasEnabledFlag = false;
            pingIPButton.Clicked += (sender, @event) => JoinByIP();
            stack.Stack(pingIPButton);

            return stack;
        }

        private void LaunchSelectedMatch()
        {
            Label selectedMatchLabel = (Label)matchListBox.SelectedItem;
            if (selectedMatchLabel == null) return;

            AdvertizedMatch match = (AdvertizedMatch)selectedMatchLabel.Tag;
            Joined.Raise(this, match);
        }

        private void OnIPEndPointTextChanged(TextField sender)
        {
            bool isValidFormat = IPv4EndPoint.TryParse(sender.Text).HasValue;
            joinIPButton.HasEnabledFlag = isValidFormat;
            pingIPButton.HasEnabledFlag = isValidFormat;
        }

        private void JoinByIP()
        {
            IPv4EndPoint endPoint = IPv4EndPoint.Parse(ipEndPointTextField.Text);
            JoinedByIP.Raise(this, endPoint);
        }

        private void PingIP()
        {
            IPv4EndPoint endPoint = IPv4EndPoint.Parse(ipEndPointTextField.Text);
            PingedIP.Raise(this, endPoint);
        }
        #endregion
    }
}
