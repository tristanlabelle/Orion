using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Geometry;
using Orion.Engine.Gui;
using Orion.Game.Matchmaking;

namespace Orion.Game.Presentation
{
    public class SinglePlayerMatchConfigurationUI : MatchConfigurationUI
    {
        public SinglePlayerMatchConfigurationUI(MatchSettings options)
            : base(options)
        { }

        public override void InitializeSlots()
        {
            LocalPlayerSlot you = new LocalPlayerSlot();
            playerSlots[0].AddItem(you);
            playerSlots[0].Enabled = false;

            for (int i = 1; i < playerSlots.Length; i++)
            {
                DropdownList<PlayerSlot> slotList = playerSlots[i];
                slotList.AddItem(new AIPlayerSlot());
                slotList.AddItem(new ClosedPlayerSlot());
                slotList.SelectedItem = slotList.Items.Last();
            }
        }
    }
}
