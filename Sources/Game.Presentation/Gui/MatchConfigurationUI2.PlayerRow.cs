﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Gui2;
using Orion.Engine;
using Orion.Engine.Gui2.Adornments;
using Orion.Game.Matchmaking;

namespace Orion.Game.Presentation.Gui
{
    partial class MatchConfigurationUI2
    {
        /// <summary>
        /// Provides a representation of a player in the player list of the match configuration.
        /// </summary>
        private sealed class PlayerRow : ContentControl
        {
            #region Fields
            private readonly Player player;
            private readonly Button kickButton;
            #endregion

            #region Constructors
            public PlayerRow(MatchConfigurationUI2 ui, Player player)
            {
                Argument.EnsureNotNull(ui, "ui");
                Argument.EnsureNotNull(player, "player");

                Adornment = new ColorAdornment(Colors.Black.ToRgba(0.2f));

                DockLayout dock = new DockLayout();
                Content = dock;

                Label nameLabel = ui.style.CreateLabel(player.Name);
                nameLabel.Color = player.Color;
                nameLabel.VerticalAlignment = Alignment.Center;
                nameLabel.MinXMargin = 5;
                dock.Dock(nameLabel, Direction.NegativeX);

                kickButton = ui.style.CreateTextButton("Kick");
                kickButton.VerticalAlignment = Alignment.Center;
                kickButton.MinXMargin = 10;
                kickButton.MaxXMargin = 5;
                kickButton.VisibilityFlag = Visibility.Hidden;
                kickButton.Clicked += (sender, @event) => ui.PlayerKicked.Raise(ui, player);
                dock.Dock(kickButton, Direction.PositiveX);

                this.player = player;
                player.NameChanged += sender => nameLabel.Text = player.Name;
                player.ColorChanged += sender => nameLabel.Color = player.Color;
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
