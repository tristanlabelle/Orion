using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Gui;
using Orion.Engine;
using Orion.Engine.Gui.Adornments;
using Orion.Game.Matchmaking;

namespace Orion.Game.Presentation.Gui
{
    partial class MatchConfigurationUI
    {
        /// <summary>
        /// Provides a representation of a player in the player list of the match configuration.
        /// </summary>
        private sealed class PlayerRow : ContentControl
        {
            #region Fields
            private readonly Player player;
            private readonly Button kickButton;
            private readonly ComboBox colorComboBox;
            #endregion

            #region Constructors
            public PlayerRow(MatchConfigurationUI ui, Player player)
            {
                Argument.EnsureNotNull(ui, "ui");
                Argument.EnsureNotNull(player, "player");

                Adornment = new ColorAdornment(Colors.Black.ToRgba(0.2f));

                DockLayout dock = new DockLayout();
                Content = dock;

                Label nameLabel = ui.style.CreateLabel(player.Name);
                nameLabel.VerticalAlignment = Alignment.Center;
                nameLabel.MinXMargin = 5;
                dock.Dock(nameLabel, Direction.NegativeX);

                kickButton = ui.style.CreateTextButton("Kick");
                kickButton.VerticalAlignment = Alignment.Center;
                kickButton.MinXMargin = 5;
                kickButton.MaxXMargin = 5;
                kickButton.VisibilityFlag = Visibility.Hidden;
                kickButton.Clicked += (sender, @event) => ui.PlayerKicked.Raise(ui, player);
                dock.Dock(kickButton, Direction.PositiveX);

                colorComboBox = ui.style.Create<ComboBox>();
                colorComboBox.Button.Width = 24;
                colorComboBox.SelectedItemViewport.Width = 60;
                colorComboBox.DropDown.Width = 60;
                colorComboBox.VerticalAlignment = Alignment.Center;

                foreach (ColorRgb color in PlayerSettings.FactionColors)
                {
                    ImageBox colorBox = new ImageBox()
                    {
                        Width = 20,
                        Height = 20,
                        Tint = color,
                        DrawIfNoTexture = true
                    };

                    colorComboBox.Items.Add(colorBox);
                    if (player.Color == color) colorComboBox.SelectedItem = colorBox;
                }

                // This is done after adding the items because the first item added triggers a selection change
                colorComboBox.SelectedItemChanged += sender =>
                {
                    ColorRgb newColor = ((ImageBox)colorComboBox.SelectedItem).Tint;
                    colorComboBox.SelectedItem = colorComboBox.Items.First(item => ((ImageBox)item).Color == player.Color);
                    ui.PlayerColorChanged.Raise(ui, player, newColor);
                };

                dock.Dock(colorComboBox, Direction.PositiveX);

                this.player = player;
                player.NameChanged += sender => nameLabel.Text = player.Name;
                player.ColorChanged += sender =>
                    colorComboBox.SelectedItem = colorComboBox.Items.First(item => ((ImageBox)item).Color == player.Color);
            }
            #endregion

            #region Properties
            /// <summary>
            /// Gets the player displayed by this row.
            /// </summary>
            public Player Player
            {
                get { return player; }
            }

            /// <summary>
            /// Accesses a value indicating if the player can be kicked.
            /// </summary>
            public bool IsKickable
            {
                get { return kickButton.VisibilityFlag == Visibility.Visible; }
                set { kickButton.VisibilityFlag = value ? Visibility.Visible : Visibility.Hidden; }
            }
            #endregion
        }
    }
}
