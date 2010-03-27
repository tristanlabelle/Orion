using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Gui;
using Orion.Engine.Networking;
using Orion.Game.Matchmaking.Networking;
using Orion.Game.Matchmaking;

namespace Orion.Game.Presentation
{
    public abstract class MultiplayerMatchConfigurationUI : MatchConfigurationUI
    {
        #region Fields
        private SafeTransporter transporter;
        protected ListFrame chatMessages;
        #endregion

        #region Constructors
        public MultiplayerMatchConfigurationUI(MatchOptions options, SafeTransporter transporter)
            : base(options)
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
                    .Where(slot => slot.HostEndPoint.HasValue)
                    .Select(slot => slot.HostEndPoint.Value);
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
            ((RemotePlayerSlot)playerSlots[slot].SelectedItem).HostEndPoint = null;
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
            ((RemotePlayerSlot)playerSlots[slot].SelectedItem).HostEndPoint = host;
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

        protected override void Update(float timeDeltaInSeconds)
        {
            transporter.Poll();
            base.Update(timeDeltaInSeconds);
        }
        #endregion
    }
}
