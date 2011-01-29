using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Gui2;
using Orion.Engine;
using Orion.Game.Simulation;
using Orion.Engine.Gui2.Adornments;

namespace Orion.Game.Presentation.Gui
{
    /// <summary>
    /// A control which allows the user to change his diplomatic state with regard to other factions.
    /// </summary>
    public sealed class DiplomacyPanel2 : ContentControl
    {
        private sealed class FactionRow : DockLayout
        {
            #region Fields
            private readonly Faction otherFaction;
            #endregion

            #region Constructors
            public FactionRow(DiplomacyPanel2 panel, OrionGuiStyle style, Faction localFaction, Faction otherFaction)
            {
                Dock(new ImageBox()
                    {
                        Width = 32,
                        Height = 32,
                        VerticalAlignment = Alignment.Center,
                        MaxXMargin = 5,
                        Tint = otherFaction.Color,
                        DrawIfNoTexture = true
                    }, Direction.NegativeX);

                Label nameLabel = style.CreateLabel(otherFaction.Name);
                nameLabel.VerticalAlignment = Alignment.Center;
                Dock(nameLabel, Direction.NegativeX);

                CheckBox alliedCheckBox = style.Create<CheckBox>();
                alliedCheckBox.VerticalAlignment = Alignment.Center;
                alliedCheckBox.Content = style.CreateLabel("Allié");
                alliedCheckBox.IsChecked = (localFaction.GetDiplomaticStance(otherFaction) & DiplomaticStance.SharedVision) != 0;
                alliedCheckBox.StateChanged += sender =>
                {
                    DiplomaticStance diplomaticStance = alliedCheckBox.IsChecked
                        ? DiplomaticStance.SharedVision | DiplomaticStance.AlliedVictory
                        : DiplomaticStance.Enemy;
                    panel.StanceChanged.Raise(panel, otherFaction, diplomaticStance);
                };

                Dock(alliedCheckBox, Direction.PositiveX);

                this.otherFaction = otherFaction;
            }
            #endregion

            #region Properties
            public Faction Faction
            {
                get { return otherFaction; }
            }
            #endregion
        }

        #region Constructors
        public DiplomacyPanel2(OrionGuiStyle style, Faction faction)
        {
            Argument.EnsureNotNull(style, "style");
            Argument.EnsureNotNull(faction, "faction");

            StackLayout stack = new StackLayout()
            {
                Direction = Direction.PositiveY,
                ChildGap = 10,
            };

            stack.Stack(style.CreateLabel("Autres factions:"));

            foreach (Faction otherFaction in faction.World.Factions)
            {
                if (otherFaction == faction) continue;
                stack.Stack(new FactionRow(this, style, faction, otherFaction));
            }

            Button closeButton = style.CreateTextButton("Accepter");
            closeButton.HorizontalAlignment = Alignment.Positive;
            closeButton.Clicked += (sender, @event) => Closed.Raise(this);
            stack.Stack(closeButton);

            faction.World.FactionDefeated += (sender, defeatedFaction) =>
            {
                stack.Children.Remove(stack.Children.First(child => ((FactionRow)child).Faction == defeatedFaction));
            };

            Adornment = new ColorAdornment(Colors.Gray);
            MinWidth = 300;
            Padding = 10;
            Content = stack;
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when the diplomatic stance of this faction with regard to other factions changes.
        /// </summary>
        public event Action<DiplomacyPanel2, Faction, DiplomaticStance> StanceChanged;

        /// <summary>
        /// Raised when the diplomacy panel gets closed by the user.
        /// </summary>
        public event Action<DiplomacyPanel2> Closed;
        #endregion
    }
}
