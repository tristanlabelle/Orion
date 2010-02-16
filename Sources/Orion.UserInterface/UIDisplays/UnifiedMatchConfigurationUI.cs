using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Math;
using Orion.UserInterface.Widgets;
using Orion.Matchmaking;
using Orion.Geometry;
using Orion.Graphics;

namespace Orion.UserInterface.UIDisplays
{
    public class UnifiedMatchConfigurationUI : UIDisplay
    {
        #region Nested Types
        public class PlayerRow : Frame
        {
            #region Fields
            private readonly UnifiedMatchConfigurationUI configurationUi;
            private readonly Label nameLabel;
            private readonly DropdownList<ColorRgb> colors;
            #endregion

            #region Constructors
            public PlayerRow(UnifiedMatchConfigurationUI ui, IEnumerable<ColorRgb> colorItems, string initialLabel, bool deletable)
                : base(ui.rowRectangle)
            {
                configurationUi = ui;
                Rectangle labelRect = Instant.CreateComponentRectangle(Bounds, new Vector2(0, 0), new Vector2(0.5f, 1));
                nameLabel = new Label(labelRect, initialLabel);
                Children.Add(nameLabel);

                Rectangle dropdownRow = Instant.CreateComponentRectangle(Bounds, new Vector2(0.6f, 0), new Vector2(0.7f, 1));
                colors = new DropdownList<ColorRgb>(dropdownRow, new DropdownListRowColorRenderer());
                Children.Add(colors);
                colors.SelectionChanged += SelectionChanged;

                foreach (ColorRgb color in colorItems)
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
            public event GenericEventHandler<PlayerRow, ColorRgb> SelectedColorChanged;
            public event GenericEventHandler<PlayerRow> RowSuppressed;
            #endregion

            #region Properties
            public DropdownList<ColorRgb> ColorList
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
            private void SelectionChanged(DropdownList<ColorRgb> list, ColorRgb color)
            {
                if (SelectedColorChanged != null) SelectedColorChanged(this, color);
            }

            private void SuppressRow(Button sender)
            {
                if (RowSuppressed != null) RowSuppressed(this);
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    SelectedColorChanged = null;
                    RowSuppressed = null;
                }

                base.Dispose(disposing);
            }
            #endregion
        }
        #endregion

        #region Fields
        private Rectangle rowRectangle;
        protected Button startButton;
        protected Button exitButton;

        private ListFrame youListFrame;
        #endregion

        #region Constructors
        public UnifiedMatchConfigurationUI(IEnumerable<ColorRgb> colors, bool displayStartButton)
        {
            Vector2 padding = new Vector2(10, 10);

            Rectangle mainFrameRect = Instant.CreateComponentRectangle(Bounds, new Vector2(0.05f, 0.3f), new Vector2(0.95f, 0.8f));
            ListFrame mainFrame = new ListFrame(mainFrameRect, padding);

            rowRectangle = Instant.CreateComponentRectangle(mainFrameRect, new Vector2(0, 0), new Vector2(1, 0.1f));

            Rectangle youFrameRect = Instant.CreateComponentRectangle(mainFrameRect, new Vector2(0, 0f), new Vector2(1, 0.1f));
            youListFrame = new ListFrame(youFrameRect, padding);
            Label youLabel = new Label("You");
            youListFrame.Children.Add(youLabel);

            PlayerRow youRow = new PlayerRow(this, colors, "You", false);
            youListFrame.Children.Add(youRow);
        }
        #endregion

        #region Methods

        #endregion
    }
}
