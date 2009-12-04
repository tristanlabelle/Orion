using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Color = System.Drawing.Color;
using OpenTK.Math;
using Orion.UserInterface.Widgets;
using Orion.Commandment;
using Orion.Geometry;
using Orion.Graphics;

namespace Orion.UserInterface
{
    public class UnifiedMatchConfigurationUI : UIDisplay
    {
        #region Nested Types
        private class PlayerRow : Frame
        {
            #region Fields
            private readonly UnifiedMatchConfigurationUI configurationUi;
            private readonly Label nameLabel;
            private readonly DropdownList<Color> colors;
            #endregion

            #region Constructors
            public PlayerRow(UnifiedMatchConfigurationUI ui, IEnumerable<Color> colorItems, string initialLabel, bool deletable)
                : base(ui.rowRectangle)
            {
                configurationUi = ui;
                Rectangle labelRect = Instant.CreateComponentRectangle(Bounds, new Vector2(0.05f, 0), new Vector2(0.5f, 1));
                nameLabel = new Label(labelRect, initialLabel);
                Children.Add(nameLabel);

                Rectangle dropdownRow = Instant.CreateComponentRectangle(Bounds, new Vector2(0.6f, 0), new Vector2(0.7f, 1));
                colors = new DropdownList<Color>(dropdownRow, new DropdownListRowColorRenderer());
                Children.Add(colors);
                colors.SelectionChanged += SelectionChanged;

                foreach (Color color in colorItems)
                    colors.AddItem(color);

                if (deletable)
                {
                    Rectangle deleteButtonRect = Instant.CreateComponentRectangle(Bounds, new Vector2(0.9f, 0), new Vector2(1, 1));
                    Button deleteButton = new Button(deleteButtonRect, "X");
                    deleteButton.Triggered += SuppressRow;
                    Children.Add(deleteButton);
                }
            }
            #endregion

            #region Events
            public event GenericEventHandler<PlayerRow, Color> SelectedColorChanged;
            public event GenericEventHandler<PlayerRow> RowSuppressed;
            #endregion

            #region Properties
            public DropdownList<Color> ColorList
            {
                get { return colors; }
            }

            public string Name
            {
                get { return nameLabel.Text.Value; }
                set { nameLabel.Text = new Text(value); }
            }
            #endregion

            #region Methods
            private void SelectionChanged(DropdownList<Color> list, Color color)
            {
                if (SelectedColorChanged != null) SelectedColorChanged(this, color);
            }

            private void SuppressRow(Button sender)
            {
                if (RowSuppressed != null) RowSuppressed(this);
            }

            public override void Dispose()
            {
                SelectedColorChanged = null;
                RowSuppressed = null;
                base.Dispose();
            }
            #endregion
        }
        #endregion

        #region Fields
        private static readonly Vector2 padding = new Vector2(10, 10);
        private Rectangle rowRectangle;
        private readonly bool isDecidor;
        private readonly Button startButton;
        private readonly Button exitButton;

        private readonly ListFrame playersListFrame;
        private readonly Dictionary<PlayerRow, PlayerSlot> playersMapping = new Dictionary<PlayerRow, PlayerSlot>();
        private readonly IEnumerable<Color> colors;
        #endregion

        #region Constructors
        public UnifiedMatchConfigurationUI(IEnumerable<Color> colors, bool isDecidor, bool displayRemotePlayers)
        {
            this.colors = colors;
            this.isDecidor = isDecidor;

            Rectangle mainFrameRect = Instant.CreateComponentRectangle(Bounds, new Vector2(0.025f, 0.3f), new Vector2(0.8f, 0.975f));
            playersListFrame = new ListFrame(mainFrameRect, padding);
            Children.Add(playersListFrame);

            rowRectangle = Instant.CreateComponentRectangle(playersListFrame.Bounds, new Vector2(0.025f, 0), new Vector2(0.975f, 0.06f));

            Rectangle exitButtonRect = Instant.CreateComponentRectangle(Bounds, new Vector2(0.825f, 0.3f), new Vector2(0.975f, 0.35f));
            exitButton = new Button(exitButtonRect, "Back");
            exitButton.Triggered += OnExit;
            Children.Add(exitButton);

            if (isDecidor)
            {
                Rectangle startButtonRect = Instant.CreateComponentRectangle(Bounds, new Vector2(0.825f, 0.375f), new Vector2(0.975f, 0.425f));
                startButton = new Button(startButtonRect, "Start");
                startButton.Triggered += OnStart;
                Children.Add(startButton);

                Rectangle addAIRect = Instant.CreateComponentRectangle(Bounds, new Vector2(0.825f, 0.925f), new Vector2(0.975f, 0.975f));
                Button addAIButton = new Button(addAIRect, "Add Computer");
                addAIButton.Triggered += AddComputerPlayer;
                Children.Add(addAIButton);

                if (displayRemotePlayers)
                {
                    Rectangle addPlayerRect = Instant.CreateComponentRectangle(Bounds, new Vector2(0.825f, 0.85f), new Vector2(0.975f, 0.9f));
                    Button addPlayerButton = new Button(addAIRect, "Add Player");
                    addPlayerButton.Triggered += AddRemotePlayer;
                    Children.Add(addPlayerButton);
                }
            }
        }
        #endregion

        #region Events
        public event GenericEventHandler<PlayerSlot> SlotDeleted;
        public event GenericEventHandler<UnifiedMatchConfigurationUI> PressedAddComputer;
        public event GenericEventHandler<UnifiedMatchConfigurationUI> PressedAddPlayer;
        public event GenericEventHandler<UnifiedMatchConfigurationUI> PressedExit;
        public event GenericEventHandler<UnifiedMatchConfigurationUI> PressedStartGame;
        #endregion

        #region Methods

        public void AddPlayer(string name, PlayerSlot slot)
        {
            Argument.EnsureLower(playersMapping.Count, colors.Count(), "playersMapping");
            PlayerRow row = new PlayerRow(this, colors, name, isDecidor);
            row.SelectedColorChanged += OnChangedSelectedColor;
            row.RowSuppressed += OnDeletedSlot;
            playersMapping[row] = slot;
            playersListFrame.Children.Add(row);
        }

        public void SetColor(PlayerSlot slot, Color color)
        {

        }

        private void AddComputerPlayer(Button sender)
        {
            if (PressedAddComputer != null)
                PressedAddComputer(this);
        }

        private void AddRemotePlayer(Button sender)
        {
            if (PressedAddPlayer != null)
                PressedAddPlayer(this);
        }

        private void OnExit(Button sender)
        {
            if (PressedExit != null)
                PressedExit(this);
        }

        private void OnStart(Button sender)
        {
            if (PressedStartGame != null)
                PressedStartGame(this);
        }

        private void OnChangedSelectedColor(PlayerRow row, Color newColor)
        {
            SetColor(playersMapping[row], newColor);
        }

        private void OnDeletedSlot(PlayerRow row)
        {
            if (SlotDeleted != null)
                SlotDeleted(playersMapping[row]);
            playersMapping.Remove(row);
        }

        public override void Dispose()
        {
            SlotDeleted = null;
            PressedAddPlayer = null;
            PressedAddComputer = null;
            PressedExit = null;
            PressedStartGame = null;
            base.Dispose();
        }

        #endregion
    }
}
