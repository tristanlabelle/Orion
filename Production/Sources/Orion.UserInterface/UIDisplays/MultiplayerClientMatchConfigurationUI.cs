using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Networking;
using Orion.Commandment;
using Orion.UserInterface.Widgets;

namespace Orion.UserInterface
{
    public class MultiplayerClientMatchConfigurationUI : MultiplayerMatchConfigurationUI
    {
        #region Methods
        protected override void InitializeSlots()
        {
            for (int i = 0; i < playerSlots.Length; i++)
            {
                DropdownList<PlayerSlot> slotList = playerSlots[i];
                slotList.AddItem(new AIPlayerSlot());
                slotList.AddItem(new ClosedPlayerSlot());
                slotList.AddItem(new RemotePlayerSlot());
                slotList.SelectedItem = slotList.Items.Last();
                slotList.AddItem(new LocalPlayerSlot());
                slotList.Enabled = false;
            }
            base.InitializeSlots();
        }
        #endregion
    }
}
