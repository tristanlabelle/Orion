using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Networking;
using Orion.Matchmaking;
using Orion.UserInterface.Widgets;

namespace Orion.UserInterface
{
    public abstract class MultiplayerMatchConfigurationUI : MatchConfigurationUI
    {
        #region Fields
        private SafeTransporter transporter;
        protected ListFrame chatMessages;
        #endregion

        #region Constructors
        public MultiplayerMatchConfigurationUI(SafeTransporter transporter)
        {
            this.transporter = transporter;
        }
        #endregion

        #region Properties
        public IEnumerable<IPv4EndPoint> PlayerAddresses
        {
            get
            {
                return Players.OfType<RemotePlayerSlot>()
                    .Where(slot => slot.RemoteHost.HasValue)
                    .Select(slot => slot.RemoteHost.Value);
            }
        }
        #endregion

        #region Events
        public event Action<int, PlayerSlot> SlotChanged;
        #endregion

        #region Methods
        public void OpenSlot(int slot)
        {
            SelectSlot<RemotePlayerSlot>(slot);
            ((RemotePlayerSlot)playerSlots[slot].SelectedItem).RemoteHost = null;
        }

        public void CloseSlot(int slot)
        {
            SelectSlot<ClosedPlayerSlot>(slot);
        }

        public void SetLocalPlayerForSlot(int slot)
        {
            SelectSlot<LocalPlayerSlot>(slot);
        }

        public void UseAIForSlot(int slot)
        {
            SelectSlot<AIPlayerSlot>(slot);
        }

        public void UsePlayerForSlot(int slot, IPv4EndPoint host)
        {
            SelectSlot<RemotePlayerSlot>(slot);
            ((RemotePlayerSlot)playerSlots[slot].SelectedItem).RemoteHost = host;
        }

        private void SelectionChanged(DropdownList<PlayerSlot> sender, PlayerSlot newValue)
        {
            Action<int, PlayerSlot> handler = SlotChanged;
            if(handler != null)
                handler(Array.IndexOf(playerSlots, sender), newValue);
        }

        private void SelectSlot<T>(int slot) where T : PlayerSlot
        {
            playerSlots[slot].SelectedItem = playerSlots[slot].Items.OfType<T>().First();
        }

        protected override void OnUpdate(UpdateEventArgs args)
        {
            transporter.Poll();
            base.OnUpdate(args);
        }
        #endregion
    }
}
