using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Networking;
using Orion.Commandment;
using Orion.UserInterface.Widgets;

namespace Orion.UserInterface
{
    public sealed class MultiplayerHostMatchConfigurationUI : MultiplayerMatchConfigurationUI
    {
        #region Events

        public event GenericEventHandler<int, PlayerSlot> SlotOccupationChanged;

        #endregion

        #region Methods
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
                slotList.AddItem(new RemotePlayerSlot());
                slotList.SelectedItem = slotList.Items.Last();
                slotList.SelectionChanged += SelectionChanged;
            }
            base.InitializeSlots();
        }

        private void SelectionChanged(DropdownList<PlayerSlot> slot, PlayerSlot value)
        {
            GenericEventHandler<int, PlayerSlot> handler = SlotOccupationChanged;
            if (handler != null)
                handler(playerSlots.IndexOf(slot), value);
        }

        public override void Dispose()
        {
            SlotOccupationChanged = null;
            base.Dispose();
        }
        #endregion
    }
}
