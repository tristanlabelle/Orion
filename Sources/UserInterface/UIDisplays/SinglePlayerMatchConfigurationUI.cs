using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Geometry;
using Orion.Engine.Gui;
using Orion.Matchmaking;

namespace Orion.UserInterface
{
    public class SinglePlayerMatchConfigurationUI : MatchConfigurationUI
    {
        public SinglePlayerMatchConfigurationUI()
        { }

        protected override void InitializeSlots()
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
