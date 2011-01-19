using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Orion.Game.Matchmaking;
using Orion.Engine;

namespace Orion.Game.Presentation.Gui
{
    partial class MatchConfigurationUI2
    {
        public sealed class PlayerCollection : Collection<Player>
        {
            #region Fields
            private readonly MatchConfigurationUI2 ui;
            #endregion

            #region Constructors
            internal PlayerCollection(MatchConfigurationUI2 ui)
            {
                Argument.EnsureNotNull(ui, "ui");

                this.ui = ui;
            }
            #endregion

            #region Methods
            public void Add(Player player, bool isKickable)
            {
                Add(player);

                var row = (PlayerRow)ui.playerStack.Children[ui.playerStack.Children.Count - 1];
                row.IsKickable = true;
            }

            protected override void InsertItem(int index, Player item)
            {
                PlayerRow row = new PlayerRow(ui, item);
                ui.playerStack.Children.Insert(index, row);
                base.InsertItem(index, item);
            }

            protected override void RemoveItem(int index)
            {
                ui.playerStack.Children.RemoveAt(index);
                base.RemoveItem(index);
            }

            protected override void ClearItems()
            {
                while (Count > 0) RemoveItem(Count - 1);
            }

            protected override void SetItem(int index, Player item)
            {
                throw new NotSupportedException();
            }
            #endregion

        }
    }
}
