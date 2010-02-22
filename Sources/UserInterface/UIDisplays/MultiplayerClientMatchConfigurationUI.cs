using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Networking;
using Orion.Networking;
using Orion.Matchmaking;
using Orion.UserInterface.Widgets;

namespace Orion.UserInterface
{
    public class MultiplayerClientMatchConfigurationUI : MultiplayerMatchConfigurationUI
    {
        #region Constructors
        public MultiplayerClientMatchConfigurationUI(SafeTransporter transporter)
            : base(transporter)
        {
            Children.Remove(startButton);
            Children.Remove(sizeChangeButton);
        }
        #endregion

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
        }
        #endregion
    }
}
