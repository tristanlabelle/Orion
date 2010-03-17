using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Collections;
using Orion.Engine.Networking;
using Orion.Networking;
using Orion.Matchmaking;
using Orion.UserInterface.Widgets;

namespace Orion.UserInterface
{
    public sealed class MultiplayerHostMatchConfigurationUI : MultiplayerMatchConfigurationUI
    {
        #region Constructors
        public MultiplayerHostMatchConfigurationUI(SafeTransporter transporter)
            : base(transporter)
        { }
        #endregion

        #region Events

        public event Action<int, PlayerSlot> SlotOccupationChanged;
        public event Action<IPv4EndPoint> KickedPlayer;

        #endregion

        #region Methods
        protected override void InitializeSlots()
        {
            LocalPlayerSlot you = new LocalPlayerSlot();
            playerSlots[0].AddItem(you);
            playerSlots[0].Enabled = false;

            for (int i = 1; i < playerSlots.Length; i++)
            {
                DropdownList<PlayerSlot> dropdownList = playerSlots[i];
                dropdownList.AddItem(new AIPlayerSlot());
                dropdownList.AddItem(new ClosedPlayerSlot());
                dropdownList.AddItem(new RemotePlayerSlot());
                dropdownList.SelectedItem = dropdownList.Items.Last();
                dropdownList.SelectionChanged += SelectionChanged;
            }
        }

        private void SelectionChanged(DropdownList<PlayerSlot> slot, PlayerSlot value)
        {
            RemotePlayerSlot remoteSlot = slot.Items.OfType<RemotePlayerSlot>().First();
            if (remoteSlot.HostEndPoint.HasValue)
            {
                Action<IPv4EndPoint> kickHandler = KickedPlayer;
                if (kickHandler != null) kickHandler(remoteSlot.HostEndPoint.Value);
                remoteSlot.HostEndPoint = null;
            }
            Action<int, PlayerSlot> handler = SlotOccupationChanged;
            if (handler != null) handler(playerSlots.IndexOf(slot), value);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) SlotOccupationChanged = null;

            base.Dispose(disposing);
        }
        #endregion
    }
}
